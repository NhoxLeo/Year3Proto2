using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class StorageStructure : Structure
{
    protected ResourceType resourceType;
    public int storage;


    private void EnableFogMask()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).DOScale(Vector3.one * 2.0f, 1.0f).SetEase(Ease.OutQuint);
    }

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
        //EnableFogMask();
        gameMan.CalculateStorageMaximum();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        gameMan.CalculateStorageMaximum();
    }
}
