﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseStructure : Structure
{
    protected override void Start()
    {
        base.Start();
        structureType = StructureType.defense;
    }
}
