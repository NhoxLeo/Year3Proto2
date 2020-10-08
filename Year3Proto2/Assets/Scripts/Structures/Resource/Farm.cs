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
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.DryFields))
        {
            batchSize = Mathf.CeilToInt(batchSize / 2f);
        }
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
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }

    protected override void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {
        fences[_side].SetActive(_exploit);
        closedFences[_side].SetActive(!_exploit);
    }

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
        meshRenderer.materials[1].SetColor("_Color", _colour);
        meshRenderer.materials[2].SetColor(colourReference, _colour);
    }

    public override void SetMaterials(bool _snow)
    {
        base.SetMaterials(_snow);
        transform.GetChild(0).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(1).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(2).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(3).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(4).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(5).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(6).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(7).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
    }
}
