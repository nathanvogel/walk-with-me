using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class FloorFinder : MonoBehaviour
{

	// The prefab to debug planes
	public GameObject planePrefab;
	public Transform originTransform;
	// Tolerance for imprecision before making a call to SetWorldOrigin();
	public float FLOOR_PRECISION_TOLERANCE = 0.02f;
	// Ignore plane that are that far, as they're probably errors from ARKit
	// or other floors than the one the user is on.
	// (This would prevent the ability to go upstairs and have an "AR minimap" of the people walking...)
	public float MAXIMUM_PLAUSIBLE_HEIGHT = 1.90f;

	private UnityARAnchorManager unityARAnchorManager;

	// Use this for initialization
	void Start ()
	{
		unityARAnchorManager = new UnityARAnchorManager ();
		// Uncomment to display planes for debugging.
//		UnityARUtility.InitializePlanePrefab (planePrefab);
	}

	void OnDestroy ()
	{
		unityARAnchorManager.Destroy ();
	}


	private int justUpdated = 0;

	// Update is called once per frame
	void Update ()
	{
		// Delay between calls to SetWorldOrigin().
		if (justUpdated > 0) {
			justUpdated--;
			return;
		}

		if (unityARAnchorManager == null) {
			print ("FloorFinder: Anchor manager is null.");
			return;
		}
		// Get the planes found by ARKit.
		List<ARPlaneAnchorGameObject> arpags = unityARAnchorManager.GetCurrentPlaneAnchors ();

		// Find the lowest plane
		float lowestPlaneHeight = 100f;
		ARPlaneAnchorGameObject lowestPlane = null;
		foreach (ARPlaneAnchorGameObject plane in arpags) {
			float planeHeight = plane.gameObject.transform.position.y;
			if (planeHeight < lowestPlaneHeight) {
				lowestPlaneHeight = planeHeight;
				lowestPlane = plane;
			}
		}

//		float cameraHeight = Camera.main.transform.position.y;
//		float yDistanceToCamera = cameraHeight - lowestPlaneHeight;
		float distToCurrentFloor = Mathf.Abs (lowestPlaneHeight - 0);

		// and ignore planes that won't make much of a difference with the current state.
		if (lowestPlane != null && distToCurrentFloor > FLOOR_PRECISION_TOLERANCE) {
			print ("UPDATING NEW FLOOR HEIGHT");
			// Don't update the origin too frequently, becaues it takes a few frames to be applied it seems.
			justUpdated = 5;

			// Use the plane transform, as it's xz-rotated in the correct direction.
//			Transform planeTransform = lowestPlane.gameObject.transform;
//			// Reset plane transform
//			planeTransform.Rotate (new Vector3 (0, -planeTransform.eulerAngles.y, 0));
//			lowestPlane.gameObject.transform.position = new Vector3 (0, lowestPlaneHeight, 0);
//			updateWorldOrigin (lowestPlane.gameObject.transform);
			originTransform.position = new Vector3 (0, lowestPlaneHeight, 0);
			updateWorldOrigin (originTransform);
			originTransform.position = new Vector3 (0, 0, 0);
		}
	}


	void updateWorldOrigin (Transform origin)
	{
		// Instead of setting the world origin, we could also apply the delta ourself 
		// when moving our objects if doing it onAnchorUpdate is buggy.
		// Warning: we call SetWorldOrigin(Camera.main.transform) when starting the experience.
		// This might be the reason why we get weird results.

		// If SetWorldOrigin is buggy, we can also put all our objects in a parent
		// and move this parent instead. 
		// We could then use InverseTransformPoint() to get the position of the Camera
		// to be sent to Firebase.
		UnityARSessionNativeInterface.GetARSessionNativeInterface ().SetWorldOrigin (origin);
	}
}
