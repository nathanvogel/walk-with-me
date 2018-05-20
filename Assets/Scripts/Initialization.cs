using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;

public class Initialization : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// Setting the URL is only needed in the editor.
		#if UNITY_EDITOR
		// Warning: Putting this in Awake() makes the Unity Editor crash!
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://newp-f426c.firebaseio.com/");
		Debug.Log ("Editor detected: Set Firebase URL");
		#endif
	}
}
