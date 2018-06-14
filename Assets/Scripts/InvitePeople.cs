using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InvitePeople : MonoBehaviour {


	public DatabasePeople data;
	public GameObject inviteDialog;

	private float startTime;

	// Use this for initialization
	void Start () {
		startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		inviteDialog.SetActive (Time.time > startTime + 3f && data.persons.Count == 0);
	}

	public void OnShareClick() {
		print ("Sharing invite");
//		shareOnlyTextMethod ("Come walk with me in augmented reality! https://itunes.apple.com/us/app/pok%C3%A9mon-go/id1094591345?mt=8");
		new NativeShare().SetText( "Come walk with me in augmented reality! https://itunes.apple.com/us/app/pok%C3%A9mon-go/id1094591345?mt=8" ).Share();


	}
		
}
