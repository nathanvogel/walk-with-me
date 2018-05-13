using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;


public class PersonData
{
	// Database values
	public Vector3 pos;
	public Vector3 rot;
	public string id;
	public string locationId;

	// Computed local values
	[System.NonSerialized]
	public float distance;

	PersonData (Vector3 pos, Vector3 rot, string locationId)
	{
		this.pos = pos;
		this.rot = rot;
		id = SystemInfo.deviceUniqueIdentifier;
		this.locationId = locationId;
	}

	// Convenience for sending our data
	public static PersonData CreatePersonData (Transform t, string locationId)
	{
		return new PersonData (t.position, t.rotation.eulerAngles, locationId);
	}

	public static PersonData Deserialize (string str)
	{
		PersonData p = JsonUtility.FromJson<PersonData> (str);
		p.computeLocalValues ();
		return p;
	}

	public void computeLocalValues ()
	{
		Vector2 userXZ = new Vector2 (Camera.main.transform.position.x, Camera.main.transform.position.z);
		Vector2 personXZ = new Vector2 (pos.x, pos.z);
		this.distance = Vector2.Distance (userXZ, personXZ);
	}
}


public class DatabaseConnection : MonoBehaviour
{

	// --- Data behavior settings ---
	// Controls whether objects are deleted from Firebase when the user
	// quits the application.
	private bool deleteWhenLeaving = true;
	private bool filterRooms = true;
	private float updateInterval = 0.08f;

	// --- Firebase ---
	// A Firebase Reference to the list of person objects.
	// Null until we start streaming
	private DatabaseReference peopleRef;

	// Room IDs
	public string[] rooms = { "parsons", "ecal" };
	public string deviceLocationId;
	// Where the user is.
	public string otherLocationId;
	// Where the user isn't.

	// List of people whose footprint we need to show.
	public Dictionary<string, PersonData> persons = new Dictionary<string, PersonData> ();


	void Start ()
	{
		// Setting the URL is only needed in the editor.
		#if UNITY_EDITOR
		// Warning: Putting this in Awake() makes the Unity Editor crash!
		FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://newp-f426c.firebaseio.com/");
		Debug.Log ("Editor detected: Set Firebase URL");
		#endif

		// If we're running inside Unity, we won't get any image detection event
		// so we can directly set a fixed location now.
		#if UNITY_EDITOR
		Debug.Log ("Editor detected: Auto-setting location");
		SetLocation ("ecal");
		#endif
	}


	/* 
	 * Called by FindWorldOrigin.cs when an image is detected for the first time.
	 */
	public void SetLocation (string locationId)
	{

		// No need to listen/send again, we can't teleport anyway.
		if (peopleRef != null) {
			Debug.Log ("Cancel SetLocation() because it was already called");
			return;
		}

		deviceLocationId = locationId;
		otherLocationId = locationId == rooms [0] ? rooms [1] : rooms [0];
		Debug.Log ("The user is in " + deviceLocationId);

		// Start listening for people 
		peopleRef = FirebaseDatabase.DefaultInstance.GetReference ("people");
		// Filter the users to only get the data from the other room.
		var query = peopleRef.OrderByChild ("locationId");
		if (filterRooms) {
			Debug.Log ("Filter rooms to only: " + otherLocationId);
			query = query.EqualTo (otherLocationId);
			// If we later want to show footprints from more than 2 rooms, we could use this technic:
			// https://stackoverflow.com/questions/39195191/firebase-query-to-exclude-data-based-on-a-condition/39195551#39195551
			// Or we could structure the data by room: /rooms/<roomId>/people/<personId>
		}
		Debug.Log ("Adding listeners");
		query.ChildAdded += HandleChildAdded;
		query.ChildChanged += HandleChildChanged;
		query.ChildRemoved += HandleChildRemoved;

		// Get ready to delete the people object if they quit the application,
		// cutting the connection to Firebase and triggering OnDisconnect().
		// This happens server-side, so it should work even if the app crashes.
		if (deleteWhenLeaving) {
			Debug.Log ("Will delete on leave");
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).OnDisconnect ().RemoveValue ();
		}

		// Start continuously saving our position to Firebase.
		StartCoroutine (_SendData ());
	}



	// This is function is called:
	// - when the app opens: for each person present in the room
	// - when a new person arrives in the room
	void HandleChildAdded (object sender, ChildChangedEventArgs args)
	{
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
			return;
		}

		// Instanciate the Object from the Firebase JSON data and add it to our array.
		PersonData person = PersonData.Deserialize (args.Snapshot.GetRawJsonValue ());
//		Debug.Log ("CREATE " + person.id);
		persons.Add (person.id, person);
	}

	// This function is called when an object (already handled in HandleChildAdded)
	// changes its data.
	void HandleChildChanged (object sender, ChildChangedEventArgs args)
	{
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
			return;
		}

		// Get new data
		PersonData person = PersonData.Deserialize (args.Snapshot.GetRawJsonValue ());
//		Debug.Log ("UPDATE " + person.id);
		persons [person.id] = person;

	}

	// When someone leaves the room.
	void HandleChildRemoved (object sender, ChildChangedEventArgs args)
	{
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
			return;
		}

		//Debug.Log ("REMOVE " + args.Snapshot.Key);
		persons.Remove (args.Snapshot.Key);
	}


	// Coroutine to save the user's position to Firebase
	IEnumerator _SendData ()
	{
		while (true) {
			// The ID that identifies the person in the room.
			string id = SystemInfo.deviceUniqueIdentifier;

			// Get the position of the current camera.
			PersonData cd = PersonData.CreatePersonData (Camera.main.transform, deviceLocationId);
			string scd = JsonUtility.ToJson (cd);

			// Save to Firebase
			peopleRef.Child (cd.id).SetRawJsonValueAsync (scd);

			// Wait some time before sending again
			yield return new WaitForSeconds (updateInterval);
		}
	}


	void Update ()
	{
		// Update the position of every footprint visual.
		foreach (KeyValuePair<string,PersonData> personData in persons) {
			persons [personData.Key].computeLocalValues ();
		}
	}


	// Called when the Unity Scene is closed.
	void OnDestroy ()
	{
		/*
		// This isn't really needed and peopleRef is already null in OnDestroy
		// (which causes an error) so it probably belong somewhere else, if at all necessary.
		peopleRef.ChildAdded -= HandleChildAdded;
		peopleRef.ChildChanged -= HandleChildChanged;
		peopleRef.ChildRemoved -= HandleChildRemoved;
		*/

		if (deleteWhenLeaving && peopleRef != null) { 
			// Also remove the people object here to ensure immediate effect.
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).RemoveValueAsync ();
		}
	}
}
