using UnityEngine;
using System.Collections;

public class MiniMap : MonoBehaviour {

    public float cameraHeight;
    Vector3 cameraVector;
    void Start()
    {
    }

    void Update()
    {
        cameraVector = new Vector3(0.0f, cameraHeight, 0.0f);
        gameObject.transform.position = GameObject.Find("Player").transform.position + cameraVector;
    }
}
