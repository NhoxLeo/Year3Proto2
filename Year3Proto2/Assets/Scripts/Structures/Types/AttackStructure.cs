using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    public enum AttackType
    {
        magmaLauncher
    }

    protected AttackType attackType;

    public AttackType GetAttackType()
    {
        return attackType;
    }
    protected void AttackStart()
    {
        StructureStart();
        structureType = StructureType.attack;
    }
}
