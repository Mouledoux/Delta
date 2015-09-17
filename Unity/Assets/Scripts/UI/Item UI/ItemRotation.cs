using UnityEngine;
using System.Collections;

public class ItemRotation : MonoBehaviour {
    Quaternion rotate = new Quaternion();
    public float rotationspeed = 1.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        rotate.eulerAngles = new Vector3(0.0f, rotationspeed, 0.0f);
        rotate.eulerAngles *= rotationspeed;
        Quaternion childrotation = gameObject.transform.GetChild(0).gameObject.transform.rotation;
        gameObject.transform.Rotate(rotate.eulerAngles);
        gameObject.transform.GetChild(0).gameObject.transform.rotation = childrotation;
	}
}
