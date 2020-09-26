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

    private const float BaseMaxHealth = 500f;

    private const int foodGen = 16;
    private const int lumberGen = 6;
    private const int metalGen = 3;


    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Longhaus;
        structureName = "Longhaus";
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
                //gameMan.AddBatch(new ResourceBatch(metalGen, ResourceType.Metal));
                //gameMan.AddBatch(new ResourceBatch(lumberGen, ResourceType.Wood));
                //gameMan.AddBatch(new ResourceBatch(foodGen, ResourceType.Food));
                //gameMan.AddBatch(new ResourceBatch(VillagerManager.GetInstance().GetRationCost(), ResourceType.Food));
            }

        }

        if (Input.GetKeyDown(KeyCode.N) && StructureManager.GetInstance().StructureIsSelected(this))
        {
            TrainVillager();
        }
    }

    public static void TrainVillager()
    {
        ResourceBundle cost = new ResourceBundle(100, 0, 0);
        if (FindObjectOfType<GameManager>().playerResources.AttemptPurchase(cost))
        {
            VillagerManager villMan = VillagerManager.GetInstance();
            villMan.AddNewVillager();
            HUDManager.GetInstance().ShowResourceDelta(cost, true);
            villMan.RedistributeVillagers();
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta += new Vector3(foodGen / productionTime - VillagerManager.GetInstance().GetFoodConsumptionPerSec(), lumberGen / productionTime, metalGen / productionTime);

        return resourceDelta;
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // poor timber multiplier
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.PoorTimber))
        {
            maxHealth *= 0.5f;
        }

        return maxHealth;
    }
}
