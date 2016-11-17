﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor (typeof(StructuralGeneration))]
public class GenerationInspector : Editor {

    public override void OnInspectorGUI()
    {
        StructuralGeneration sg = (StructuralGeneration)target;

        sg.realTimeGen = EditorGUILayout.Toggle("Real Time Generation", sg.realTimeGen);
        sg.x_cells = EditorGUILayout.IntSlider("X size", sg.x_cells, 15, 100);
        sg.z_cells = EditorGUILayout.IntSlider("Z size", sg.z_cells, 15, 100);
        EditorGUILayout.Space();

        int[] seedArray = sg.getCurSeed;
        string seed = "";
        for (int i = 0; i < seedArray.Length; i++)
        {
            seed += seedArray[i];
        }

        sg.seeddisplay = EditorGUILayout.TextField("Seed:", seed);

        sg.startFloor = EditorGUILayout.IntSlider("Starting Floor", sg.startFloor, 1, 100);
        seedArray[0] = sg.startFloor;
        sg.getCurSeed = seedArray;

        sg.QuadrantsWanted = EditorGUILayout.IntField("Quadrants Wanted", sg.QuadrantsWanted);
        sg.roomsPerQuadrant = EditorGUILayout.IntSlider("Rooms per Quadrant", sg.roomsPerQuadrant, 2, 15);

        sg.cellSize = EditorGUILayout.Vector3Field("Cell Size", sg.cellSize);
        sg.cellWallHeight = EditorGUILayout.FloatField("Wall Height", sg.cellWallHeight);
        sg.cellWallWidth = EditorGUILayout.FloatField("Wall Width", sg.cellWallWidth);

        sg.WALL = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", sg.WALL, typeof(GameObject), true);
        sg.FLOOR = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", sg.FLOOR, typeof(GameObject), true);
    }
}
