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
        storage = 1000;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlace()
    {
        FindObjectOfType<GameManager>().CalculateStorageMaximum();
    }

    public override bool IsStructure(string _structureName)
    {
        return _structureName == "Lumber Pile";
    }
}
