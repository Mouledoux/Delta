using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    //Global Variables
    //MOVEMENT
    public bool toggleMove;     //Can the player move

    public float speed;         //Speed player moves in free mode
    public float combatspeed;   //Speed player moves in combat mode
    public float startSpeed;    //Speed of transition from idle to top speed
    public float endSpeed;      //Speed of transition from top speed to idle

    private float movement;     //Speed player is currently using
    private float ForwardMove;
    private float HorizontalMove;

    private bool forward;       //Track state of forward key
    private bool back;          //Track state of back key
    private bool left;          //Track state of left key
    private bool right;         //Track state of right key
    private bool ctrl;          //Track state of control key
    private bool onHand;        //Track state of onhand attack key
    private bool offHand;       //Track state of offhand attack key
    private bool attacking;     //Is player attacking?

    private bool mode;          //Tracks state of combat/free mode key
    private bool roll;          //Track state of roll key

    private Vector3 moveVec;    //Vector that moves the player

    //CLASS INSTANCES
    private InputManager inputs;        //Used to create instance of Input Manager
    private Rigidbody rb;               //Handles the rigidbody component
    private Animator anim;              //Handles the animator component
    public GameObject cam;             //Gets camera gameobject
    private AnimatorStateInfo animinfo; //Gets info from the animator

    private Vector3 stillRotation;
    private float rotatetrack;

    //These bools tracks the state of the player:



    //WEAPON USAGE:
    private string Empty = "Empty";                 //Track state of empty hands
    private string OneHand = "OneHand";             //Track state of one-handed weapon (no shield)
    private string OneHandShield = "OneHandShield"; //Track state of one-handed weapon (shield)
    private string DualWield = "DualWield";         //Track state of dual-wield
    private string TwoHand = "TwoHand";             //Track state of two-handed weapon
    private string Bow = "Bow";                     //Track state of bow
    private string Crossbow = "Crossbow";           //Track state of crossbow
    private string Polearm = "Polearm";             //Track state of polearm

    public bool hold;
    float lastAttackTime = 0.0f;
    public float attackWait;
    int atkseq = 0;

    #region Updates and Start

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;   //Lock cursor
        //Cursor.visible = false;                     //Turn off cursor

        inputs = new InputManager();                //Initialize input manager
        anim = GetComponentInChildren<Animator>();  //Get animator

        moveVec = transform.position;               //Set movePosition to player's initial position
        anim.SetBool("Empty", true);                //Player initially has no weapon

        movement = speed * Time.deltaTime;          //Set movement speed == free mode speed
    }

    void FixedUpdate()
    {
        bool moving = false;
        //transform.forward = Vector3.Lerp(transform.forward, cam.transform.forward, 0.15f);

        if (toggleMove)
        {
            forward = Input.GetKey(inputs.Forward);     //Get forward key state
            back = Input.GetKey(inputs.Backward);    //Get back key state
            left = Input.GetKey(inputs.Left);        //Get left key state
            right = Input.GetKey(inputs.Right);       //Get right key state
            ctrl = Input.GetKeyDown(inputs.Mode);    //Get combat key state
            roll = Input.GetKeyDown(inputs.Roll);    //Get roll key state
        }

        onHand = Input.GetKey(inputs.OnHand);       //Get Onhand
        offHand = Input.GetKey(inputs.OffHand);     //Get Offhand


        animinfo = anim.GetCurrentAnimatorStateInfo(1);

        if (forward) //If forward key is being held
        {
            //Debug.Log(LeftFoot.localPosition.z);

            ForwardMove += startSpeed * cam.transform.forward.z * Time.deltaTime;
            ForwardMove = Mathf.Clamp(ForwardMove, 0, movement);
        }

        else if (back)   //If back key is being held
        {
            ForwardMove -= startSpeed * Time.deltaTime;
            ForwardMove = Mathf.Clamp(ForwardMove, -movement, 0);
        }

        if (left)   //If left key is being held    
        {
            float rotate;
            if (forward)
                rotate = 45.0f;
            else if (back)
                rotate = -45.0f;
            else
                rotate = 90.0f;

            HorizontalMove -= startSpeed * Time.deltaTime;
            HorizontalMove = Mathf.Clamp(HorizontalMove, -movement, 0);

            Vector3 rot = cam.transform.rotation.eulerAngles - new Vector3(0.0f, rotate, 0.0f);
            rb.MoveRotation(Quaternion.Lerp(transform.rotation, Quaternion.Euler(rot), 0.1f * Time.time));
        }


        else if (right)  //If right key is being held
        {
            float rotate;
            if (forward)
                rotate = 45.0f;
            else if (back)
                rotate = -45.0f;
            else
                rotate = 90.0f;

            HorizontalMove += startSpeed * Time.deltaTime;
            HorizontalMove = Mathf.Clamp(HorizontalMove, 0, movement);

            Vector3 rot = cam.transform.rotation.eulerAngles + new Vector3(0.0f, rotate, 0.0f);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.Euler(rot), 0.05f * Time.time));
        }

        moving = forward || back || right || left;

        if (!moving)
        {
            if (ForwardMove < 0)
                if (ForwardMove > -0.001)
                    ForwardMove = 0;
                else
                    ForwardMove += endSpeed * Time.deltaTime;

            else if (ForwardMove > 0)
                if (ForwardMove < 0.001)
                    ForwardMove = 0;

                else
                    ForwardMove -= endSpeed * Time.deltaTime;

            if (HorizontalMove < 0)
                if (HorizontalMove > -0.001)
                    HorizontalMove = 0;
                else
                    HorizontalMove += endSpeed * Time.deltaTime;

            else if (HorizontalMove > 0)
                if (HorizontalMove < 0.001)
                    HorizontalMove = 0;

                else
                    HorizontalMove -= endSpeed * Time.deltaTime;

        }

        moveVec = new Vector3(HorizontalMove, 0, ForwardMove);
        transform.position += moveVec;

        if (roll)   //If roll key has been pressed
        {
            setState("Rolling", true);                  //Execute "Roll" animation
            StartCoroutine(Roll());                     //Reset animation to false after playing
        }

        if (ctrl)  //If mode key is being held
        {
            setState("Mode", mode = !mode); //Switch modes
            if (!mode)
                movement = speed * Time.deltaTime;           //Adjust speed for free mode
            else
                movement = combatspeed * Time.deltaTime;     //Adjust speed for combat mode
        }

        if (moving)
        {
            if (!left && !right)
            {
                transform.forward = Vector3.Lerp(transform.forward, cam.transform.forward, 0.01f * Time.time);

            }
        }

        setMovementState(forward, back, left, right);       //Update animator based on movement
        setState("Attack_OneHand_1", onHand);
    }

    
    #endregion

    #region Animation Control
    ////////////////////////////////////////////////////////////////////////////////////////
    //Region is used to make adjustments to animations (ex. Adding bounce, update movement)//
    ////////////////////////////////////////////////////////////////////////////////////////

        //Change weapon animations based on current weapon held
    void changeWeaponAnimation(string currentstate, string newstate)
    {
        anim.SetBool(currentstate, false);  //Current weapon state is now false
        anim.SetBool(newstate, true);       //New weapon state is now true
    }

        //Adjusts parameters for movement
    void setMovementState(bool forward, bool back, bool left, bool right)
    {
        anim.SetBool("Forward Key", forward);   //Set forward key parameter
        anim.SetBool("Backward Key", back);     //Set back key parameter
        anim.SetBool("Left Key", left);         //Set left key parameter
        anim.SetBool("Right Key", right);       //Set right key parameter
    }

        //Change state of parameters in the Animator
    void setState(string parameter, bool state)
    {
        anim.SetBool(parameter, state);         //Set parameter == to state
    }
        //Change between combat and free modes
    void changeMode(bool mode)
    {
        anim.SetBool("Mode", mode);             //Set mode == to mode
    }

    void setInt(string parameter, int value)
    {
        anim.SetInteger(parameter, value);
    }


    #endregion

    #region CoRoutines

    //Coroutine for roll animation
    IEnumerator Roll()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);   //Get roll animation
        yield return new WaitForSeconds(info.length);                   //Wait for roll length
        setState("Rolling", false);                                     //Set state to false
    }

    IEnumerator Rotate(bool left)
    {
        float l_rotSpeed = 5;
        float rotation = 0;

        
        if (left)
        {
            Vector3 rot = transform.position - new Vector3(0.0f, 90.0f, 0.0f);
            Quaternion wantedRotation = Quaternion.LookRotation(transform.position - rot);
            while (rotation < 90)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        else
        {
            Vector3 rot = new Vector3(0.0f, 90.0f, 0.0f);
            while (rotation < 90)
            {
                gameObject.transform.Rotate(new Vector3(0, -l_rotSpeed, 0));
                rotation += l_rotSpeed;
                yield return new WaitForFixedUpdate();
            }
        }

        
    }
    #endregion
}
