using UnityEngine;
using System.Collections;

public class IKController : MonoBehaviour
{
    public Transform LeftToe;   //Left Toe Bone
    public Transform LeftFoot;  //Left Foot Bone
    public Transform LeftShin;  //Left Shin Bone
    public Transform LeftThigh; //Left Thigh Bone

    Ray FootRay;                //Used to detect objects in front of foot
    Ray ground;
    float distanceToGround;
    float temp = 0;

    float targetHeight;
    //Notes
    /*
        - Moving localRotation.y moves tow down and up (+ -)
        - To raise foot
            - rotate thigh on -yaxis
            - rotate shin on +yaxis
            - Whatever operation done to thigh, the opposite on shin properly adjusts*/
    void Start()
    {

    }

    void LateUpdate()
    {
        if (targetHeight != 0)
        {
            Debug.Log("Going good");
            float movementNeeded = targetHeight - ground.direction.y;
            float shinRot = 0.0f;
            float thighRot = 0.0f;

            if (ground.direction.y < targetHeight)
            {

                shinRot += targetHeight * 10;
                thighRot -= targetHeight * 10;
                Quaternion shinRotNeed = Quaternion.Euler(LeftShin.localRotation.eulerAngles + new Vector3(0.0f, shinRot, 0.0f));
                Quaternion thighRotNeed = Quaternion.Euler(LeftShin.localRotation.eulerAngles + new Vector3(0.0f, thighRot, 0.0f));
                LeftShin.localRotation = Quaternion.Slerp(LeftShin.localRotation, shinRotNeed, 2);
                LeftThigh.localRotation = Quaternion.Slerp(LeftThigh.localRotation, thighRotNeed, 2);
            }


        }
    }

    void FixedUpdate()
    {
        targetHeight = 0;
        FootRay.origin = LeftToe.position;      //Start ray from toes
        FootRay.direction = -LeftToe.right;     //Ray goes in direction of toes
        ground.origin = LeftFoot.position;
        ground.direction = new Vector3(0.0f, -5.0f, 0.0f);
        
        RaycastHit objHit;                      //Collision detecting

        if (Physics.Raycast(ground, out objHit))
        {
            distanceToGround = objHit.distance;        
        }

        if (Physics.Raycast(FootRay, out objHit, 0.5f)) //If ray detects collider
        {
            //Get the point hit on collider and return the height it was hit at
            float localPointHit = objHit.transform.InverseTransformPoint(objHit.point).y;

            float height = objHit.transform.localScale.y;   //get actual height of object
            height -= localPointHit;                        //subtract height from position hit
            height /= 2;                                    //Divide this value by 2
                                                            //Results in distance to top of object

            targetHeight = height;
        }
        Debug.DrawRay(LeftToe.position, new Vector3(-LeftToe.right.x, -LeftToe.right.y, 0.5f), Color.black);
    }
}
