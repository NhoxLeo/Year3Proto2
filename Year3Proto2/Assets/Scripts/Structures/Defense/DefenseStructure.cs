using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;
        villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        //FindObjectOfType<HUDManager>().ShowOneVillagerWidget(villagerWidget);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        //FindObjectOfType<HUDManager>().HideAllVillagerWidgets();
    }
}
