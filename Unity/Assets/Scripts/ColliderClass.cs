using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class ColliderProperties
{
    public bool active = true;
    public GameObject collider;
    public Vector3 position;
    public float scale;

}

public class ColliderClass : MonoBehaviour {

    public float defaultScale;
    public int numOfColliders;
    public List<ColliderProperties> Colliders;
    public List<ColliderProperties> DeletedColliders;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmos()
    {
        if (Selection.activeTransform != null)
        if (Selection.activeTransform == transform || Selection.activeTransform.parent == transform)
        for (int i = 0; i < Colliders.Count; i++)
        {
            if (Colliders[i].active && Colliders[i].collider)
            Gizmos.DrawWireSphere(Colliders[i].collider.transform.position, Colliders[i].collider.transform.localScale.x);
        }
    }
}
