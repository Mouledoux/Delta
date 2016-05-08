using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

//Custom Editor for collider class.
//Used for visualizing the raycasts used for collision detecting
[CustomEditor (typeof(ColliderClass))]
public class ColliderInspector : Editor{

    public List<ColliderProperties> colProp;    //Reference of ColliderClass.Colliders

    public override void OnInspectorGUI()
    {
        ColliderClass colClass = (ColliderClass)target; //Reference to the Collider Class
        float defaultScale = colClass.defaultScale;     //Get the default scale from the Collider Class

        //Number of colliders on the entity
        colClass.numOfColliders = EditorGUILayout.IntSlider("Generate", colClass.numOfColliders, 0, 30);

        //Default scale of each collider
        colClass.defaultScale = EditorGUILayout.FloatField("Default Scale", defaultScale);

        for (int i = 0; i < colProp.Count; i++)
        {
            GameObject col = colProp[i].collider;   //Use a reference for the collider's gameobject
            /*
            EditorGUILayout is used for custom inspector fields

            1. Is the collider active?  (Toggable boolean field)
            2. Collider Name            (Text field)
            3. Vector3 Position         (Vector3 Field)
            4. Vector3 Scale            (Vector3 Field)
            */
            colProp[i].active = EditorGUILayout.BeginToggleGroup(col.name, colProp[i].active);          //1
            colProp[i].collider.name = EditorGUILayout.TextField(colProp[i].collider.name);             //2
            colProp[i].position = EditorGUILayout.Vector3Field("Position", colProp[i].position);        //3
            colProp[i].scale = EditorGUILayout.FloatField("Scale", colProp[i].scale);                   //4
            EditorGUILayout.EndToggleGroup();

            //Override the collider's position with the one in the inspector
            col.transform.position = colProp[i].position;

            //Override the collider's scale with the one in the inspector
            Vector3 scale = new Vector3(colProp[i].scale, colProp[i].scale, colProp[i].scale);
            col.transform.localScale = scale;
        }

    }

    void OnSceneGUI()
    {
        ColliderClass colClass = (ColliderClass)target;         //Reference to Collider Class
        float defaultScale = colClass.defaultScale;             //Get default scale

        checkColliderList(colClass);                            //Make sure we have all colliders
        colProp = colClass.Colliders;                           //Reference Collider List

        int collidersInScene = colProp.Count;                   //Number of colliders in scene    

        if (collidersInScene < colClass.numOfColliders)         //If colliders in scene are less than number wanted
        {
            ColliderProperties collider = new ColliderProperties();                         //Create new collider

            collider.collider = new GameObject("Collider " + colProp.Count);                //Name collider
            collider.collider.transform.parent = colClass.gameObject.transform;             //Set parent to gameobject
            collider.position = colClass.gameObject.transform.position;                     //Set position
            collider.scale = defaultScale / 10;                                             //Set scale
            collider.collider.transform.position = collider.position;                       //Collider position = position
            Vector3 scale = new Vector3(collider.scale, collider.scale, collider.scale);    //Collider scale = scale   
            collider.collider.transform.localScale = scale;                                 //^^^^^^^^^^^^^^^^^^^^^^

            colProp.Add(collider);                                                          //Add collider to list
        }

        else if (collidersInScene > colClass.numOfColliders)    //If colliders in scene are more than number wanted
        {
            ColliderProperties collider = colProp[colProp.Count - 1];   //Reference to collider last in list
            collider.collider.transform.parent = null;                  //Set parent to null
            DestroyImmediate(collider.collider);                        //Destroy the gameobject
            colProp.RemoveAt(colProp.Count - 1);                        //Remove from list
        }

        for (int i = 0; i < colProp.Count; i++)                 //While i is less than list of colliders
        {
            ColliderProperties collider = colProp[i];                   //reference current collider in iteration
            collider.position = collider.collider.transform.position;   //set position to game object's position
            collider.scale = collider.collider.transform.localScale.x;  //set scale to gameobjects scale
            colProp[i] = collider;                                      //finalize the values
        }

        colClass.Colliders = colProp;                                   //Set the real list equal to the reference
    }

    void checkColliderList(ColliderClass colClass)
    {
        //Used to detect if any colliders have been deleted from scene view as opposed to the inspector

        for (int i = 0; i < colClass.Colliders.Count; i++)  //While i is less than list of colliders
        {
            if (!colClass.Colliders[i].collider)                //If the current collider iteration has null gameobject
            {
                colClass.Colliders.RemoveAt(i);                     //Remove from the list
                colClass.numOfColliders--;                          //Subtract from the number of colliders so another
                                                                    //doesn't get generated
                i--;                                                //Move up a step to properly detect other elements
            }
        }
    }
}
