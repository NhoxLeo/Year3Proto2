using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberPile : StorageStructure
{
    // Start is called before the first frame update
    void Start()
    {
        StorageStart();
        resourceType = ResourceType.wood;
        storage = 500;
        structureName = "Lumber Pile";
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
