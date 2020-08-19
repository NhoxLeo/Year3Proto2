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

    public static float productionTime = 3f;
    protected float remainingTime = 3f;

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Longhaus;
        structureName = "Longhaus";
        maxHealth = 400f;
        health = maxHealth;
    }

    // Update is called once per frame
    protected override void Update()
    {
        GameManager gameMan = GameManager.GetInstance();
        base.Update();
        if (health > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                gameMan.AddBatch(new ResourceBatch(3, ResourceType.Metal));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.Wood));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.Food));
                gameMan.AddBatch(new ResourceBatch(VillagerManager.GetInstance().GetRationCost(), ResourceType.Food));
            }

        }

        if (Input.GetKeyDown(KeyCode.N) && StructureManager.GetInstance().StructureIsSelected(this))
        {
            TrainVillager();
        }
    }

    public static void TrainVillager()
    {
        ResourceBundle cost = new ResourceBundle(0, 0, 100);
        if (FindObjectOfType<GameManager>().playerResources.AttemptPurchase(cost))
        {
            VillagerManager.GetInstance().AddNewVillager();
            HUDManager.GetInstance().ShowResourceDelta(cost, true);
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta += new Vector3(7f / productionTime, 3f / productionTime, 7f / productionTime - VillagerManager.GetInstance().GetFoodConsumptionPerSec());

        return resourceDelta;
    }
}
