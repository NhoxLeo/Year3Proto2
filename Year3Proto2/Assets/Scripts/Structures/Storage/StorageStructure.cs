﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class StorageStructure : Structure
{
    protected ResourceType resourceType;
    public int storage = 500;
    protected const float BaseMaxHealth = 300f;

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Storage;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void OnPlace()
    {
        base.OnPlace();
        SetMaterials(SuperManager.GetInstance().GetSnow());
        GameManager.GetInstance().CalculateStorageMaximum();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        GameManager.GetInstance().CalculateStorageMaximum();
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();
        
        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }
}
