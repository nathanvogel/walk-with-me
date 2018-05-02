using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviour {
	AudioSource m_audio;

	// Use this for initialization
	void Start () {
		m_audio = GetComponent<AudioSource>();
//		m_audio.Pause ();
	}
	
	// Update is called once per frame
	void Update () {
//		m_audio.Play ();
		if (FootprintManager.prox) {
			m_audio.Play ();
		} else {
			m_audio.Pause ();
		}
	}
}
