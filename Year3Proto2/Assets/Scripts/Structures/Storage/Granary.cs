using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granary : StorageStructure
{
    // Start is called before the first frame update
    void Start()
    {
        StorageStart();
        resourceType = ResourceType.food;
        storage = 1000;
        structureName = "Granary";
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
    }

    public void OnPlace()
    {
        FindObjectOfType<GameManager>().CalculateStorageMaximum();
    }
}
