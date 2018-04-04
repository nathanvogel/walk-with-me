using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FootprintManager : MonoBehaviour
{

	public DatabaseConnection data;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		// We can access the persons in the other room with data.persons

		// For debug purposes 
		if (Time.frameCount % 30 == 0) {
			Debug.Log ("Number of session: " + data.persons.Count);
			foreach (KeyValuePair<string, PersonData> kvp in data.persons) {
				Debug.Log (kvp);
				PersonData cv = kvp.Value;
				Debug.Log ("User position: " + cv.id + ", " + cv.pos.x + ", " + cv.pos.y + ", " + cv.pos.z);
				Debug.Log ("User rotation: " + cv.id + ", " + cv.rot.x + ", " + cv.rot.y + ", " + cv.rot.z);
			}
		}
	}
}

