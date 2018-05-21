using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using UnityEngine.UI;


public class MessageData
{
	// Database values
	public Vector3 pos;
	public Vector3 rot;
	public string id;
	public string uid;
	public string text;

	MessageData ()
	{
		// Parameterless default constructor for Newtonsoft JSON deserializing
	}

	MessageData (Vector3 pos, Vector3 rot, string text)
	{
		this.pos = pos;
		this.rot = rot;
		this.text = text;
		this.uid = SystemInfo.deviceUniqueIdentifier;
		// Generate a new message ID
		this.id = FirebaseDatabase.DefaultInstance.GetReference ("anything").Push ().Key;
	}

	// Convenience for sending our data
	public static MessageData CreateNewMessage (Transform t, string text)
	{
		return new MessageData (t.position, t.rotation.eulerAngles, text);
	}

	public static MessageData CreateFromJson (JsonSerializer serializer, string str)
	{
		MessageData m = serializer.Deserialize<MessageData> (new JsonTextReader (new StringReader (str)));
		return m;
	}

	public void updateFromJson (JsonSerializer serializer, string str)
	{
		serializer.Populate (new StringReader (str), this);
	}
}


public class Messaging : MonoBehaviour
{
	// --- Firebase ---
	private DatabaseReference messagesRef;
	private Query messagesQuery;
	// JSON helper
	JsonSerializer serializer = new JsonSerializer ();

	// List of messages that we need to show.
	public Dictionary<string, MessageData> messages = new Dictionary<string, MessageData> ();
	public bool sending = false;
	public PhoneCollisionManager sendInteractionChecker;


	// --- UI ---
	public InputField messageField;
	public Button sendButton;

	public Canvas messageUI;


	void Start ()
	{

		string id = SystemInfo.deviceUniqueIdentifier;

		// Start listening for people 
		messagesRef = FirebaseDatabase.DefaultInstance.GetReference ("chats").Child (id).Child ("m");
		messagesQuery = messagesRef.OrderByKey ();

		Debug.Log ("Adding listeners for messages");
		messagesQuery.ChildAdded += HandleChildAdded;
		messagesQuery.ChildChanged += HandleChildChanged;
		messagesQuery.ChildRemoved += HandleChildRemoved;
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
		MessageData message = MessageData.CreateFromJson (serializer, args.Snapshot.GetRawJsonValue ());
		Debug.Log ("Got Message " + message.id + " and x = " + message.pos.x);
		messages.Add (message.id, message);
//		text.transform.pos
	}

	void HandleChildChanged (object sender, ChildChangedEventArgs args)
	{
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
			return;
		}

		// Get new data
		messages [args.Snapshot.Key].updateFromJson (serializer, args.Snapshot.GetRawJsonValue ());

	}

	void HandleChildRemoved (object sender, ChildChangedEventArgs args)
	{
		// Check errors
		if (args.DatabaseError != null) {
			Debug.LogError (args.DatabaseError.Message);
			return;
		}

		//Debug.Log ("REMOVE " + args.Snapshot.Key);
		messages.Remove (args.Snapshot.Key);
	}


	void Update ()
	{
		if (sendInteractionChecker.shouldSend ()) {
			// SendMessage() takes care of checking if basic conditions
			// for sending messages are met, so we can call that on 
			// every frame.
			// Case not well handled : errors when sending while cause retry everytime
			SendMessage ();
		}
	}


	void SendMessage ()
	{
		// Check that we aren't already sending.
		if (sending)
			return;
		// The message
		string text = messageField.text.Trim ();
		// will only send if there's a content.
		if (text.Length == 0)
			return;
		
		// Build the message
		// The ID that identifies the person in the room.
		string id = SystemInfo.deviceUniqueIdentifier;
		// Get the position of the current camera.
		MessageData message = MessageData.CreateNewMessage (Camera.main.transform, text);
		// Rotate it towards the other users
		message.rot.y += 180;
		string rawJson = JsonUtility.ToJson (message);


		// Save to Firebase
		print ("Sending...");
		sending = true;
		messageField.interactable = false;
		sendButton.interactable = false;

		// Send to others
		foreach (string uid in sendInteractionChecker.uidsInCollisions) {
			DatabaseReference recipientRef = FirebaseDatabase.DefaultInstance.GetReference ("chats").Child (uid).Child ("m");
			recipientRef.Child (message.id).SetRawJsonValueAsync (rawJson).ContinueWith (task => {
				if (task.IsFaulted) {
					// Handle the error...
					Debug.Log ("Couldn't save the message");
					Debug.Log (task.Exception.Message);
				} else if (task.IsCompleted) {
					messageField.text = "";
				}
				sending = false;
				messageField.interactable = true;
				sendButton.interactable = true;
			});
		}
		if (sendInteractionChecker.uidsInCollisions.Count == 0) {
			messageField.text = "";
			sending = false;
			messageField.interactable = true;
			sendButton.interactable = true;
		}
			
		// Display locally on own screen
		ShowMessage(message);
	}


	void ShowMessage(MessageData message) {
		Vector3 rotation = message.rot;
		rotation.x = 0;
		rotation.z = 0;
		Canvas canvas = Instantiate (messageUI, message.pos, Quaternion.Euler (rotation));
		Text text = (Text)canvas.GetComponentsInChildren<Text> ().GetValue (0);
		text.text = message.text;
	}


	public void OnClick ()
	{
		SendMessage ();
	}


	// Called when the Unity Scene is closed.
	void OnDestroy ()
	{
		/*
		// This isn't really needed and peopleRef is already null in OnDestroy
		// (which causes an error) so it probably belong somewhere else, if at all necessary.
		messagesRef.ChildAdded -= HandleChildAdded;
		messagesRef.ChildChanged -= HandleChildChanged;
		messagesRef.ChildRemoved -= HandleChildRemoved;
		*/

	}
}
