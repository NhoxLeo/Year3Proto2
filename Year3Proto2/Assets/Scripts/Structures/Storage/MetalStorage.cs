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
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
    }

    public override void OnPlace()
    {
        FindObjectOfType<GameManager>().CalculateStorageMaximum();
    }
}
