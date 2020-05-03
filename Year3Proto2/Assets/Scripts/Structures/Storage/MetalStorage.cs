using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalStorage : StorageStructure
{
    // Start is called before the first frame update
    void Start()
    {
        StorageStart();
        resourceType = ResourceType.metal;
        storage = 500;
        structureName = "Metal Storage";
        maxHealth = 200f;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
    }
}
