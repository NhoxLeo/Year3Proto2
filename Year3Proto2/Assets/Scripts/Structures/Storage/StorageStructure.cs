using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StorageStructure : Structure
{
    protected ResourceType resourceType;
    public int storage;

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.storage;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void OnPlace()
    {
        gameMan.CalculateStorageMaximum();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        gameMan.CalculateStorageMaximum();
    }
}
