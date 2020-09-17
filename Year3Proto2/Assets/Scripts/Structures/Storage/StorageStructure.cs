using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class StorageStructure : Structure
{
    protected ResourceType resourceType;
    public int storage;

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Storage;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        GameManager.GetInstance().CalculateStorageMaximum();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        GameManager.GetInstance().CalculateStorageMaximum();
    }
}
