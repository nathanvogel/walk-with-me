using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;


public class PersonData {
    public Vector3 pos;
    public Vector3 rot;
    public string id;
	public string locationId;

	PersonData(Vector3 pos, Vector3 rot, string locationId) {
        this.pos = pos;
        this.rot = rot;
        id = SystemInfo.deviceUniqueIdentifier;
		this.locationId = locationId;
    }

    // Convenience
	public static PersonData CreatePersonData(Transform t, string locationId) {
		return new PersonData(t.position, t.rotation.eulerAngles, locationId);
    }
}


public class DatabaseConnection : MonoBehaviour {

	// --- Data behavior settings ---
	// Controls whether objects are deleted from Firebase when the user
	// quits the application.
	private bool deleteWhenLeaving = true;
	private bool filterRooms = true;

	// --- Firebase ---
	// A Firebase Reference to the list of person objects.
	private DatabaseReference peopleRef;

	// Room IDs
	public string[] rooms = {"parsons", "ecal"};
	public string deviceLocationId; // Where the user is.
	public string otherLocationId; // Where the user isn't.

	// List of people whose footprint we need to show.
	public Dictionary<string, PersonData> persons = new Dictionary<string, PersonData>();


    void Start() {
		// To be called before any other Firebase function
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://newp-f426c.firebaseio.com/");

		// Set in which room the user is.
		// This could be determined by:
		// - Recognition of the origin object/QR code in AR
		// - GPS coordinates
		// - User choice
		// - ...
		int roomIndex = Random.Range(0, 2);
		deviceLocationId = rooms[roomIndex];
		otherLocationId = rooms [roomIndex == 0 ? 1 : 0];
		Debug.Log ("The user is in " + deviceLocationId);

		// Start listening for people 
		peopleRef = FirebaseDatabase.DefaultInstance.GetReference("people");
		// Filter the users to only get the data from the other room.
		var query = peopleRef.OrderByChild("locationId");
		if (filterRooms) {
			query = query.EqualTo (otherLocationId);
			// If we later want to show footprints from more than 2 rooms, we could use this technic:
			// https://stackoverflow.com/questions/39195191/firebase-query-to-exclude-data-based-on-a-condition/39195551#39195551
			// Or we could structure the data by room: /rooms/<roomId>/people/<personId>
		}
		query.ChildAdded += HandleChildAdded;
		query.ChildChanged += HandleChildChanged;
		query.ChildRemoved += HandleChildRemoved;

		// Get ready to delete the people object if they quit the application,
		// cutting the connection to Firebase and triggering OnDisconnect().
		if (deleteWhenLeaving) {
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).OnDisconnect ().RemoveValue ();
		}

		// Start continuously saving our position to Firebase.
		StartCoroutine(_SendData());
	}



	// This is function is called:
	// - when the app opens: for each person present in the room
	// - when a new person arrives in the room
	void HandleChildAdded(object sender, ChildChangedEventArgs args) {
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		// Instanciate the Object from the Firebase JSON data and add it to our array.
		PersonData person = JsonUtility.FromJson<PersonData>(args.Snapshot.GetRawJsonValue());
		Debug.Log ("CREATE " + person.id);
		persons.Add(person.id, person);
	}

	// This function is called when an object (already handled in HandleChildAdded)
	// changes its data.
	void HandleChildChanged(object sender, ChildChangedEventArgs args) {
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		// Get new data
		PersonData person = JsonUtility.FromJson<PersonData>(args.Snapshot.GetRawJsonValue());
		Debug.Log ("UPDATE " + person.id);
		persons[person.id] = person;

	}

	// When someone leaves the room.
	void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		Debug.Log ("REMOVE " + args.Snapshot.Key);
		persons.Remove(args.Snapshot.Key);
	}


    // Coroutine to save the user's position to Firebase
    IEnumerator _SendData() {
        while(true) {
			// The ID that identifies the person in the room.
			string id = SystemInfo.deviceUniqueIdentifier;

			PersonData cd = PersonData.CreatePersonData(Camera.main.transform, deviceLocationId);
            string scd = JsonUtility.ToJson(cd);

			// Save to Firebase
			peopleRef.Child(cd.id).SetRawJsonValueAsync(scd);
			//Debug.Log (cd.pos.x);

            // Wait half a second
            yield return new WaitForSeconds(0.5f);
        }
    }






	// Called when the Unity Scene is closed.
	void OnDestroy() {
		/*
		// This isn't really needed and peopleRef is already null in OnDestroy
		// (which causes an error) so it probably belong somewhere else, if at all necessary.
		// Turn off Firebase listener to clean up the app.
		peopleRef.ChildAdded -= HandleChildAdded;
		peopleRef.ChildChanged -= HandleChildChanged;
		peopleRef.ChildRemoved -= HandleChildRemoved;
		*/

		if (deleteWhenLeaving) { 
			// Also remove the people object here to ensure immediate effect.
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).RemoveValueAsync ();
		}
	}
}
