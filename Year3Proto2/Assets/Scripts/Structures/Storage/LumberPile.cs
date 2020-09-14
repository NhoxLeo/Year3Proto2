using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberPile : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Wood;
        storage = 500;
        structureName = StructureNames.LumberStorage;
        maxHealth = 200f;
        health = maxHealth;
    }
}
