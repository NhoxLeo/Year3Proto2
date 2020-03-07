using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Longhaus : Structure
{
    [SerializeField]
    static public int foodStorage = 500;

    [SerializeField]
    static public int woodStorage = 500;

    [SerializeField]
    static public int metalStorage = 500;

    // Start is called before the first frame update
    void Start()
    {
        StructureStart();
        structureType = StructureType.longhaus;
        structureName = "Longhaus";
    }

    // Update is called once per frame
    void Update()
    {

    }
}
