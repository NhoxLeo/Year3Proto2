using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStructure : Structure
{
    public readonly ResourceType resourceType;

    public float productionTime = 10.0f;
    private float remainingTime;

    public ResourceStructure(ResourceType resourceType) : base(StructureType.RESOURCE)
    {
        this.resourceType = resourceType;
        remainingTime = productionTime;
    }
    private void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0.0f) 
        {
            remainingTime = productionTime;
        }
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void Check(GameObject gameobject)
    {

    }
}
