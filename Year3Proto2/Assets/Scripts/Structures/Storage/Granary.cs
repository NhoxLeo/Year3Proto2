using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granary : StorageStructure
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.food;
        storage = 500;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Granary];
        maxHealth = 200f;
        health = maxHealth;
    }
}
