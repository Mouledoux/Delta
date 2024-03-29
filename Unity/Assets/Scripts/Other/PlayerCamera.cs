﻿using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour {

    public float H_rotSpeed;        //Speed camera rotates horizontally
    public float V_rotSpeed;        //Speed camera rotates vertically
    public float zoomspeed;

    Vector3 originalPos;

    public GameObject player;

    // Update is called once per frame
    void Update () {

        float HorizontalRot = Input.GetAxis("Mouse X");     //Get horizontal mouse movement
        float VerticalRot = Input.GetAxis("Mouse Y");     //Get vertical mouse movement
        float zoom = Input.GetAxis("Mouse ScrollWheel");        //Get mouse scroll movement

        if (zoom != 0)
        {
            transform.localPosition += transform.forward * zoom * zoomspeed;
        }

        if (originalPos != player.transform.position)
        {
            transform.position += player.transform.position - originalPos;
            originalPos = player.transform.position;
        }

        Vector3 rot = new Vector3(0, HorizontalRot * H_rotSpeed, 0);
        transform.Rotate(rot);
        //transform.RotateAround(player.transform.position, player.transform.up, 20.0f * HorizontalRot * H_rotSpeed * Time.deltaTime);
        //transform.Rotate(new Vector3(-1.0f * VerticalRot * V_rotSpeed, 0, 0));
        //transform.RotateAround(-player.transform.position, player.transform.right, 20.0f * VerticalRot * V_rotSpeed * Time.deltaTime);
        Vector3 tem = transform.rotation.eulerAngles;
        tem.z = 0.0f;
        transform.rotation = Quaternion.Euler(tem);

    }

    void Start()
    {
        originalPos = player.transform.position;
    }
}
