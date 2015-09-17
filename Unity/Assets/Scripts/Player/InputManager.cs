using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour
{
    // Movement
    public KeyCode Forward  = KeyCode.W;    // Move Forward
    public KeyCode Backward = KeyCode.S;    // Move Backward
    public KeyCode Left     = KeyCode.A;    // Move Left
    public KeyCode Right    = KeyCode.D;    // Move Right

    // Attacking/Defending
    public KeyCode OnHand   = KeyCode.Mouse0;   // Attack/Defend with primary
    public KeyCode OffHand  = KeyCode.Mouse1;   // Attack/Defend with secondary

    // Item Managment
    public KeyCode Inventory = KeyCode.I;   // Open Inventory/Equipment

	// Update is called once per frame
	void FixedUpdate ()
    {

	    if (Input.GetKey(Forward))
        {
            PlayerController.MoveForward(gameObject);
        }

        if (Input.GetKey(Backward))
        {
            PlayerController.MoveBackward(gameObject);
        }

        if (Input.GetKey(Left))
        {
            PlayerController.StrafeLeft(gameObject);
        }

        if (Input.GetKey(Right))
        {
            PlayerController.StrafeRight(gameObject);
        }

        //////////////////////////////////////////////////////////////////////
        //Reset the controls on the frame the player stops pressing an input/
        ////////////////////////////////////////////////////////////////////

        if (Input.GetKeyUp(Forward))
        {
            PlayerController.MoveForwardReset(gameObject);
        }

        if (Input.GetKeyUp(Backward))
        {
            PlayerController.MoveBackwardReset(gameObject);
        }

        if (Input.GetKeyUp(Left))
        {
            PlayerController.StrafeLeftReset(gameObject);
        }

        if (Input.GetKeyUp(Right))
        {
            PlayerController.StrafeRightReset(gameObject);
        }

        if (Input.GetKey(OnHand))
        {

        }

        if (Input.GetKey(OffHand))
        {

        }

        if (Input.GetKeyDown(Inventory))
        {

        }

	}


}
