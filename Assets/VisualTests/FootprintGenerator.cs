using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
public class FootprintGenerator : MonoBehaviour {

    public GameObject footstepPrefab;
    public float footstepLength;
    public float footstepOffset;
 
    private bool left;
    // Use this for initialization
    private Vector3 lastPos;
    void Start () {
        lastPos = gameObject.transform.position;
		
	}
	//int Quaternion = transformRotation;

	// Update is called once per frame
	void Update () {
		//transformRotation = Quaternion(transform.rotation.x, -transform.rotation.y, transform.rotation.z, 0.0);

        if (!((lastPos.magnitude+footstepLength)>gameObject.transform.position.magnitude)|| !((lastPos.magnitude - footstepLength) < gameObject.transform.position.magnitude)) {
           
            print("spawn dem steppy steps");
            Vector3 stepPos;
            //update last pos
            lastPos = gameObject.transform.position;
            if (left)
            {
                print("left bob");
                stepPos = new Vector3(lastPos.x + footstepOffset, lastPos.y, lastPos.z);
               
                left = false;
            }
            else {
                print("right bob");
                stepPos = new Vector3(lastPos.x - footstepOffset, lastPos.y, lastPos.z);
                left = true;
            }
            Instantiate(footstepPrefab, stepPos, Quaternion.identity);

        }
		
	}
}
