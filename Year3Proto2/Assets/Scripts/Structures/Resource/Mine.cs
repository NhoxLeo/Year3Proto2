using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : ResourceStructure
{
    public bool wasPlacedOnHills = false;
    protected Dictionary<TileBehaviour.TileCode, GameObject> scaffolding;

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Metal;
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Mine];
        health = maxHealth;
    }

    protected override void Start()
    {
        base.Start();
        scaffolding = new Dictionary<TileBehaviour.TileCode, GameObject>
        {
            { TileBehaviour.TileCode.north, transform.GetChild(0).gameObject },
            { TileBehaviour.TileCode.east, transform.GetChild(1).gameObject },
            { TileBehaviour.TileCode.south, transform.GetChild(2).gameObject },
            { TileBehaviour.TileCode.west, transform.GetChild(3).gameObject }
        };
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnHills)
        {
            tileBonus++;
        }
    }

    protected override void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {
        scaffolding[_side].SetActive(_exploit);
    }
}
