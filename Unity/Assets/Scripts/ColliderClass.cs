using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[System.Serializable]
public class ColliderProperties
{
    public bool active = true;      //is the collider active
    public GameObject collider;     //empty game object where the gizmo will be drawn. Allows for easy scaling
    public Vector3 position;        //Position of the gameobject/gizmo
    public float scale;             //Scale of the gameobject/gizmo

}

public class ColliderClass : MonoBehaviour {

    public float defaultScale;                                                  //Default radius of the gizmo
    public int numOfColliders;                                                  //Number of colliders in the scene
    public List<ColliderProperties> Colliders = new List<ColliderProperties>(); //List of colliders

    void OnDrawGizmos()
    {
        //If selection isn't null and we select the game object or any of its children
            //For every collider in the List of Colliders
                //If the collider is active and has a gameobject
                    //Draw a sphere at the position with the scale

        if (Selection.activeTransform != null)
            if (Selection.activeTransform == transform || Selection.activeTransform.parent == transform)
                for (int i = 0; i < Colliders.Count; i++)
                {
                    if (Colliders[i].active && Colliders[i].collider)
                        Gizmos.DrawWireSphere(Colliders[i].collider.transform.position, Colliders[i].collider.transform.localScale.x);
                }
    }
}
