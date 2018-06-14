using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class AppLifecycle : MonoBehaviour
{


	public Canvas chatCanvas;

	public GameObject tutorialContainer;
	public Canvas tutorialCanvas;
	public int numberOfCanvas = 4;

	public GameObject experienceManager;
	public Transform originTransform;
	public DatabasePeople data;

	// Use this for initialization
	void Start ()
	{
		// Initilization
		iTween.Defaults.easeType = iTween.EaseType.easeInOutQuad;

		// Disable interactions and enable the tutorial
		chatCanvas.gameObject.SetActive (false);
		tutorialCanvas.gameObject.SetActive (true);

		// Position the tutorial
		// Warning: Do it after its activation, otherwise the calculation is wrong for some reason.
		int canvasIndex = PlayerPrefs.HasKey (C.PREF_HAS_JOINED_ONCE) ? 3 : 0;
		tutorialContainer.transform.localPosition = GetPositionForTutorialStep (canvasIndex);

		experienceManager.SetActive (false);
	}

	public void OnGoToTutorialStepClick (int step)
	{
		// Animate between canvas. The id (number/index) of the canvas is specified
		// by the button click.
		iTween.MoveTo (tutorialContainer, iTween.Hash (
			"position", GetPositionForTutorialStep (step), 
			"time", 0.300f,
			"islocal", true,
			"delay", 0f,
			"easetype", "easeOutSine"
		));
	}

	Vector3 GetPositionForTutorialStep (int step)
	{
		Rect rect = tutorialCanvas.GetComponent<RectTransform> ().rect;
		// Offset for the initial position, because the transform is at the center of all canvas.
		float offset = (float)(numberOfCanvas - 1) / 2;
		Vector3 newPos = new Vector3 (rect.width * offset + rect.width * -1 * step, 0);
		return newPos;
	}

	public void OnJoinClick ()
	{
		// Use the current user position as a starting point, but suppose the user phone is at the
		// height that we preset in our build (the phone should be around 0.8m - 1m50 in most cases)
		originTransform.position = new Vector3 (
			Camera.main.transform.position.x, 
			originTransform.position.y, 
			Camera.main.transform.position.z);
		UnityARSessionNativeInterface.GetARSessionNativeInterface ().SetWorldOrigin (originTransform);
		// Move our anchor with it, so that FindFloors work.
//		originTransform.position = new Vector3 (0, 0, 0);

		// Setting the URL is only needed in the editor.
		#if UNITY_EDITOR
		// Warning: Putting this in Awake() makes the Unity Editor crash!
		// Warning: Call this just before using the database for the first time.
		// If done too far apart or in a separate script, it might cause 
		// 		Firebase App initializing app __FIRAPP_DEFAULT (default 1).
		// to log twice, which makes Firebase unable to listen events.
		// (but it's still capable of sending data).
		Debug.Log ("Editor detected: Set Firebase URL");
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://newp-f426c.firebaseio.com/");
		#endif

		// Enable the experience.
		chatCanvas.gameObject.SetActive (true);
		tutorialCanvas.gameObject.SetActive (false);
		experienceManager.SetActive (true);
		data.SetLocation ("ecal");

		// Save that the user started once
		PlayerPrefs.SetInt(C.PREF_HAS_JOINED_ONCE, 1);
	}

	public void OnHelpClick() {
		chatCanvas.gameObject.SetActive (false);
		tutorialCanvas.gameObject.SetActive (true);
		tutorialContainer.transform.localPosition = GetPositionForTutorialStep (0);
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}
