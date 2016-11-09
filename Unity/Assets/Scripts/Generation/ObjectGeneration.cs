using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class ObjectGeneration : MonoBehaviour
{
    //Script needs to add prefabs for objects

    //Enemies
    public GameObject EntitySpawn;
    public GameObject Enemy;

    //Loot
    public GameObject Chest;
    public GameObject WeaponRack;

    //Lighting
    public GameObject Torch;
    public GameObject FirePit;

    //Traps
    public GameObject Trap;

    //Immersive
    public GameObject HealingWell;
    public GameObject Altar;

    public float tileSizes = 0.25f;     //Subdives cells as necessary
    public float lootChance;            //Chance of loot spawning

    bool done = false;
    bool creation = false;

    List<GameObject> EnemySpawns;

    public int EnemiesPerRoom;


    void SubdivideDungeon()
    {
    }

    void EnemySpawning()
    {

    }

    void LootSpawning()
    {

    }

    void LightSpawning()
    {

    }

    void DecorativeSpawning()
    {

    }

    void Start()
    {
        StartCoroutine(checkStructure());
    }

    void Update()
    {
        if (creation)
        {
            CreateSpawn();
        }

        if (!StructuralGeneration.structureDone && GameObject.Find("Spawns"))
        {
            creation = false;
            Destroy(GameObject.Find("Spawns"));
            StartCoroutine(checkStructure());
        }

    }

    IEnumerator checkStructure()
    {
        int x = 0;
        while (!StructuralGeneration.structureDone)
        {
            yield return new WaitForSeconds(1);
            x++;
        }
        creation = true;

        StopAllCoroutines();
    }

    void CreateSpawn()
    {
        EnemySpawns = new List<GameObject>();
        List<Vector3> roomCenters = new List<Vector3>();
        GameObject Rooms = GameObject.Find("Rooms");
        for (int i = 0; i < Rooms.transform.childCount; i++)
        {
            GameObject child = Rooms.transform.GetChild(i).gameObject;
            roomCenters.Add(child.transform.GetChild(0).transform.position);
        }

        for (int i = 0; i < roomCenters.Count; i++)
        {
            GameObject spawn = Instantiate(EntitySpawn);
            spawn.transform.position = roomCenters[i];
            EnemySpawns.Add(spawn);
        }

        UniversalHelper.parentObject(EnemySpawns, "Spawns");

        placeEnemies();

        creation = false;
    }

    void placeEnemies()
    {
        List<GameObject> enemies = new List<GameObject>();
        for (int i = 0; i < EnemySpawns.Count; i++)
        {
            for (int x = 0; x < EnemiesPerRoom; x++)
            {
                GameObject enemy = Instantiate(Enemy);
                enemy.transform.position = EnemySpawns[i].transform.position;
                enemies.Add(enemy);
            }
        }

        UniversalHelper.parentObject(enemies, "Enemies");
    }
}
