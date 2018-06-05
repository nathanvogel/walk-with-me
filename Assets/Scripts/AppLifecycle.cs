using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class AppLifecycle : MonoBehaviour
{


	public Canvas chatCanvas;
	public Canvas teleportCanvas;
	public GameObject experienceManager;
	public Transform originTransform;
	public DatabasePeople data;

	InputField nameField;

	// Use this for initialization
	void Start ()
	{
		// Initilization
		iTween.Defaults.easeType = iTween.EaseType.easeInOutQuad;

		teleportCanvas.gameObject.SetActive (true);
		chatCanvas.gameObject.SetActive (false);
		experienceManager.SetActive (false);

		nameField = teleportCanvas.GetComponentInChildren<InputField> ();

		if (PlayerPrefs.HasKey ("displayName")) {
			nameField.text = PlayerPrefs.GetString ("displayName");
		} else {
			nameField.text = PlayerPrefs.GetString ("");
		}
	}

	public void OnTeleportClick ()
	{
		// Check that the user wrote a name
		string name = nameField.text.Trim ();
		if (name.Length < 3)
			return;

		// Save the name
		PlayerPrefs.SetString ("displayName", name);
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
		teleportCanvas.gameObject.SetActive (false);
		chatCanvas.gameObject.SetActive (true);
		experienceManager.SetActive (true);
		data.SetLocation ("ecal");
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
