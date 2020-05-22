using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ResourceStructure
{
    public bool wasPlacedOnHills = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        resourceType = ResourceType.metal;
        structureName = "Mine";
        health = maxHealth;
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.metal;
        structureName = "Mine";
        health = maxHealth;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnHills)
        {
            tileBonus++;
        }
    }
}
