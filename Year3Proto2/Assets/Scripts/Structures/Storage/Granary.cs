using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granary : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Food;
        structureName = StructureNames.FoodStorage;
    }

    public override void SetColour(Color _colour)
    {
        meshRenderer.materials[0].SetColor("_BaseColor", _colour);
    }
}
