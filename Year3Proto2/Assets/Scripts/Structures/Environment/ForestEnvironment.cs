using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestEnvironment : EnvironmentStructure
{

    // Start is called before the first frame update
    void Start()
    {
        EnvironmentStart();
        environmentType = EnvironmentType.forest;
        structureName = "Forest Environment";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
