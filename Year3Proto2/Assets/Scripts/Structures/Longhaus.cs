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

    private bool longhausDead;

    // Start is called before the first frame update
    void Start()
    {
        StructureStart();
        structureType = StructureType.longhaus;
        structureName = "Longhaus";
        longhausDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
        if (health <= 0)
        {
            if (!longhausDead)
            {
                FindObjectOfType<MessageBox>().ShowMessage("You Lost!", 3f);
                //FindObjectOfType<SceneSwitcher>().SceneSwitch("TitleScreen");
            }
            longhausDead = true;
        }
    }
}
