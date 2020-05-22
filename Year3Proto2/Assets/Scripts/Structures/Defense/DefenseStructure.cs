using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseStructure : Structure
{
    public void DefenseStart()
    {
        StructureStart();
        structureType = StructureType.defense;
    }
}
