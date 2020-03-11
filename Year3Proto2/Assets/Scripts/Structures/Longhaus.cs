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

    public float productionTime = 3f;
    protected float remainingTime = 3f;

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
                GameManager.CreateAudioEffect("lose", Camera.main.transform.position, 2f);
                //FindObjectOfType<SceneSwitcher>().SceneSwitch("TitleScreen");
            }
            longhausDead = true;
        }
        else
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                GameManager game = FindObjectOfType<GameManager>();
                game.AddBatch(new Batch(3, ResourceType.metal));
                game.AddBatch(new Batch(7, ResourceType.wood));
                game.AddBatch(new Batch(7, ResourceType.food));
            }
        }
    }
}
