using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PersonVisual
{
	public string id;
	GameObject phone;

	PersonVisual (PersonData p, GameObject go)
	{
		this.id = p.id;
		this.phone = go;
	}

	// Convenience
	public static PersonVisual CreatePersonVisual (PersonData p, GameObject go)
	{
		return new PersonVisual (p, go);
	}

	public void update (PersonData person)
	{
		/* smooth interpolation attempt
		phone.transform.position = Vector3.MoveTowards (
			phone.transform.position, person.pos, 0.3f
		);

		phone.transform.rotation = Quaternion.RotateTowards (
			phone.transform.rotation, Quaternion.Euler(person.rot), 0.3f
		);
		*/

		phone.transform.position = person.pos;
		phone.transform.rotation = Quaternion.Euler (person.rot);
	}

	public void onDestroy() {
		Object.Destroy (this.phone);
	}
}


public class FootprintManager : MonoBehaviour
{
	public GameObject cubePrefab;

	Dictionary<string, PersonVisual> phones = new Dictionary<string, PersonVisual> ();


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

		foreach (KeyValuePair<string,PersonData> person in data.persons) {
			// Check for persons arriving in the room
			// Instantiate their objects if needed
			if (!phones.ContainsKey (person.Key)) {
				onPersonArrival (person.Value);
			}

			phones [person.Key].update (person.Value);
		}

		// Check for persons leaving the room.
		foreach (KeyValuePair<string,PersonVisual> phone in phones) {
			if (!data.persons.ContainsKey (phone.Key)) {
				onPersonLeave (phone.Key);
			}
		}

		// Need to adjust this part of the code to draw the footprint at the position instead of overwriting it
		foreach (KeyValuePair<string,PersonData> person in data.persons) {
			if (phones.ContainsKey (person.Key)) {
				planeUpdate (person.Value);
			}
		}

		// Get the distance between me and the other objects
		foreach (KeyValuePair<string,PersonData> person in data.persons) {
			proximityDetect(Camera.main.transform.position, person.Value);
		}
	}

	void onPersonArrival (PersonData p)
	{
		// Find the position of the empty object

		// Need to adjust this part too as it fixes the rotation
		p.rot.x = 90;
		p.rot.y = 0;

		GameObject go = Instantiate (cubePrefab, p.pos, Quaternion.Euler (p.rot));
		Debug.Log ("A new person arrived!");
		Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", p.pos.x, p.pos.y, p.pos.z));
		PersonVisual visual = PersonVisual.CreatePersonVisual (p, go);
		phones.Add (p.id, visual);
	}

	void onPersonLeave (string id)
	{
		phones [id].onDestroy ();
		Debug.Log ("A person left!");
		phones.Remove (id);
	}

	void planeUpdate (PersonData p)
	{
		// Need to adjust this part too as it fixes the rotation
		p.rot.x = 90;
		p.rot.y = 0;

		// Check the anchor image position
		if (!FindWorldOrigin.yPos.Equals(null)) {
			p.pos.y = FindWorldOrigin.yPos;
//			Debug.Log ("Pass Anchor position to objects");
//			Debug.Log (string.Format ("y:{0:0.######}", p.pos.y));
		}
	}

	bool proximityDetect (Vector3 mainPos, PersonData p)
	{
		float dist = Vector3.Distance (mainPos, p.pos);
		Debug.Log ("Distance to other: " + dist);

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


