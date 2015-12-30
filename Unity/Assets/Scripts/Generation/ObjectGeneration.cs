using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class ObjectGeneration : StructuralGeneration
{
    //Script needs to add prefabs for objects

    //Enemies
    public GameObject EntitySpawn;
    public GameObject Agony;

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
    public int lootDistance;            //Any loot objects must maintain this distance from each other when spawning


    void SubdivideDungeon()
    {
        
        for (int i = 0; i < getCells.Count; i++)
        {

        }
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

    }
}
