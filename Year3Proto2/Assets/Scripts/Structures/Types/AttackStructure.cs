using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackStructure : Structure
{
    public override void Check(GameObject gameobject)
    {

    }

    protected void AttackStart()
    {
        StructureStart();
        structureType = StructureType.attack;
    }
}
