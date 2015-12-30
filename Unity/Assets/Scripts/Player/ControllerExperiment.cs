using UnityEngine;
using System.Collections;
using System;

public class ControllerExperiment : AnimationMaster
{
    private InputManager inputs; //Used to create instance of Input Manager

    private bool forward;   //Track state of forward key
    private bool back;      //Track state of back key
    private bool left;      //Track state of left key
    private bool right;     //Track state of right key
    private bool ctrl;      //Track state of control key

    void Start()
    {
        inputs = new InputManager();        //Initialize input manager;
        Anim = GetComponent<Animator>();    //Initialize Animator
    }
    void FixedUpdate()
    {

        forward = Input.GetKey(inputs.Forward);     //Get forward key state
        back    = Input.GetKey(inputs.Backward);    //Get back key state
        left    = Input.GetKey(inputs.Left);        //Get left key state
        right   = Input.GetKey(inputs.Right);       //Get right key state
        ctrl    = Input.GetKeyDown(inputs.Mode);    //Get combat key state

        if (forward)
        {
            PlayAnimation("MoveForward");
            CheckFoot();
        }

    }
}
