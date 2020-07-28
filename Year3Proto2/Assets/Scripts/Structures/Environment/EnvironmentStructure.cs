using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentStructure : Structure
{
    public enum EnvironmentType
    {
        forest,
        hill,
        plains
    }

    protected EnvironmentType environmentType;
    protected ResourceStructure gatherer = null;

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.environment;
    }

    public ResourceStructure GetGatherer()
    {
        return gatherer;
    }

    public void SetGatherer(ResourceStructure _gatherer)
    {
        gatherer = _gatherer;
    }

    public EnvironmentType GetEnvironmentType()
    {
        return environmentType;
    }
}
