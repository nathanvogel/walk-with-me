using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ImageAnchorFinder : MonoBehaviour {


	[SerializeField]
	private ARReferenceImage imageParsons;

	[SerializeField]
	private ARReferenceImage imageEcal;

	public GameObject prefabToGenerate;
	public DatabasePeople data;

	private GameObject imageAnchorGO;
	public static float yPos;

	// Use this for initialization
	void Start () {
		UnityARSessionNativeInterface.ARImageAnchorAddedEvent += AddImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent += UpdateImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent += RemoveImageAnchor;
	}

	void AddImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.Log ("image anchor added");
		// Check that it's one of our images
		if (arImageAnchor.referenceImageName == imageParsons.imageName 
			|| arImageAnchor.referenceImageName == imageEcal.imageName) {

			// Visual debug and one empty gameObject is necessary to have a Transform in Unity anyway.
			Vector3 position = UnityARMatrixOps.GetPosition (arImageAnchor.transform);
			Quaternion rotation = UnityARMatrixOps.GetRotation (arImageAnchor.transform);
			imageAnchorGO = Instantiate<GameObject> (prefabToGenerate, position, rotation);

			updateWorldOrigin (imageAnchorGO.transform);

			// If it's Parsons.
			if (arImageAnchor.referenceImageName == imageParsons.imageName) {
				data.SetLocation ("parsons");
			} else if (arImageAnchor.referenceImageName == imageEcal.imageName) {
				data.SetLocation ("ecal");
			}
		}
	}

	void updateWorldOrigin(Transform origin) {
		// Instead of setting the world origin, we could also apply the delta ourself 
		// when moving our objects if doing it onAnchorUpdate is buggy
		UnityARSessionNativeInterface.GetARSessionNativeInterface().SetWorldOrigin (origin);
	}

	void UpdateImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.Log ("image anchor updated");
		if (arImageAnchor.referenceImageName == imageParsons.imageName 
			|| arImageAnchor.referenceImageName == imageEcal.imageName) {
			imageAnchorGO.transform.position = UnityARMatrixOps.GetPosition (arImageAnchor.transform);
			imageAnchorGO.transform.rotation = UnityARMatrixOps.GetRotation (arImageAnchor.transform);
//			Debug.Log ("Y = " + imageAnchorGO.transform.position.y);

			// No need to update the origin again, ARKit seems smart.
			//updateWorldOrigin (imageAnchorGO.transform);
		}

		yPos = imageAnchorGO.transform.position.y;
		Debug.Log ("imageAnchorGo y position + " + yPos);
	}

	void RemoveImageAnchor(ARImageAnchor arImageAnchor)
	{
		Debug.Log ("image anchor removed");
		if (imageAnchorGO) {
			GameObject.Destroy (imageAnchorGO);
		}

	}

	void OnDestroy()
	{
		UnityARSessionNativeInterface.ARImageAnchorAddedEvent -= AddImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorUpdatedEvent -= UpdateImageAnchor;
		UnityARSessionNativeInterface.ARImageAnchorRemovedEvent -= RemoveImageAnchor;

	}

	// Update is called once per frame
	void Update () {
		
	}
}
