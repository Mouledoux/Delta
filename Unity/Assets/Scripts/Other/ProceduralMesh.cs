using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ProceduralMesh : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKeyDown(KeyCode.Y))
        {
            CreateMesh();
        }
	}

    void CreateMesh()
    {
        GameObject obj = new GameObject("procmesh");
        obj.transform.position = Vector3.zero;
        Mesh test = new Mesh();
        Vector3[] verts = new Vector3[] {new Vector3(0,0,0), new Vector3(0,0,2), new Vector3(2,0,0), new Vector3(2,0,2) };
        int[] tri = new int[] {0,1,2,2,1,3 };

        test.vertices = verts;
        test.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 0), new Vector2(2,2) };
        test.triangles = tri;
        test.RecalculateNormals();
        test.RecalculateBounds();

        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.GetComponent<MeshFilter>().mesh = test;
        obj.GetComponent<MeshFilter>().mesh.name = "procmesh";
    }
}
