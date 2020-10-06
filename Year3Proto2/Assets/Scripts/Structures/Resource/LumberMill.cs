using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    public bool wasPlacedOnForest = false;

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Wood;
        structureName = StructureNames.LumberResource;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        if (wasPlacedOnForest)
        {
            tileBonus++;
        }
        SetMaterials(SuperManager.GetInstance().GetSnow());
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
}
