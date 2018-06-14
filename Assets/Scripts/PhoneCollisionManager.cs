using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneCollisionManager : MonoBehaviour
{
	
	public List<string> uidsInCollisions = new List<string>();


	public bool isInCollision() {
		return uidsInCollisions.Count > 0;
	}

	void OnTriggerEnter (Collider other)
	{
		print ("Collision !");
		string otherId = other.gameObject.name.Replace ("Phone.", "");
		uidsInCollisions.Add (otherId);
	}

	void OnTriggerStay (Collider other)
	{
	}

	void OnTriggerExit (Collider other)
	{
		print ("Collision exit.");
		string otherId = other.gameObject.name.Replace ("Phone.", "");
		uidsInCollisions.Remove (otherId);
	}
}
