using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintsGenerator : MonoBehaviour
{


	public Vector3 lastPos;
	public static Vector3 leftPos;
	public static Vector3 rightPos;

	public float footstepLength;
	public float footstepOffset;

	public GameObject footstepPrefab;
	//color
	public static Color stepColor;

	private List<GameObject> pastFootprints = new List<GameObject> ();


	//left/right
	private bool left;

	void Start ()
	{
		//get pos
		lastPos = gameObject.transform.position;
		//start with black color
		stepColor = C.DEFAULT_FOOTPRINT_COLOR;

		// Spawn initial right step
		pastFootprints.Add (Instantiate (footstepPrefab, GetStepPosition (), GetStepRotation ()));
		// Spawn initial left step
		pastFootprints.Add (Instantiate (footstepPrefab, GetStepPosition (), GetStepRotation ()));


	}


	public void ChangeColor (Color newColor)
	{
		// Save the color for future steps
		stepColor = newColor;

		// Go in the history of footsteps and recolor them.
		foreach (GameObject footprint in pastFootprints) {
			// Check that the object isn't destroyed.
			if (footprint != null) {
				footprint.gameObject.GetComponent<MeshRenderer> ().material.color = stepColor;
			}
		}
	}

	void Update ()
	{
		// Go through the footprints and remove the ones that are destroyed.
		// Done in two loops to avoid concurrent modification of the array while iterating.
		List<GameObject> toremove = new List<GameObject> ();
		foreach (GameObject footprint in pastFootprints) {
			if (footprint == null) {
				toremove.Add (footprint);
			}
		}
		foreach (GameObject footprint in toremove) {
			pastFootprints.Remove (footprint);
		}


		// If the foot is now far enough
		if (!((lastPos.magnitude + footstepLength) > gameObject.transform.position.magnitude) ||
		    !((lastPos.magnitude - footstepLength) < gameObject.transform.position.magnitude)) {

			// Spawn a new step
			Quaternion rotation = GetStepRotation ();
			Vector3 stepPos = GetStepPosition ();
			pastFootprints.Add (Instantiate (footstepPrefab, stepPos, rotation));
		}

	}

	void OnDestroy ()
	{
		foreach (GameObject footprint in pastFootprints) {
			Destroy (footprint);
		}
	}


	Quaternion GetStepRotation ()
	{
		return Quaternion.Euler (new Vector3 (
			gameObject.transform.rotation.eulerAngles.x + 90, 
			gameObject.transform.rotation.eulerAngles.y, 
			gameObject.transform.rotation.eulerAngles.z));
	}


	Vector3 GetStepPosition ()
	{
		float angle = gameObject.transform.rotation.eulerAngles.y;
		Vector3 stepPos;

		// Update last pos
		lastPos = gameObject.transform.position;

		if (left) {
			if (angle < 92 && angle > 88 || angle < 272 && angle > 268) {
				stepPos = new Vector3 (lastPos.x, 0 + .02f, lastPos.z + footstepOffset);
			} else {
				stepPos = new Vector3 (lastPos.x + footstepOffset, 0 + .02f, lastPos.z);
			}
			leftPos = stepPos;


			left = false;
		} else {

			if (angle < 92 && angle > 88 || angle < 272 && angle > 268) {
				stepPos = new Vector3 (lastPos.x, 0 + .02f, lastPos.z - footstepOffset);
			} else {
				stepPos = new Vector3 (lastPos.x - footstepOffset, 0 + .02f, lastPos.z);
			}
			left = true;
			rightPos = stepPos;
		}
		return stepPos;
	}
}
