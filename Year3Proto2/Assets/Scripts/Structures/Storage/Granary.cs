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
        storage = 500;
        structureName = "Granary";
        maxHealth = 200f;
        health = maxHealth;
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
