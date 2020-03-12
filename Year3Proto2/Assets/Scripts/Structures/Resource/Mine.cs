using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ResourceStructure
{
    public bool wasPlacedOnHills = false;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        wasPlacedOnHills = false;
        resourceType = ResourceType.metal;
        structureName = "Mine";
        maxHealth = 100f;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        ResourceUpdate();
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
