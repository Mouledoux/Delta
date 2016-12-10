using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;

[CustomEditor (typeof(StructuralGeneration))]
public class GenerationInspector : Editor {

    static List<string> savedSeeds = new List<string>();
    List<string> saveSeeds = savedSeeds;
    static int index = 0;

    public override void OnInspectorGUI()
    {
        savedSeeds  = saveSeeds;
        saveSeeds   = savedSeeds;

        StructuralGeneration sg = (StructuralGeneration)target;

        sg.Generate     = EditorGUILayout.Toggle("Generate", sg.Generate);
        sg.realTimeGen  = EditorGUILayout.Toggle("Real Time Generation", sg.realTimeGen);
        sg.xCells       = EditorGUILayout.IntSlider("X size", sg.xCells, 9, 60);
        sg.zCells       = EditorGUILayout.IntSlider("Z size", sg.zCells, 9, 60);
        sg.xQuadrants   = EditorGUILayout.IntSlider("X Quadrants", sg.xQuadrants, 2, 10);
        sg.zQuadrants   = EditorGUILayout.IntSlider("Z Quadrants", sg.zQuadrants, 2, 10);
        sg.floor        = EditorGUILayout.IntSlider("Floor", sg.floor, 1, 100);

        if (!Application.isPlaying)
            sg.roomCycle();
        EditorGUILayout.IntField("Number of cells", sg.getCellCount());
        EditorGUILayout.Space();

        index = EditorGUILayout.Popup(index, savedSeeds.ToArray());

        if (GUILayout.Button("Use Seed"))
        {
            sg.seedString = savedSeeds[index].ToString();
            sg.seedStringToSeed();
            StructuralGeneration.ClearDungeon();
            sg.Generate = true;
        }

        if (GUILayout.Button("Clear Seeds"))
        {
            index = 0;
            savedSeeds = new List<string>();
            saveSeeds = new List<string>();
        }

        bool resetSeed = false;
        resetSeed = EditorGUILayout.Toggle("Reset Seed?", resetSeed);

        if (resetSeed)
        {
            sg.seedString = sg.getDefaultSeed();
            sg.seedStringToSeed();
        }

        bool resetFloor = false;
        resetFloor = EditorGUILayout.Toggle("Reset Floor?", resetFloor);

        if (resetFloor)
        {
            sg.floor = 0;
        }

        sg.seedString = "";
        foreach (int s in sg.getCurSeed)
        {
            sg.seedString += s.ToString();
        }

        sg.seedString = EditorGUILayout.TextField("Seed:", sg.seedString);
        sg.seedStringToSeed();
        sg.setSeedAt(0, sg.floor);

        if (GUILayout.Button("Save Seed"))
        {
            if (!savedSeeds.Contains(sg.seedString))
            {
                savedSeeds.Add(sg.seedString);
                index = savedSeeds.Count - 1;
            }
        }

        sg.minRoomSize = EditorGUILayout.IntSlider("Min Room Size", sg.minRoomSize, 1, 20);
        sg.maxRoomSize = EditorGUILayout.IntSlider("Max Room Size", sg.maxRoomSize, 1, 20);
        float medianRoomSize = ((sg.maxRoomSize - sg.minRoomSize) / 2) + sg.minRoomSize;
        sg.maxPerQuadrant = Mathf.RoundToInt((sg.getQuadrantSize / 2) / (medianRoomSize * medianRoomSize));

        if (sg.maxPerQuadrant < 2)
            Debug.LogError("Dungeon size is too small. May cause unwanted behaviours");

        sg.roomsPerQuadrant = EditorGUILayout.IntSlider("Per Quadrant", sg.roomsPerQuadrant, 2, sg.maxPerQuadrant);

        sg.cellSize = EditorGUILayout.Vector3Field("Cell Size", sg.cellSize);
        sg.cellWallHeight = EditorGUILayout.FloatField("Wall Height", sg.cellWallHeight);
        sg.cellWallWidth = EditorGUILayout.FloatField("Wall Width", sg.cellWallWidth);

        sg.WALL = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", sg.WALL, typeof(GameObject), true);
        sg.FLOOR = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", sg.FLOOR, typeof(GameObject), true);
    }

}
