using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granary : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Food;
        storage = 500;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Granary];
        maxHealth = 200f;
        health = maxHealth;
    }
}
