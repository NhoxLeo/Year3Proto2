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

    private static int villagers = 0;
    private static int availableVillagers = 0;
    private static int starveTicks = 0;
    [SerializeField]
    private int villagerHungerModifier = 3;

    public static void AddStarveTicks(int _ticks)
    {
        starveTicks += _ticks;
        if (starveTicks >= 100)
        {
            starveTicks = 0;
            List<Structure> populated = new List<Structure>();
            foreach (Structure structure in FindObjectsOfType<Structure>())
            {
                if (structure.GetAllocated() > 0)
                {
                    populated.Add(structure);
                }
            }
            if (populated.Count > 0)
            {
                populated[Random.Range(0, populated.Count)].DeallocateVillager();
            }
            villagers--;
            availableVillagers--;
        }
    }

    public static int GetVillagers()
    {
        return villagers;
    }

    public static void SetVillagers(int _villagers)
    {
        villagers = _villagers;
    }

    public static void AddNewVillager()
    {
        villagers++;
    }

    public static void RemoveVillagers(int _villagers)
    {
        villagers -= _villagers;
    }

    public static int GetAvailable()
    {
        return availableVillagers;
    }

    public static void OnStructureDestroyed(int _allocated)
    {
        villagers -= _allocated;
    }

    public static bool VillagerAvailable()
    {
        return availableVillagers > 0;
    }

    public static void OnVillagerAllocated()
    {
        availableVillagers--;
    }

    public static void OnVillagerDeallocated()
    {
        availableVillagers++;
    }

    public static void ReturnVillagers(int _villagers)
    {
        availableVillagers += _villagers;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        villagers = 5;
        availableVillagers = 5;
    }

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.longhaus;
        structureName = "Longhaus";
        maxHealth = 400f;
        health = maxHealth;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (health > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                gameMan.AddBatch(new ResourceBatch(3, ResourceType.metal));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.wood));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.food));
                gameMan.AddBatch(new ResourceBatch(villagers * -villagerHungerModifier, ResourceType.food));
            }

        }

        if (Input.GetKeyDown(KeyCode.N) && structMan.IsThisStructureSelected(this))
        {
            TrainVillager();
        }
    }

    public static void TrainVillager()
    {
        ResourceBundle cost = new ResourceBundle(0, 0, 100);
        if (FindObjectOfType<GameManager>().playerResources.AttemptPurchase(cost))
        {
            villagers++;
            availableVillagers++;
            FindObjectOfType<HUDManager>().ShowResourceDelta(cost, true);
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta += new Vector3(7f / productionTime, 3f / productionTime, 7f / productionTime - (villagers * villagerHungerModifier / productionTime));

        return resourceDelta;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }
}
