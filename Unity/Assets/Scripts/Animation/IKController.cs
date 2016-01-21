using UnityEngine;
using System.Collections;

public class IKController : MonoBehaviour
{
    public Transform BaseJoint;
    public Transform MidJoint;
    public Transform EndJoint;

    private float BaseMidDistance;
    private float MidEndDistance;
    private float reachDistance;

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
        //endTarget.position = EndJoint.position;

        basePoint = BaseJoint.position;
        midPoint = MidJoint.position;
        endPoint = EndJoint.position;
        movement = endPoint;

        reachDistance = Vector3.Distance(BaseJoint.position, EndJoint.position);

    }

    void FixedUpdate()
    {
        Vector3 mejointVec = Vector3.MoveTowards(MidJoint.position, EndJoint.position,10.0f);
        Vector3 meTargetVec = Vector3.MoveTowards(MidJoint.position, endTarget.position, 10.0f);
        Vector3 bejointVec = Vector3.MoveTowards(BaseJoint.position, EndJoint.position, 10.0f);
        Vector3 beTargeVec = Vector3.MoveTowards(BaseJoint.position, endTarget.position, 10.0f);

        Debug.DrawLine(EndJoint.position, endTarget.position, Color.black);
        Debug.DrawLine(MidJoint.position, EndJoint.position, Color.black);
        Debug.DrawLine(MidJoint.position, endTarget.position, Color.black);
        //Debug.DrawLine(BaseJoint.position, EndJoint.position, Color.magenta);
        //Debug.DrawLine(BaseJoint.position, endTarget.position, Color.magenta);

        float a = Vector3.Distance(EndJoint.position, endTarget.position);
        float b = Vector3.Distance(MidJoint.position, EndJoint.position);
        float c = Vector3.Distance(MidJoint.position, endTarget.position);
        float angle = Mathf.Acos((-(a * a) + (b * b) + (c * c))/ (2 * b * c));
        Vector3 axis = Vector3.Cross(MidJoint.position, endTarget.position).normalized;

        angle = angle * Mathf.Rad2Deg;

        MidJoint.rotation = Quaternion.Slerp(MidJoint.rotation, Quaternion.AngleAxis(angle, axis), 0.01f);

        a = Vector3.Distance(EndJoint.position, endTarget.position);
        b = Vector3.Distance(BaseJoint.position, EndJoint.position);
        c = Vector3.Distance(BaseJoint.position, endTarget.position);
        angle = Mathf.Acos((-(a * a) + (b * b) + (c * c)) / (2 * b * c));
        axis = Vector3.Cross(BaseJoint.position, endTarget.position).normalized;

        angle = angle * Mathf.Rad2Deg;

        BaseJoint.rotation = Quaternion.Slerp(BaseJoint.rotation, Quaternion.AngleAxis(angle, axis), 0.1f);
    }

    void ikSolver()
    {

    }

}
