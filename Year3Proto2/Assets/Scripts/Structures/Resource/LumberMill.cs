using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    public bool wasPlacedOnForest = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Wood;
        structureName = StructureNames.LumberResource;
        health = maxHealth;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnForest)
        {
            tileBonus++;
        }
    }

    protected override void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {
        // intentionally does nothing, but must be implemented for the compiler
    }
}
