using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberPile : StorageStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Wood;
        structureName = StructureNames.LumberStorage;
    }

    public override void SetColour(Color _colour)
    {
        meshRenderer.materials[0].SetColor("_BaseColor", _colour);
    }
}
