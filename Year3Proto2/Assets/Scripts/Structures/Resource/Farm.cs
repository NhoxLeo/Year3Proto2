using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : ResourceStructure
{
    public bool wasPlacedOnPlains = false;
    protected Dictionary<TileBehaviour.TileCode, GameObject> fences;
    protected Dictionary<TileBehaviour.TileCode, GameObject> closedFences;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        fences = new Dictionary<TileBehaviour.TileCode, GameObject>
        {
            { TileBehaviour.TileCode.north, transform.GetChild(0).gameObject },
            { TileBehaviour.TileCode.east, transform.GetChild(1).gameObject },
            { TileBehaviour.TileCode.south, transform.GetChild(2).gameObject },
            { TileBehaviour.TileCode.west, transform.GetChild(3).gameObject }
        };
        closedFences = new Dictionary<TileBehaviour.TileCode, GameObject>
        {
            { TileBehaviour.TileCode.north, transform.GetChild(4).gameObject },
            { TileBehaviour.TileCode.east, transform.GetChild(5).gameObject },
            { TileBehaviour.TileCode.south, transform.GetChild(6).gameObject },
            { TileBehaviour.TileCode.west, transform.GetChild(7).gameObject }
        };
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.DryFields)) { batchSize = Mathf.CeilToInt(batchSize / 2f); }
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Food;
        structureName = StructureNames.FoodResource;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnPlains)
        {
            tileBonus++;
        }
    }

    protected override void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {
        fences[_side].SetActive(_exploit);
        closedFences[_side].SetActive(!_exploit);
    }
}
