using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ResourceStructure
{
    public bool wasPlacedOnHills = false;

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Metal;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Mine];
        health = maxHealth;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        Mine[] mines = FindObjectsOfType<Mine>();
        if (mines.Length >= 2)
        {
            Mine other = (mines[0] == this) ? mines[1] : mines[0];
        }
        if (wasPlacedOnHills)
        {
            tileBonus++;
        }
    }
}
