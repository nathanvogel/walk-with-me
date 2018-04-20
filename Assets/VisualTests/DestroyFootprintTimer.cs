using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyFootprintTimer : MonoBehaviour {

    // Use this for initialization
    public float destroyTimer;
	void Start () {
        Destroy(gameObject, destroyTimer);
       
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
