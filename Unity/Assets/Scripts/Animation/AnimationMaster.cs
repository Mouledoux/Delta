using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationMaster : MonoBehaviour
{

    public Transform LeftFoot;
    public Transform RightFoot;
    public Animator Anim;
    private Quaternion LeftFootRot;

    public void InitializeFeet()
    {
        LeftFootRot = LeftFoot.localRotation;
    }

    public void CheckFoot()
    {
        if (LeftFootRot != LeftFoot.localRotation)
        {
        }
    }

    void MoveEntity()
    {
    }
}
