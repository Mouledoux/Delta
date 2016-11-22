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
        StructuralGeneration sg = (StructuralGeneration)target;
        sg.realTimeGen = EditorGUILayout.Toggle("Real Time Generation", sg.realTimeGen);
        sg.x_cells = EditorGUILayout.IntSlider("X size", sg.x_cells, 21, 100);
        sg.z_cells = EditorGUILayout.IntSlider("Z size", sg.z_cells, 21, 100);
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


        sg.seedString = "";
        foreach (int s in sg.getCurSeed)
        {
            sg.seedString += s.ToString();
        }

        bool resetSeed = false;
        resetSeed = EditorGUILayout.Toggle("Reset Seed?", resetSeed);

        if (resetSeed)
        {
            sg.seedString = sg.getDefaultSeed();
        }

        bool resetFloor = false;
        resetFloor = EditorGUILayout.Toggle("Reset Floor?", resetFloor);

        if (resetFloor)
        {
            sg.floor = 0;
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

        sg.QuadrantsWanted = (sg.x_cells + sg.z_cells) / 8;
        sg.QuadrantsWanted = sg.evenQuadrant() - sg.roomsPerQuadrant;
        sg.QuadrantsWanted = sg.evenQuadrant();
        EditorGUILayout.IntField("Quadrants Wanted", sg.QuadrantsWanted);
        sg.roomsPerQuadrant = EditorGUILayout.IntSlider("Rooms per Quadrant", sg.roomsPerQuadrant, 2, (sg.QuadrantsWanted == 4) ? sg.roomsPerQuadrant : 2 + (sg.QuadrantsWanted / 4));

        sg.cellSize = EditorGUILayout.Vector3Field("Cell Size", sg.cellSize);
        sg.cellWallHeight = EditorGUILayout.FloatField("Wall Height", sg.cellWallHeight);
        sg.cellWallWidth = EditorGUILayout.FloatField("Wall Width", sg.cellWallWidth);

        sg.WALL = (GameObject)EditorGUILayout.ObjectField("Wall Prefab", sg.WALL, typeof(GameObject), true);
        sg.FLOOR = (GameObject)EditorGUILayout.ObjectField("Floor Prefab", sg.FLOOR, typeof(GameObject), true);
    }

}
