﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : ResourceStructure
{
    public bool wasPlacedOnPlains = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (superMan.CurrentLevelHasModifier(SuperManager.DryFields)) { batchSize = Mathf.CeilToInt(batchSize / 2f); }
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Food;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Farm];
        health = maxHealth;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnPlains)
        {
            tileBonus++;
        }
    }
}
