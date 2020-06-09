using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ResourceStructure
{
    public bool wasPlacedOnHills = false;

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.metal;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Mine];
        health = maxHealth;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnHills)
        {
            tileBonus++;
        }
    }
}
