using UnityEngine;
using System.Collections;

public class IKController : MonoBehaviour
{
    public Transform BaseJoint;
    public Transform MidJoint;
    public Transform EndJoint;

    private float BaseMidDistance;
    private float MidEndDistance;

    public Transform endTarget;

    private Quaternion BaseRotation;

    private Vector3 basePoint;
    private Vector3 midPoint;
    private Vector3 endPoint;

    private Ray d;

    Vector3 movement;

    void Start()
    {
        BaseMidDistance = Vector3.Distance(BaseJoint.position, MidJoint.position);
        MidEndDistance  = Vector3.Distance(MidJoint.position, EndJoint.position);
        BaseRotation = BaseJoint.rotation;
        endTarget.position = EndJoint.position;
        Debug.Log(MidEndDistance);

        basePoint = BaseJoint.position;
        midPoint = MidJoint.position;
        endPoint = EndJoint.position;
        movement = endPoint;

    }

    void FixedUpdate()
    {
        float angle = Vector3.Angle(MidJoint.position, EndJoint.position);
        Debug.Log(angle);
        Debug.DrawLine(MidJoint.position, Vector3.zero);
        Debug.DrawLine(EndJoint.position, Vector3.zero);
    }

}
