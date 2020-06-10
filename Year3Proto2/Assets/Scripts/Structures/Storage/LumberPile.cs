using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberPile : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.wood;
        storage = 500;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.LumberPile];
        maxHealth = 200f;
        health = maxHealth;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }
}
