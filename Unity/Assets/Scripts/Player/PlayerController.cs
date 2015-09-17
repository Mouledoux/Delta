using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    //Variables
    public static float MoveSpeed = 6.0F;   //speed player moves
    public float rotationSpeed = 20.0f;     //speed player rotates
    public float gravity = 100f;

    void FixedUpdate()
    {
        float HorizontalRotate = Input.GetAxis("Mouse X");
        float VerticalRotate = Input.GetAxis("Mouse Y");
        float rotation = 0.0f;
        if (HorizontalRotate < 0 || HorizontalRotate > 0)
        {
            rotation = 10 *  HorizontalRotate * rotationSpeed * Time.deltaTime;
            gameObject.transform.Rotate(new Vector3(0.0f, rotation, 0.0f));
        }//*/

        if (VerticalRotate < 0 || VerticalRotate > 0)
        {
            rotation = VerticalRotate * rotationSpeed * Time.deltaTime;
            gameObject.transform.Find("Camera").gameObject.transform.Rotate(new Vector3(-rotation, 0.0f, 0.0f));
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void move(GameObject player, Vector3 move)
    {
        
        move = player.transform.TransformDirection(move);
        move *= MoveSpeed;
        player.transform.position += move * Time.deltaTime;
    }

    public static void MoveForward(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Moving Forward", true);
        move(player, new Vector3(0.0f, 0.0f, 1.0f));
    }

    public static void MoveBackward(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Moving Backward", true);
        move(player, new Vector3(0.0f, 0.0f, -0.5f));
    }

    public static void StrafeLeft(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Strafing Left",true);
        move(player, new Vector3(-0.6f, 0.0f, 0.0f));
    }

    public static void StrafeRight(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Strafing Right", true);
        move(player, new Vector3(0.6f, 0.0f, 0.0f));
    }

    public static void MoveForwardReset(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Moving Forward", false);
        move(player, new Vector3(0.0f, 0.0f, 1.0f));
    }

    public static void MoveBackwardReset(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Moving Backward", false);
        move(player, new Vector3(0.0f, 0.0f, -1.0f));
    }

    public static void StrafeLeftReset(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Strafing Left", false);
        move(player, new Vector3(-1.0f, 0.0f, 0.0f));
    }

    public static void StrafeRightReset(GameObject player)
    {
        player.GetComponent<Animator>().SetBool("Strafing Right", false);
        move(player, new Vector3(1.0f, 0.0f, 0.0f));
    }
}
