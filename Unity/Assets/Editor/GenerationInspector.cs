using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor (typeof(StructuralGeneration))]
public class GenerationInspector : Editor {

    static List<string> savedSeeds = new List<string>();
    List<string> saveSeeds = savedSeeds;
    static int index = 0;

    public override void OnInspectorGUI()
    {
        savedSeeds = saveSeeds;
        saveSeeds = savedSeeds;
        StructuralGeneration sg = (StructuralGeneration)target;
        sg.realTimeGen = EditorGUILayout.Toggle("Real Time Generation", sg.realTimeGen);
        sg.xCells = EditorGUILayout.IntSlider("X size", sg.xCells, 3, 60);
        sg.zCells = EditorGUILayout.IntSlider("Z size", sg.zCells, 3, 60);
        sg.xQuadrants = EditorGUILayout.IntSlider("X Quadrants", sg.xQuadrants, 2, 10);
        sg.zQuadrants = EditorGUILayout.IntSlider("Z Quadrants", sg.zQuadrants, 2, 10);
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

        if (GUILayout.Button("Save Seed"))
        {
            if (!savedSeeds.Contains(sg.seedString))
            {
                savedSeeds.Add(sg.seedString);
                index = savedSeeds.Count - 1;
            }
        }

        sg.floor = EditorGUILayout.IntSlider("Floor", sg.floor, 1, 100);

        sg.setSeedAt(0, sg.floor);

        sg.minRoomSize = EditorGUILayout.IntSlider("Minimum Room Size", sg.minRoomSize, 1, 20);
        sg.maxRoomSize = EditorGUILayout.IntSlider("Max Room Room Size", sg.maxRoomSize, 1, 20);
        sg.roomsPerQuadrant = EditorGUILayout.IntSlider("Rooms per Quadrant", sg.roomsPerQuadrant, 2, 9);

        sg.cellSize = EditorGUILayout.Vector3Field("Cell Size", sg.cellSize);
        sg.cellWallHeight = EditorGUILayout.FloatField("Wall Height", sg.cellWallHeight);
        sg.cellWallWidth = EditorGUILayout.FloatField("Wall Width", sg.cellWallWidth);

        sg.WALL = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", sg.WALL, typeof(GameObject), true);
        sg.FLOOR = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", sg.FLOOR, typeof(GameObject), true);
    }

}
