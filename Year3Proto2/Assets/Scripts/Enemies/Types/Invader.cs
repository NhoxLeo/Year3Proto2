using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invader : Enemy
{
    private void Start()
    {
        //The structures that it will attack...
        structureTypes = new List<StructureType>(new[] { 
            StructureType.attack,
            StructureType.resource
        }); 
    }

    public override void Action(Structure structure)
    {

    }
}
