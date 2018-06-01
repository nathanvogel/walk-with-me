using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class FindFloor : MonoBehaviour
{


	public GameObject planePrefab;
	public GameObject originAnchor;
	private UnityARAnchorManager unityARAnchorManager;

//	public float floorHeight = -1.30f;

	// Use this for initialization
	void Start ()
	{
		unityARAnchorManager = new UnityARAnchorManager ();
		UnityARUtility.InitializePlanePrefab (planePrefab);
	}

	void OnDestroy ()
	{
		unityARAnchorManager.Destroy ();
	}


	int justUpdated = 0;
	// Update is called once per frame
	void Update ()
	{
		if (justUpdated > 0) {
			justUpdated--;
			return;
		}


		float cameraHeight = Round(Camera.main.transform.position.y, 2);

		List<ARPlaneAnchorGameObject> arpags = unityARAnchorManager.GetCurrentPlaneAnchors ();

		float lowestPlaneHeight = 100f;
		ARPlaneAnchorGameObject lowestPlane = null;
		foreach (ARPlaneAnchorGameObject plane in arpags) {
			float planeHeight = plane.gameObject.transform.position.y;
			if (planeHeight < lowestPlaneHeight) {
				lowestPlaneHeight = Round(planeHeight, 2);
				lowestPlane = plane;
			}
		}

		print (Camera.main.transform.position);

		if (arpags.Count == 0) {
			print ("No plane. Height = " + cameraHeight);
			return;
		}

		float relativeCameraHeight = cameraHeight - lowestPlaneHeight;
		float distToCurrentFloor = Mathf.Abs (lowestPlaneHeight - 0);

		print ("lowest: " + lowestPlaneHeight + " out of " + arpags.Count + " at dist " + distToCurrentFloor + " cur. height " + (cameraHeight - 0) + " rel. " + relativeCameraHeight);

		// Ignore planes that are too far from the user (probably errors)
		// and ignore planes that won't make much of a difference with the current state.
		if (relativeCameraHeight < 1.90f & distToCurrentFloor > 0.04f) {
			print ("UPDATING NEW FLOOR HEIGHT");
			// Pause
			justUpdated = 5;
//			floorHeight = lowestPlaneHeight;

			originAnchor.transform.position = new Vector3(0, lowestPlaneHeight, 0);
//			originAnchor.transform.rotation = new Quaternion (0, 0, 0, 0);
//			print (originAnchor.transform.position);
//			print (originAnchor.transform.rotation);
//			print (originAnchor.transform.localScale);
//			updateWorldOrigin (originAnchor.transform);



			if (lowestPlane == null) {
				print ("PROBLEM");
				return;
			}
			Transform planeTransform = lowestPlane.gameObject.transform;
			print (lowestPlane.gameObject.transform.position);
			print (lowestPlane.gameObject.transform.rotation);
			// Reset plane transform
			planeTransform.Rotate (new Vector3 (0, -planeTransform.eulerAngles.y, 0));
			lowestPlane.gameObject.transform.position = new Vector3 (0, lowestPlaneHeight, 0);
			print (lowestPlane.gameObject.transform.position);
			print (lowestPlane.gameObject.transform.rotation);
			updateWorldOrigin (lowestPlane.gameObject.transform);

		}

		/*
		if (originAnchor.transform.position.y != lowest) {
			Vector3 newPosition = originAnchor.transform.position;
			newPosition.y = lowest;
			originAnchor.transform.position = newPosition;
			print ("UPDATING ORIGIN !!!");
			updateWorldOrigin (originAnchor.transform);
		}
		*/

	}

	public static float Round(float value, int digits)
	{
		float mult = Mathf.Pow(10.0f, (float)digits);
		return Mathf.Round(value * mult) / mult;
	}


	void updateWorldOrigin(Transform origin) {
		// Instead of setting the world origin, we could also apply the delta ourself 
		// when moving our objects if doing it onAnchorUpdate is buggy
		UnityARSessionNativeInterface.GetARSessionNativeInterface().SetWorldOrigin (origin);
	}

	void OnGUI() {
		/*
		List<ARPlaneAnchorGameObject> arpags = unityARAnchorManager.GetCurrentPlaneAnchors ();
		// Show debug planes

		if (arpags.Count >= 1) {
			print ("Show debug planes");
			ARPlaneAnchor ap = arpags [0].planeAnchor;
			GUI.Box (new Rect (100, 100, 800, 60), string.Format ("Center: x:{0}, y:{1}, z:{2}", ap.center.x, ap.center.y, ap.center.z));
			GUI.Box (new Rect (100, 200, 800, 60), string.Format ("Extent: x:{0}, y:{1}, z:{2}", ap.extent.x, ap.extent.y, ap.extent.z));
		}
		*/
	}

}
