using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PersonVisual
{
	public string id;
	GameObject footprintGenerator;
	public GameObject phone;
	private Vector3 velocity = Vector3.zero;
	private bool lastVisible = false;

	PersonVisual (PersonData p, GameObject go_footprints, GameObject go_phone)
	{
		// Create a new object, but doesn't set the position. 
		// This is handled later by updateVisual().
		this.id = p.id;
		this.footprintGenerator = go_footprints;
		this.phone = go_phone;
		this.phone.name = "Phone." + this.id;
	}

	// Convenience
	public static PersonVisual CreatePersonVisual (PersonData p, GameObject go_footprints, GameObject go_phone)
	{
		return new PersonVisual (p, go_footprints, go_phone);
	}

	public void updateVisual (PersonData person)
	{
		// FOOTPRINTS
		// Constrain the position on the height 
		// Vector3 is a struct, so assigning it creates a copy.
		// This way we don't modify the original data in PersonData.
		Vector3 constrainedPosition = person.pos;
		// Check the anchor image position and put the footprint generator at this position
		constrainedPosition.y = 0f;
		footprintGenerator.transform.position = constrainedPosition;

		// Constrain the rotation on irrelevant axis
		Vector3 constrainedRotation = person.rot;
		constrainedRotation.x = 0;
		constrainedRotation.z = 0;
		footprintGenerator.transform.rotation = Quaternion.Euler (constrainedRotation);

		// Color
		FootprintsGenerator script = footprintGenerator.GetComponent<FootprintsGenerator> ();
		if (script == null) {
			Debug.Log ("Couldn't find FootprintsGenerator in GameObject");
		} else {
			script.ChangeColor (person.lastInteraction > 0 ? C.APP_COLOR_DARK : C.DEFAULT_FOOTPRINT_COLOR);
		}

		// PHONE
		// Only show the phone if we're close enough
		if (person.distance < 2.5f) {
			if (lastVisible) {
				// Smooth the motion.
				phone.transform.position = Vector3.SmoothDamp (phone.transform.position, person.pos, ref velocity, 0.1f);
			} else {
				// Move the phone directly upon making it visible
				enableRenderers (true);
				phone.transform.position = person.pos;
			}
			phone.transform.rotation = Quaternion.Euler (person.rot);
			lastVisible = true;
		} else {
			if (lastVisible) {
				enableRenderers (false);
			}
			lastVisible = false;
		}
	}

	public void enableRenderers(bool enabled) {
		Renderer[] renderers = phone.GetComponentsInChildren<Renderer> ();
		foreach (Renderer r in renderers) {
			r.enabled = enabled;
		}
	}

	public void onDestroy (AudioClip soundLeave)
	{
		Object.Destroy (this.footprintGenerator);
		// Make the object invisible and play a short sound before destroying it.
		phone.GetComponentInChildren<Renderer> ().enabled = false;
		phone.GetComponent<AudioSource> ().PlayOneShot (soundLeave);
		Object.Destroy (this.phone, 1.5f);
	}
}


public class PersonVisualManager : MonoBehaviour
{
	public GameObject footprintsPrefab;
	public GameObject phonePrefab;

	Dictionary<string, PersonVisual> visuals = new Dictionary<string, PersonVisual> ();

	public AudioClip audioOnAppear;
	public AudioClip audioOnLeave;
	float enabledAt;

	public DatabasePeople data;


	void Start ()
	{
		enabledAt = Time.time;
	}

	// Update is called once per frame
	void Update ()
	{
		/*
		// For debug purposes 
		if (Time.frameCount % 30 == 0) {
			Debug.Log (data.persons.Count);
		}
		*/

		// Check for persons entering the room
		foreach (KeyValuePair<string,PersonData> personData in data.persons) {
			// Instantiate their objects if needed
			if (!visuals.ContainsKey (personData.Key)) {
				onPersonArrival (personData.Value);
			}
		}

		// Check for persons leaving the room.
		List<string> toRemove = new List<string> ();
		foreach (KeyValuePair<string,PersonVisual> visual in visuals) {
			if (!data.persons.ContainsKey (visual.Key)) {
				toRemove.Add (visual.Key);
			}
		}
		foreach (string key in toRemove) {
			onPersonLeave (key);
		}

		// Update the position of every footprint visual.
		foreach (KeyValuePair<string,PersonData> personData in data.persons) {
			visuals [personData.Key].updateVisual (personData.Value);
		}
	}

	void onPersonArrival (PersonData p)
	{
		Debug.Log ("Someone arrived!");
		Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", p.pos.x, p.pos.y, p.pos.z));

		GameObject go_footprints = Instantiate (footprintsPrefab, p.pos, Quaternion.Euler (p.rot));
		GameObject go_phone = Instantiate (phonePrefab, p.pos, Quaternion.Euler (p.rot));
		PersonVisual visual = PersonVisual.CreatePersonVisual (p, go_footprints, go_phone);
		// Play sound, but not for the first wave of initial messages.
		if (enabledAt + 3f < Time.time) {
			visual.phone.GetComponent<AudioSource> ().PlayOneShot (audioOnAppear);
		}
		visuals.Add (p.id, visual);
	}

	void onPersonLeave (string id)
	{
		Debug.Log ("Someone left!");

		visuals [id].onDestroy (audioOnLeave);
		visuals.Remove (id);
	}
}


