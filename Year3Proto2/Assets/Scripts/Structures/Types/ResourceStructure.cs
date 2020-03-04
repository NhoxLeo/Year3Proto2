using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStructure : Structure
{
    protected ResourceType resourceType;

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void Check(GameObject gameobject)
    {

    }

    protected void ResourceStart()
    {
        StructureStart();
        structureType = StructureType.resource;
    }
}
