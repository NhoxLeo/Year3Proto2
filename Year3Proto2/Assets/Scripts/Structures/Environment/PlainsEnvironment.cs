using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsEnvironment : EnvironmentStructure
{
    // Start is called before the first frame update
    void Start()
    {
        EnvironmentStart();
        environmentType = EnvironmentType.plains;
        structureName = "Plains Environment";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
