using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsEnvironment : EnvironmentStructure
{
    protected override void Awake()
    {
        base.Awake();
        environmentType = EnvironmentType.plains;
        structureName = StructureNames.FoodEnvironment;
    }

    public override float GetBaseMaxHealth()
    {
        return 100f;
    }

    public override float GetTrueMaxHealth()
    {
        return GetBaseMaxHealth();
    }
}
