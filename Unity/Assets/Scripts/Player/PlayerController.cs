using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //Variables
    public float speed;             //Speed player moves
    public float rotationSpeed;     //Speed player rotates
    public float gravity;           //Used to effect player falling

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

    private bool mode;      //Tracks state of combat/free modes

    private bool roll;      //Track state of roll

        //WEAPON USAGE:
    private bool OneHand;   //Track state of one-handed weapon (no shield)
    private bool TwoHand;   //Track state of two-handed weapon
    
    #region Updates and Start
    void FixedUpdate()
    {
        forward = Input.GetKey(inputs.Forward);     //Get forward key state
        back    = Input.GetKey(inputs.Backward);    //Get back key state
        left    = Input.GetKey(inputs.Left);        //Get left key state
        right   = Input.GetKey(inputs.Right);       //Get right key state

        float HorizontalRotate  = Input.GetAxis("Mouse X");  //Get horizontal mouse movement
        float VerticalRotate    = Input.GetAxis("Mouse Y");    //Get vertical mouse movement

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

        if (forward) //If forward key is being held
        {
            movePosition += transform.forward; //Add forward transform to movePosition
        }

        if (back)   //If back key is being held
        {
            movePosition -= transform.forward; //Subtract forward transform from movePosition
        }

        if (left)   //If left key is being held    
        {
            movePosition -= transform.right; //Subtract right transform to movePosition
        }

        if (right)  //If right key is being held
        {
            movePosition += transform.right; //Subtract right transform from movePosition
        }

        rb.MovePosition(movePosition * speed * Time.deltaTime); //Move player through rigidbody
        setMovementState(forward, back, left, right);           //Update animator based on movement
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;   //Lock cursor
        Cursor.visible = false;                     //Turn off cursor

        rb = GetComponent<Rigidbody>();     //Get rigidbody
        inputs = new InputManager();        //Initialize input manager
        anim = GetComponent<Animator>();    //Get animator

        movePosition = transform.position; //Set movePosition to player's initial position
    }
    #endregion

    #region Animation Control

    void changeLayer()
    {

    }

    //Make adjustments to animations based on layer (ex. Adding bounce)
    void layerAdjustments()
    {

    }

    void AnimationStateController()
    {

    }

    void setMovementState(bool forward, bool back, bool left, bool right)
    {
        anim.SetBool("Moving Forward", forward);
        anim.SetBool("Moving Backward", back);
        anim.SetBool("Moving Left", left);
        anim.SetBool("Moving Right", right);
    }

    #endregion
}
