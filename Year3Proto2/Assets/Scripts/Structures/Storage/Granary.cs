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
}
