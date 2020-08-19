using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;
    }

    public override void OnSelected()
    {
        base.OnSelected();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
    }
}
