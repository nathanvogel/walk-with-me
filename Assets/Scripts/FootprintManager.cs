using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PersonVisual
{
	public string id;
	GameObject footprintGenerator;

	PersonVisual (PersonData p, GameObject gameObject)
	{
		// Create a new object, but doesn't set the position. 
		// This is handled later by updateVisual().
		this.id = p.id;
		this.footprintGenerator = gameObject;
	}

	// Convenience
	public static PersonVisual CreatePersonVisual (PersonData p, GameObject go)
	{
		return new PersonVisual (p, go);
	}

	public void updateVisual (PersonData person)
	{
		// Constrain the position on the height 
		// Vector3 is a struct, so assigning it creates a copy.
		// This way we don't modify the original data in PersonData.
		Vector3 constrainedPosition = person.pos;
		// Check the anchor image position and put the footprint generator slightly
		// above the ground (in case we do collisions, but I disabled gravity now.)
		if (!FindWorldOrigin.yPos.Equals (null)) {
			constrainedPosition.y = FindWorldOrigin.yPos + 0.04f;
//			Debug.Log ("Pass Anchor position to objects");
//			Debug.Log (string.Format ("y:{0:0.######}", constrainedPosition.y));
		} else {
			constrainedPosition.y = 0.04f;
		}
		footprintGenerator.transform.position = constrainedPosition;

		// Constrain the rotation on irrelevant axis
		Vector3 constrainedRotation = person.rot;
		constrainedRotation.x = 0;
		constrainedRotation.z = 0;
		footprintGenerator.transform.rotation = Quaternion.Euler (constrainedRotation);
	}

	public void onDestroy ()
	{
		Object.Destroy (this.footprintGenerator);
	}
}


public class FootprintManager : MonoBehaviour
{
	public GameObject footprintsPrefab;

	Dictionary<string, PersonVisual> visuals = new Dictionary<string, PersonVisual> ();


	public DatabaseConnection data;

	void Start ()
	{
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
		foreach (KeyValuePair<string,PersonVisual> visual in visuals) {
			if (!data.persons.ContainsKey (visual.Key)) {
				onPersonLeave (visual.Key);
			}
		}

		// Update the position of every footprint visual.
		foreach (KeyValuePair<string,PersonData> personData in data.persons) {
			visuals [personData.Key].updateVisual (personData.Value);
		}

		// Get the distance between me and the other objects
		foreach (KeyValuePair<string,PersonData> person in data.persons) {
			proximityDetect (Camera.main.transform.position, person.Value);
		}
	}

	void onPersonArrival (PersonData p)
	{
		Debug.Log ("A new person arrived!");
		Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", p.pos.x, p.pos.y, p.pos.z));

		GameObject go = Instantiate (footprintsPrefab, p.pos, Quaternion.Euler (p.rot));
		PersonVisual visual = PersonVisual.CreatePersonVisual (p, go);
		visuals.Add (p.id, visual);
	}

	void onPersonLeave (string id)
	{
		Debug.Log ("A person left!");

		visuals [id].onDestroy ();
		visuals.Remove (id);
	}

	bool proximityDetect (Vector3 mainPos, PersonData p)
	{
		float dist = Vector3.Distance (mainPos, p.pos);
		//Debug.Log ("Distance to other: " + dist);

		if (dist < 0.5) {
			Debug.Log ("A person is approaching!");

			//Need to add interaction

			return true;
		} else {
			return false;
		}
	}

	void destruction ()
	{
		Destroy (this.gameObject); 

	}
}


