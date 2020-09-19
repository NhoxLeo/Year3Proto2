using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillEnvironment : EnvironmentStructure
{
    protected override void Awake()
    {
        base.Awake();
        environmentType = EnvironmentType.hill;
        structureName = StructureNames.MetalEnvironment;
    }
}
