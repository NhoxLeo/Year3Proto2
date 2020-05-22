﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalStorage : StorageStructure
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.metal;
        storage = 500;
        structureName = "Metal Storage";
        maxHealth = 200f;
        health = maxHealth;
    }
}
