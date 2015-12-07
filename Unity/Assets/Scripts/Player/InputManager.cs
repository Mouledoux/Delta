using UnityEngine;
using System.Collections;

public class InputManager
{
    // Movement
    public KeyCode Mode = KeyCode.LeftControl; //Switches to combat/free mode (only for testing)

    public KeyCode Forward  = KeyCode.W;    // Move Forward
    public KeyCode Backward = KeyCode.S;    // Move Backward
    public KeyCode Left     = KeyCode.A;    // Move Left
    public KeyCode Right    = KeyCode.D;    // Move Right

    public KeyCode Roll = KeyCode.Space; //Roll

    // Attacking/Defending
    public KeyCode OnHand   = KeyCode.Mouse0;   // Attack/Defend with primary
    public KeyCode OffHand  = KeyCode.Mouse1;   // Attack/Defend with secondary

    // Item Managment
    public KeyCode Inventory = KeyCode.E;   // Open Inventory/Equipment

	// Update is called once per frame
	void FixedUpdate ()
    {
	}


}
