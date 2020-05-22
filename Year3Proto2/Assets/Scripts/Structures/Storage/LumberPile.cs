using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberPile : StorageStructure
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.wood;
        storage = 500;
        structureName = "Lumber Pile";
        maxHealth = 200f;
        health = maxHealth;
    }
}
