using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    protected override void Start()
    {
        base.Start();
        structureType = StructureType.defense;
        VillagerAllocation villagerAllocation = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocataionWidgets")).GetComponent<VillagerAllocation>();
        villagerAllocation.SetTarget(this);
    }
}
