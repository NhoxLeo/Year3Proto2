using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceStructure : Structure
{
    protected ResourceType resourceType;

    protected void ResourceStart()
    {
        StructureStart();
        structureType = StructureType.resource;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }
}
