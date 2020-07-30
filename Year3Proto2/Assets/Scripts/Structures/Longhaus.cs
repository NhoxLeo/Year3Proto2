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

    private static int villagers = 0;
    private static int availableVillagers = 0;
    private static int starveTicks = 0;
    [SerializeField]
    private static int villagerHungerModifier = 2;

    public static int GetStarveTicks()
    {
        return starveTicks;
    }

    public static void SetStarveTicks(int _ticks)
    {
        starveTicks = _ticks;
    }

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

    public static void SetAvailable(int _available)
    {
        availableVillagers = _available;
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
                gameMan.AddBatch(new ResourceBatch(3, ResourceType.Metal));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.Wood));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.Food));
                gameMan.AddBatch(new ResourceBatch(villagers * -villagerHungerModifier, ResourceType.Food));
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

        // wood metal food
        resourceDelta += new Vector3(7f / productionTime, 3f / productionTime, 7f / productionTime - (villagers * villagerHungerModifier / productionTime));

        return resourceDelta;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }

    public static float GetFoodConsumptionPerSec()
    {
        return (villagers * villagerHungerModifier) / productionTime;
    }
}
