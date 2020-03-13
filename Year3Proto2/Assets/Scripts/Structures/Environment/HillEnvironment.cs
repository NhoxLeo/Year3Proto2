using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillEnvironment : EnvironmentStructure
{
    // Start is called before the first frame update
    void Start()
    {
        EnvironmentStart();
        environmentType = EnvironmentType.hill;
        structureName = "Hill Environment";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
