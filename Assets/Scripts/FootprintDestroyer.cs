using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintDestroyer : MonoBehaviour
{

	public Color c;

	private float alpha = 1f;
	private float alphaLevelChange = 0.02f;
	private bool fading = false;

	void Start ()
	{
		//get color from main Script
		c = FootprintsGenerator.stepColor;
		GetComponent<MeshRenderer> ().material.color = c;
	}


	void Update ()
	{
		// If the step is far from the foot, start fading to destruction.
		if (!fading &&
		    (
		        transform.position.x != FootprintsGenerator.leftPos.x ||
		        transform.position.y != FootprintsGenerator.leftPos.y) &&
			
		    (
		        transform.position.x != FootprintsGenerator.rightPos.x ||
		        transform.position.y != FootprintsGenerator.rightPos.y)) {
			fading = true;
		}

		if (fading) {
			// If we're almost transparent, destroy and quit.
			if (alpha < (0 + alphaLevelChange)) {
				Destroy (gameObject, 0f);
				return;
			}
			// Change the mesh color
			c = FootprintsGenerator.stepColor;
			GetComponent<MeshRenderer> ().material.color = new Color (c.r, c.g, c.b, alpha);
			// Become more transparent, at an fps-independant speed (here calibrated on 60 FPS)
			// deltaTime will be bigger if the frame is longer than usual (less FPS) -> More change.
			// * 60 so that if the game is running smoothly at 60 FPS, it has no effect (same as * 1) 
			alpha -= alphaLevelChange * Time.deltaTime * 60;
		}
	}
}



