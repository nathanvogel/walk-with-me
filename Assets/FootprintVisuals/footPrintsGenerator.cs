using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootprintsGenerator : MonoBehaviour {

    public GameObject footstepPrefab;
    public float footstepLength;
    public float footstepOffset;
    public static Rigidbody rigitBody;
    private bool left;
    // Use this for initialization
    public Vector3 lastPos;
    public static Vector3 leftPos;
    public static Vector3 rightPos;
    private LinkedList<GameObject> steps;
    public static bool destroy = false;
    void Start () { 
        lastPos = gameObject.transform.position;
        rigitBody = gameObject.GetComponent<Rigidbody>();
        steps = new LinkedList<GameObject>();
	}
    //round dem bobs
  

    // Update is called once per frame
    void Update () {
       
        if (!((lastPos.magnitude+footstepLength)>gameObject.transform.position.magnitude)|| !((lastPos.magnitude - footstepLength) < gameObject.transform.position.magnitude)) {

            Quaternion rotation = Quaternion.Euler(new Vector3(gameObject.transform.rotation.eulerAngles.x, gameObject.transform.rotation.eulerAngles.y+180, gameObject.transform.rotation.eulerAngles.z));
            float angle=gameObject.transform.rotation.eulerAngles.y;
            Vector3 stepPos;
            //update last pos
            //TODO: 
            lastPos = gameObject.transform.position;
            if (left)
            {
                if (angle < 92 && angle > 88 || angle < 272 && angle > 268)
                {
                    stepPos = new Vector3(lastPos.x , lastPos.y, lastPos.z + footstepOffset);
                }
                else {
                    stepPos = new Vector3(lastPos.x + footstepOffset, lastPos.y, lastPos.z);
                }
                leftPos = stepPos;


                left = false;
            }
            else {

                if (angle < 92 && angle > 88 || angle < 272 && angle > 268)
                {
                    stepPos = new Vector3(lastPos.x, lastPos.y, lastPos.z - footstepOffset);
                }
                else
                {
                    stepPos = new Vector3(lastPos.x - footstepOffset, lastPos.y, lastPos.z);
                }
                left = true;
                rightPos = stepPos;
            }
            Instantiate(footstepPrefab, stepPos, rotation);
        

        }
       		
	}
}
