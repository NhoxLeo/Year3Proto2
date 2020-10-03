using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentStructure : Structure
{

    protected ResourceType resourceType;
    protected ResourceStructure gatherer = null;
    protected bool exploited = false;
    protected int exploiterID = -1;

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Environment;
    }

    public ResourceStructure GetGatherer()
    {
        return gatherer;
    }

    public void SetGatherer(ResourceStructure _gatherer)
    {
        gatherer = _gatherer;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public void SetExploited(bool _exploited)
    {
        exploited = _exploited;
    }

    public bool GetExploited()
    {
        return exploited;
    }

    public void SetExploiterID(int _ID)
    {
        exploiterID = _ID;
    }

    public int GetExploiterID()
    {
        return exploiterID;
    }

    public abstract void SetOpacity(float _opacity);
}
