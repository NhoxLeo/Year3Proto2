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
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.MetalStorage];
        maxHealth = 200f;
        health = maxHealth;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }
}
