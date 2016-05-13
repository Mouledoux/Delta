using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationMaster : MonoBehaviour
{
    Animation anim;
    Animator a;
    Motion motion;

    void Start()
    {
        anim = GetComponent<Animation>();
        a = GetComponent<Animator>();
    }

    void Update()
    {
        a.Play("", 0, 0.3f);
    }
}
