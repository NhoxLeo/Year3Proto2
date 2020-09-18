using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalStorage : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Metal;
        storage = 500;
        structureName = StructureNames.MetalStorage;
        maxHealth = 200f;
        health = maxHealth;
    }
}
