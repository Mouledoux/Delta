using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment : MonoBehaviour
{
    public GameObject onHand;           // Current primary weapon
    public GameObject offHand;          // Current secondary weapon

    public GameObject onHandBack;       // Extra primary weapond
    public GameObject offHandBack;      // Extra secondary weapon

    public GameObject armour;           // Current armour

    public GameObject temp;             // Temporary object for swapping and picking up weapons

    public List<GameObject> runeBag;    // Storage for equipment augmentations

    // Resets GameObject's Transform
    void ResetTransform(GameObject object1)
    {
        object1.transform.localPosition = new Vector3   (0, 0, 0);      // Sets Position relitave to Parent to 0
        object1.transform.localRotation = new Quaternion(0, 0, 0, 0);   // Sets Rotation relative to parent to 0
        object1.transform.localScale = new Vector3(1, 1, 1);            // Makes sure GameObject has not been rescaled
    }

    // Swaps Children of GameObjects
    public void Switch(GameObject object1, GameObject object2)
    {
        if (object1.transform.childCount == 1 && object2.transform.childCount == 1)     // If both GameObjects have a Child
        {                                                                               // 
            object1.transform.GetChild(0).transform.parent = object2.transform;             // Make object2 the new Parent of the first Child of object1
            object2.transform.GetChild(0).transform.parent = object1.transform;             // Make object1 the new Parent of the first Child of object2
            ResetTransform(object1.transform.GetChild(0).gameObject);                       // Reset object1's new Child's Transform to 0
            ResetTransform(object2.transform.GetChild(0).gameObject);                       // Reset object2's new Child's Transform to 0
        }
        else if (object1.transform.childCount == 1)                                     // Else if only object1 has a Child
        {                                                                               //
            object1.transform.GetChild(0).transform.parent = object2.transform;             // Make object2 the new Parent of the only Child of object1
            ResetTransform(object2.transform.GetChild(0).gameObject);                       // Reset object2's new Child's Transform to 0
        }
        else if (object2.transform.childCount == 1)                                     // Else if only object2 has a Child
        {                                                                               //
            object2.transform.GetChild(0).transform.parent = object1.transform;             // Make object1 the new Parent of the only Child of object2
            ResetTransform(object1.transform.GetChild(0).gameObject);                       // Reset object1's new Child's Transform to 0
        }
    }

    // Swaps current on hand weapond for current off hand weapond
    public void SwitchHands()
    {
        Switch(onHand, offHand);
    }

    // Swaps current on hand weapond for extra on hand weapon
    public void SwitchOnHandWeapon()
    {
        Switch(onHand, onHandBack);
    }

    // Swaps current off hand weapond for extra off hand weapon
    public void SwitchOffHandWeapon()
    {
        Switch(offHand, offHandBack);
    }

    // Swaps both an hand and off hand weapons for extras
    public void SwitchBothWeapons()
    {
        SwitchOnHandWeapon();       // Swaps on hand weapons
        SwitchOffHandWeapon();      // Swaps off hand waepons
    }

    // Handles picking up with onHand
    public void PickupOnHand(GameObject pickup)
    {
        if(onHand.transform.childCount == 0)                        // If onHand has no Child
        {                                                           //
            pickup.transform.parent = onHand.transform;                 // Make pickup a Child of onHand
            ResetTransform(pickup);                                     // Reset pickup's Transform to 0
        }
        else if (onHandBack.transform.childCount == 0)              // Else if onHandBack has no Child
        {                                                           //
            Switch(onHand, onHandBack);                                 // Switch onHand to onHandBack
            PickupOnHand(pickup);                                       // Retry
        }
        else                                                        // Else if both onHand and onHandback have a Child
        {                                                           //
            onHand.transform.GetChild(0).transform.parent = null;       // Remove onHand's Child
            PickupOnHand(pickup);                                       // Retry
        }
    }

    // Handles picking up with offHand
    public void PickupOffHand(GameObject pickup)
    {
        if (offHand.transform.childCount == 0)                      // If offHand has no Child
        {                                                           //
            pickup.transform.parent = offHand.transform;                // Make pickup a Child of offHand
            ResetTransform(pickup);                                     // Reset pickup's Transform to 0
        }
        else if (onHandBack.transform.childCount == 0)              // Else if offHandBack has no Child
        {                                                           //
            Switch(offHand, onHandBack);                                // Switch offHand to offHandBack
            PickupOffHand(pickup);                                      // Retry
        }
        else                                                        // Else if both offHand and offHandback have a Child
        {                                                           //
            offHand.transform.GetChild(0).transform.parent = null;      // Remove offHand's Child
            PickupOffHand(pickup);                                      // Retry
        }
    }

    /// <TESTING>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            SwitchOnHandWeapon();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchOffHandWeapon();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchBothWeapons();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchHands();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform.parent == null)
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                PickupOnHand(other.gameObject);
            }
        }
    }
}