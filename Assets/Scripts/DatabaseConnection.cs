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
	public string roomId;

    PersonData(Vector3 pos, Vector3 rot) {
        this.pos = pos;
        this.rot = rot;
        id = SystemInfo.deviceUniqueIdentifier;
		this.roomId = "ecal";
    }

    // convenience
    public static PersonData CreatePersonData(Transform t) {
        return new PersonData(t.position, t.rotation.eulerAngles);
    }
}

public class DatabaseConnection : MonoBehaviour {

	// A Firebase Reference to the list of person objects.
	private DatabaseReference peopleRef;
	// Controls whether objects are deleted from Firebase when the user
	// quits the application.
	bool deleteWhenLeaving = false;


    void Start() {
		// To be called before any other Firebase function
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://newp-f426c.firebaseio.com/");

		// Start listening for people 
		peopleRef = FirebaseDatabase.DefaultInstance.GetReference("people");
		peopleRef.ChildAdded += HandleChildAdded;
		peopleRef.ChildChanged += HandleChildChanged;
		peopleRef.ChildRemoved += HandleChildRemoved;

		if (deleteWhenLeaving) {
			// Get ready to delete the people object if they quit the application,
			// cutting the connection to Firebase and triggering OnDisconnect().
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).OnDisconnect ().RemoveValue ();
		}

		// Start continuously saving our position to Firebase.
		StartCoroutine(_SendData());
	}


	// Called when the Unity Scene is closed.
	void OnDestroy() {
		// Turn off Firebase listener to clean up the app.
		peopleRef.ChildAdded -= HandleChildAdded;
		peopleRef.ChildChanged -= HandleChildChanged;
		peopleRef.ChildRemoved -= HandleChildRemoved;

		if (deleteWhenLeaving) { 
			// Also remove the people object here to ensure immediate effect.
			string id = SystemInfo.deviceUniqueIdentifier;
			peopleRef.Child (id).RemoveValueAsync ();
		}
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

		// Do something with the data in args.Snapshot
		PersonData person = JsonUtility.FromJson<PersonData>(args.Snapshot.GetRawJsonValue());
		Debug.Log (person.pos.x);
	}

	// This function is called when an object (already handled in HandleChildAdded)
	// changes its data.
	void HandleChildChanged(object sender, ChildChangedEventArgs args) {
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		// Get data
		PersonData person = JsonUtility.FromJson<PersonData>(args.Snapshot.GetRawJsonValue());
		Debug.Log (person.pos.x);

	}

	// When someone leaves the room.
	void HandleChildRemoved(object sender, ChildChangedEventArgs args) {
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError(args.DatabaseError.Message);
			return;
		}

		// Do something with the data in args.Snapshot
	}


    // Coroutine
    IEnumerator _SendData() {
        while(true) {
			// The ID that identifies the person in the room.
			string id = SystemInfo.deviceUniqueIdentifier;

            PersonData cd = PersonData.CreatePersonData(Camera.main.transform);
            string scd = JsonUtility.ToJson(cd);

			// Save to Firebase
			peopleRef.Child(cd.id).SetRawJsonValueAsync(scd);
			//Debug.Log (cd.pos.x);

            // Wait half a second
            yield return new WaitForSeconds(0.5f);
        }
    }

}