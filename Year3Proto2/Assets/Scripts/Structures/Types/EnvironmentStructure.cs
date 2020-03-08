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

    protected void EnvironmentStart()
    {
        StructureStart();
        structureType = StructureType.environment;
    }

    public EnvironmentType GetEnvironmentType()
    {
        return environmentType;
    }
}
