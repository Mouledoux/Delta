using UnityEngine;
using System.Collections;

public class IKController : MonoBehaviour {

    public float objPositionWeight;
    public float objRotationWeight;

    public Transform targetOne;
    public Transform targetTwo;

    private Animator anim;
    private Avatar avatar;
    

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        avatar = anim.avatar;
    }

    // Update is called once per frame
    void Update() {

    }

    void OnAnimatorIK()
    {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, objPositionWeight);
    }
}
