using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    public bool wasPlacedOnForest = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.wood;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.LumberMill];
        health = maxHealth;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        foreach (LumberMill mill in FindObjectsOfType<LumberMill>())
        {
            mill.SetFoodAllocation(_allocation);
        }
    }

    public override void OnPlace()
    {
        base.OnPlace();
        LumberMill[] lumberMills = FindObjectsOfType<LumberMill>();
        if (lumberMills.Length >= 2)
        {
            LumberMill other = (lumberMills[0] == this) ? lumberMills[1] : lumberMills[0];
            SetFoodAllocation(other.foodAllocation);
        }
        if (wasPlacedOnForest)
        {
            tileBonus++;
        }
    }
}
