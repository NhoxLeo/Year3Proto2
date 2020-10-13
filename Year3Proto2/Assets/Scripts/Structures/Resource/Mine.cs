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
        structureName = StructureNames.MetalResource;
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
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }

    protected override void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {
        scaffolding[_side].SetActive(_exploit);
    }

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
    }

    public override void SetMaterials(bool _snow)
    {
        base.SetMaterials(_snow);
        transform.GetChild(0).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(1).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(2).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
        transform.GetChild(3).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
    }
}
