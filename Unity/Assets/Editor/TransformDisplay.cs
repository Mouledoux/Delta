using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Transform))]
public class TransfromDisplay : Editor {

    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;

        EditorGUILayout.BeginHorizontal();
        t.position = EditorGUILayout.Vector3Field("World Transform", t.position);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        t.localPosition = EditorGUILayout.Vector3Field("Local Transform", t.localPosition);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        Vector3 rotation = EditorGUILayout.Vector3Field("Rotation", t.eulerAngles);
        EditorGUILayout.EndHorizontal();

        t.rotation = Quaternion.Euler(rotation);

        EditorGUILayout.BeginHorizontal();
        t.localScale = EditorGUILayout.Vector3Field("Scale" , t.localScale);
        EditorGUILayout.EndHorizontal();
    }
}
