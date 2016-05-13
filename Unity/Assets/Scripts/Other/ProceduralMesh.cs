using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;

public class ProceduralMesh : MonoBehaviour {

    public float interpol;
    public Vector3 start, end;
    public float meshWidth;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	if (Input.GetKey(KeyCode.Y))
        {
            CreateMesh();
            //CreateHall(start, end);
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

    void CreateMeshRef(Vector3[] midPoints)
    {
        float offset = meshWidth / 2;

        GameObject obj = new GameObject("procmesh");
        obj.transform.position = Vector3.zero;
        Mesh test = new Mesh();
        Vector3[] verts = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 2), new Vector3(2, 0, 0), new Vector3(2, 0, 2) };
        int[] tri = new int[] { 0, 1, 2, 2, 1, 3 };

        test.vertices = verts;
        test.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 2), new Vector2(2, 0), new Vector2(2, 2) };
        test.triangles = tri;
        test.RecalculateNormals();
        test.RecalculateBounds();

        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.GetComponent<MeshFilter>().mesh = test;
        obj.GetComponent<MeshFilter>().mesh.name = "procmesh";
    }

    void CreateHall(Vector3 start, Vector3 end)
    {
        //check alignment
        //Cover shorter distance first

        int floorsNeeded = 2;
        float x, z;
        CheckAlignment(out x, out z);

        if (x == 0 || z == 0)
            floorsNeeded--;

        string larger = returnLarger(x,z);
        Vector3 half = end;

        switch (larger)
        {
            case "x":
                half.x = start.x;
                break;

            case "z":
                half.z = start.z;
                break;

            default:
                break;
        }

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);

        float ly = floor.transform.localScale.y;
        float lx = floor.transform.localScale.x;
        float lz = floor.transform.localScale.z;
        floor.transform.position = start;

        Debug.DrawLine(start, half);
        Debug.DrawLine(half, end);

    }

    void CheckAlignment(out float x, out float z)
    {
        x = Mathf.Abs(start.x - end.x);
        z = Mathf.Abs(start.z - end.z);
    }

    string returnLarger(float x, float z)
    {
        string s;

        if (x > z)
        {
            s = "x";
        }

        else if (z > x)
        {
            s = "z";
        }

        else
        {
            s = "";
        }

        return s;
    }
}
