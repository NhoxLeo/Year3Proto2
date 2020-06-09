using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : DefenseStructure
{
    private GameObject soldierPrefab;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Barracks];
        maxHealth = 200f;
        health = maxHealth;
    }
}
