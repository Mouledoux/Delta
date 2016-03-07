using UnityEngine;
using System.Collections;
using System;

public class BasicMovement : MonoBehaviour {


    private InputManager input;
    private GameObject cam;
    public float speed;

	// Use this for initialization
	void Start ()
    {
        input = new InputManager();
        cam = GameObject.Find("Camera");
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetKey(KeyCode.W))
        {
            transform.position += cam.transform.forward * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= cam.transform.right * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= cam.transform.forward * speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.position += cam.transform.right * speed * Time.deltaTime;
        }

    }
}
