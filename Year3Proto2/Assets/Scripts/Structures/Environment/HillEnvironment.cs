using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillEnvironment : EnvironmentStructure
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        environmentType = EnvironmentType.hill;
        structureName = "Hills Environment";
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }
}
