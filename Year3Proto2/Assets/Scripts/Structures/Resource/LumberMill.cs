using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    public bool wasPlacedOnForest = false;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        wasPlacedOnForest = false;
        resourceType = ResourceType.wood;
        structureName = "Lumber Mill";
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
        if (wasPlacedOnForest)
        {
            tileBonus++;
        }
    }
}
