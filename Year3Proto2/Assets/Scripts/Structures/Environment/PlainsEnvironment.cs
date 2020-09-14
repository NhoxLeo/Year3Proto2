using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsEnvironment : EnvironmentStructure
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        environmentType = EnvironmentType.plains;
        structureName = StructureNames.FoodEnvironment;
    }
}
