using UnityEngine;
using System.Collections;

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
			Debug.Log (data.persons.Count);
		}
	}
}

