using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentStructure : Structure
{

    private readonly ResourceType resourceType;

    public EnvironmentStructure(ResourceType resourceType) : base(StructureType.ENVIRONMENT)
    {
        this.resourceType = resourceType;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }
}
