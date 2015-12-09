using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    //Variables
    public float speed;             //Speed player moves
    public float combatspeed;       //Speed player moves in combat mode
    public float rotationSpeed;     //Speed player rotates
    public float gravity;           //Used to effect player falling
    private float movement;         //Actual value of speed that gets passed to rigidbody

    private InputManager inputs; //Used to create instance of Input Manager
    private Rigidbody rb;        //Handles the rigidbody component
    private Animator anim;       //Handles the animator component

    private Vector3 movePosition;   //Tracks direction of player movement
    private Vector3 moveRotation;   //Tracks direction of player rotation

    //These bools tracks the state of the player:

        //MOVEMENT:
    private bool forward;   //Track state of forward key
    private bool back;      //Track state of back key
    private bool left;      //Track state of left key
    private bool right;     //Track state of right key
    private bool ctrl;      //Track state of control key

    private bool mode;      //Tracks state of combat/free mode key
    private bool roll;      //Track state of roll key

        //WEAPON USAGE:
    private string Empty = "Empty";                 //Track state of empty hands
    private string OneHand = "OneHand";             //Track state of one-handed weapon (no shield)
    private string OneHandShield = "OneHandShield"; //Track state of one-handed weapon (shield)
    private string DualWield = "DualWield";         //Track state of dual-wield
    private string TwoHand = "TwoHand";             //Track state of two-handed weapon
    private string Bow = "Bow";                     //Track state of bow
    private string Crossbow = "Crossbow";           //Track state of crossbow
    private string Polearm = "Polearm";             //Track state of polearm

    public float zoomspeed;

    #region Updates and Start
    void FixedUpdate()
    {
        forward = Input.GetKey(inputs.Forward);     //Get forward key state
        back    = Input.GetKey(inputs.Backward);    //Get back key state
        left    = Input.GetKey(inputs.Left);        //Get left key state
        right   = Input.GetKey(inputs.Right);       //Get right key state
        ctrl    = Input.GetKeyDown(inputs.Mode);    //Get combat key state
        roll    = Input.GetKeyDown(inputs.Roll);    //Get roll key state

        float HorizontalRotate  = Input.GetAxis("Mouse X");     //Get horizontal mouse movement
        float VerticalRotate    = Input.GetAxis("Mouse Y");     //Get vertical mouse movement
        float zoom = Input.GetAxis("Mouse ScrollWheel");        //Get mouse scroll movement

        //Rotate player based on horizontal mouse movement multiplied by rotationSpeed
        moveRotation += new Vector3(0.0f, HorizontalRotate, 0.0f) * rotationSpeed * 10 * Time.deltaTime;

        Quaternion rotate = Quaternion.Euler(moveRotation); //Convert moveRotation into a quaternion
        rb.MoveRotation(rotate);                            //rotate player

        //If player moves mouse up or down, rotate the camera instead of the player
        //using vertical mouse movement multplied by rotation sped
        if (VerticalRotate < 0 || VerticalRotate > 0)
        {
            float rotation = VerticalRotate * rotationSpeed * Time.deltaTime;
            gameObject.transform.Find("Camera").gameObject.transform.Rotate(new Vector3(-rotation, 0.0f, 0.0f));
        }

        if (zoom > 0 || zoom < 0)
        {
            gameObject.transform.Find("Camera").gameObject.transform.position += (new Vector3(0.0f, 0.0f, zoom * zoomspeed));
        }

        if (forward) //If forward key is being held
        {
            movePosition += transform.forward * movement; //Add forward transform to movePosition
        }

        if (back)   //If back key is being held
        {
            movePosition -= transform.forward * movement; //Subtract forward transform from movePosition
        }

        if (left)   //If left key is being held    
        {
            movePosition -= transform.right * movement; //Subtract right transform from movePosition
        }

        if (right)  //If right key is being held
        {
            movePosition += transform.right * movement; //Add right transform from movePosition
        }

        if (roll)   //If roll key has been pressed
        {
            setState("Rolling", true);                  //Execute "Roll" animation
            StartCoroutine(Roll());                     //Reset animation to false after playing
        }

        if (ctrl)  //If mode key is being held
        {
            changeMode(mode = !mode);       //Switch modes
            if (!mode)
                movement = speed;           //Adjust speed for free mode
            else
                movement = combatspeed;     //Adjust speed for combat mode
        }

        setMovementState(forward, back, left, right);       //Update animator based on movement
        rb.MovePosition(movePosition * Time.deltaTime);     //Move player through rigidbody(free)


    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //Lock cursor
        Cursor.visible = false;                     //Turn off cursor

        rb = GetComponent<Rigidbody>();     //Get rigidbody
        inputs = new InputManager();        //Initialize input manager
        anim = GetComponent<Animator>();    //Get animator

        movePosition = transform.position;  //Set movePosition to player's initial position
        anim.SetBool("Empty", true);        //Player initially has no weapon

        movement = speed;                   //Set movement speed == free mode speed
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

        //Coroutine for roll animation
    IEnumerator Roll()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);   //Get roll animation
        yield return new WaitForSeconds(info.length);                   //Wait for roll length
        setState("Rolling", false);                                     //Set state to false
    }
    #endregion
}
