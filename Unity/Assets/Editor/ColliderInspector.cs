using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

//Custom Editor for collider class.
//Used for visualizing the raycasts used for collision detecting
[CustomEditor (typeof(ColliderClass))]
public class ColliderInspector : Editor{

    public List<ColliderProperties> colProp;    //Used for getting and setting values of collider properties

    public override void OnInspectorGUI()
    {
        ColliderClass colClass = (ColliderClass)target;
        float defaultScale = colClass.defaultScale;
        //Number of colliders on the character
        colClass.numOfColliders = EditorGUILayout.IntSlider("Generate", colClass.numOfColliders, 0, 30);
        colClass.defaultScale = EditorGUILayout.FloatField("Default Scale", defaultScale);

        for (int i = 0; i < colProp.Count; i++)
        {
            GameObject col = colProp[i].collider;
            /*
            1. Is the collider active?
            2. Collider Name
            3. Shows field for game obj that collider is built around. Not necessary since object is empty
            4. Shows field for collider position
            5. Shows field for collider scale
            */
            colProp[i].active = EditorGUILayout.BeginToggleGroup(col.name, colProp[i].active);          //1
            colProp[i].collider.name = EditorGUILayout.TextField(colProp[i].collider.name);             //2
            //EditorGUILayout.ObjectField(colProp[i].collider, typeof(GameObject), true);               //3
            colProp[i].position = EditorGUILayout.Vector3Field("Position", colProp[i].position);        //4
            colProp[i].scale = EditorGUILayout.FloatField("Scale", colProp[i].scale);                   //5
            EditorGUILayout.EndToggleGroup();
            col.transform.position = colProp[i].position;
            Vector3 scale = new Vector3(colProp[i].scale, colProp[i].scale, colProp[i].scale);

            col.transform.localScale = scale;
        }

    }

    void OnSceneGUI()
    {
        ColliderClass colClass = (ColliderClass)target;
        float defaultScale = colClass.defaultScale;

        checkColliderList(colClass);
        colProp = colClass.Colliders;

        int collidersInScene = colProp.Count;

        if (collidersInScene < colClass.numOfColliders)
        {
            ColliderProperties collider = new ColliderProperties();

            collider.collider = new GameObject("Collider " + colProp.Count);
            collider.collider.transform.parent = colClass.gameObject.transform;
            collider.position = colClass.gameObject.transform.position;
            collider.scale = defaultScale / 10;
            collider.collider.transform.position = collider.position;
            Vector3 scale = new Vector3(collider.scale, collider.scale, collider.scale);
            collider.collider.transform.localScale = scale;

            colProp.Add(collider);
        }

        else if (collidersInScene > colClass.numOfColliders)
        {
            ColliderProperties collider = colProp[colProp.Count - 1];
            collider.collider.transform.parent = null;
            DestroyImmediate(collider.collider);
            colProp.RemoveAt(colProp.Count - 1);
        }

        for (int i = 0; i < colProp.Count; i++)
        {
            ColliderProperties collider = colProp[i];
            collider.position = collider.collider.transform.position;
            collider.scale = collider.collider.transform.localScale.x;
            colProp[i] = collider;
        }
        colClass.Colliders = colProp;
    }

    void checkColliderList(ColliderClass colClass)
    {
        for (int i = 0; i < colClass.Colliders.Count; i++)
        {
            if (!colClass.Colliders[i].collider)
            {
                colClass.Colliders.RemoveAt(i);
                colClass.numOfColliders--;
                i--;
            }
        }
    }
}
