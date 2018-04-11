﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


 public class PersonVisual
{ 	public string id;
	GameObject phone;
 	PersonVisual (PersonData p, GameObject go)
	{ 		this.id = p.id;
		this.phone = go; 	}  	// Convenience 	public static PersonVisual CreatePersonVisual (PersonData p, GameObject go)
	{ 		return new PersonVisual (p, go); 	}

	public void update (PersonData person)
	{
		/* smooth interpolation attempt 		phone.transform.position = Vector3.MoveTowards ( 			phone.transform.position, person.pos, 0.3f 		);
 		phone.transform.rotation = Quaternion.RotateTowards ( 			phone.transform.rotation, Quaternion.Euler(person.rot), 0.3f 		);
		*/

		phone.transform.position = person.pos;
		phone.transform.rotation = Quaternion.Euler (person.rot);
	}

	public void onDestroy() { 		Object.Destroy (this.phone);
	} } 

public class FootprintManager : MonoBehaviour
{
	public GameObject cubePrefab;
	GameObject myCube; 
	Dictionary<string, PersonVisual> phones = new Dictionary<string, PersonVisual> ();


	public DatabaseConnection data;

	void Start ()
	{
	}

	// Update is called once per frame
	void Update ()
	{
		// For debug purposes 
		if (Time.frameCount % 30 == 0) {
			Debug.Log (data.persons.Count);
		}

  		foreach (KeyValuePair<string,PersonData> person in data.persons) {
			// Check for persons arriving in the room
			// Instantiate their objects if needed
			if (!phones.ContainsKey (person.Key)) {
				onPersonArrival (person.Value);
			}

			phones [person.Key].update (person.Value); 		}

		// Check for persons leaving the room. 		foreach (KeyValuePair<string,PersonVisual> phone in phones) { 			if (!data.persons.ContainsKey (phone.Key)) { 				onPersonLeave (phone.Key); 			} 		}
	}

	void onPersonArrival (PersonData p)
	{
		GameObject go = Instantiate (cubePrefab, p.pos, Quaternion.Euler (p.rot));	
		PersonVisual visual = PersonVisual.CreatePersonVisual (p, go);
		phones.Add (p.id, visual);
	}

	void onPersonLeave (string id)
	{
		phones [id].onDestroy ();
		phones.Remove (id);
	}

	void destruction ()
	{
		Destroy (this.gameObject); 

	}
}


