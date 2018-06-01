using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class AppLifecycle : MonoBehaviour
{


	public Canvas chatCanvas;
	public Canvas teleportCanvas;
	public GameObject experienceManager;
	public GameObject originAnchor;
	public DatabaseConnection data;

	InputField nameField;

	// Use this for initialization
	void Start ()
	{
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
		// Use the current position for the canvas
		UnityARSessionNativeInterface.GetARSessionNativeInterface ().SetWorldOrigin (Camera.main.transform);
		// Move our anchor with it, so that FindFloors work.
		originAnchor.transform.position = new Vector3 (0, 0, 0);

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
