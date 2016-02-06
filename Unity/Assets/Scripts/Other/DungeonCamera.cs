using UnityEngine;
using System.Collections;

public class DungeonCamera : MonoBehaviour {
    public float speed;
    public float H_rotSpeed;
    public float V_rotSpeed;
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.forward * Time.deltaTime* speed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.right * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * Time.deltaTime * speed;
        }

        float HorizontalRot = Input.GetAxis("Mouse X");     //Get horizontal mouse movement
        float VerticalRot = Input.GetAxis("Mouse Y");     //Get vertical mouse movement
        transform.RotateAround(transform.position, transform.up, 20.0f * HorizontalRot * H_rotSpeed * Time.deltaTime);
        transform.RotateAround(transform.position, -transform.right, 20.0f * VerticalRot * V_rotSpeed * Time.deltaTime);
    }
}
