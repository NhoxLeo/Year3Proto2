using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentStructure : Structure
{
    public enum EnvironmentType
    {
        forest
    }

    protected EnvironmentType environmentType;
    
    public override void Check(GameObject gameobject)
    {
        
    }



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
