using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StorageStructure : Structure
{
    protected ResourceType resourceType;
    public int storage;

    protected void StorageStart()
    {
        StructureStart();
        structureType = StructureType.storage;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }
}
