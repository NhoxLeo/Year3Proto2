using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : ResourceStructure
{
    public bool wasPlacedOnPlains = false;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        wasPlacedOnPlains = false;
        resourceType = ResourceType.food;
        structureName = "Farm";
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
        if (wasPlacedOnPlains)
        {
            tileBonus++;
        }
    }
}
