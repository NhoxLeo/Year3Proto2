using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerManager : MonoBehaviour
{
    private static VillagerManager instance;

    private List<Structure> allocationStructures = null;
    private int villagers = 0;
    private int availableVillagers = 0;
    private int villagersManAllocated = 0;
    private int starveTicks = 0;
    [SerializeField]
    private int villagerHungerModifier = 2;
    private Priority[] priorityOrder = new Priority[3] { Priority.Food, Priority.Wood, Priority.Metal };

    private void Awake()
    {
        instance = this;
    }

    public static VillagerManager GetInstance()
    {
        return instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        villagers = 5;
        availableVillagers = 5;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreasePriority(Priority _priority)
    {
        // if the first element is the priority to be increased...
        if (priorityOrder[0] == _priority)
        {
            // there's nothing to do.
            return;
        }
        for (int i = 1; i < 3; i++)
        {
            // if this is the priority
            if (priorityOrder[i] == _priority)
            {
                // move this priority up.
                Priority temp = priorityOrder[i - 1];
                priorityOrder[i - 1] = _priority;
                priorityOrder[i] = temp;
            }
        }
    }

    public void SetPriority(Priority priority)
    {
        if (allocationStructures == null)
        {
            allocationStructures = new List<Structure>();
        }
        else
        {
            allocationStructures.Clear();
        }
        allocationStructures.AddRange(FindObjectsOfType<ResourceStructure>());
        DeallocateAll();

        switch (priority)
        {
            case Priority.BalancedProduction:
                // first even out with food
                AAProduceMinimumFood();

                // then distribute to all resources fairly
                AADistributeResources();
                break;
            case Priority.Food:
                // first put all into food
                AAFillResourceType(ResourceType.Food);

                // then distribute to all resources fairly
                AADistributeResources();
                break;
            case Priority.Wood:
                // first even out with food
                AAProduceMinimumFood();

                // then put all into wood
                AAFillResourceType(ResourceType.Wood);

                // then distribute to all resources fairly
                AADistributeResources();
                break;
            case Priority.Metal:
                // first even out with food
                AAProduceMinimumFood();

                // then put all into wood
                AAFillResourceType(ResourceType.Metal);

                // then distribute to all resources fairly
                AADistributeResources();
                break;
            default:
                break;
        }
    }

    private void DeallocateAll()
    {
        foreach (Structure structure in FindObjectsOfType<ResourceStructure>())
        {
            structure.DeallocateAll();
        }
    }

    private void AAProduceMinimumFood()
    {
        int villagersRemaining = GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }
        // get the necessary amount of food for holding even
        float foodConsumptionPerSec = GetFoodConsumptionPerSec();
        List<Farm> farms = new List<Farm>();
        foreach (Structure structure in allocationStructures)
        {
            Farm farmComponent = structure.GetComponent<Farm>();
            if (farmComponent)
            {
                farms.Add(farmComponent);
            }
        }
        if (farms.Count > 0)
        {
            farms.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else
        {
            return;
        }
        float foodProductionPerSec = 0f;
        foreach (Farm farm in farms)
        {
            for (int i = 0; i < 3; i++)
            {
                farm.AllocateVillager();
                villagersRemaining--;
                foodProductionPerSec += farm.GetRPSPerVillager();
                if (foodProductionPerSec >= foodConsumptionPerSec || villagersRemaining == 0)
                {
                    break;
                }
            }
            if (foodProductionPerSec >= foodConsumptionPerSec || villagersRemaining == 0)
            {
                break;
            }
        }
    }

    private void AAFillResourceType(ResourceType _resource)
    {
        // fill up the relevant structures until out of villagers or out of structures
        int villagersRemaining = GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }
        List<ResourceStructure> resStructures = new List<ResourceStructure>();
        switch (_resource)
        {
            case ResourceType.Wood:
                foreach (Structure structure in allocationStructures)
                {
                    LumberMill lumberComponent = structure.GetComponent<LumberMill>();
                    if (lumberComponent)
                    {
                        resStructures.Add(lumberComponent);
                    }
                }
                break;
            case ResourceType.Metal:
                foreach (Structure structure in allocationStructures)
                {
                    Mine mineComponent = structure.GetComponent<Mine>();
                    if (mineComponent)
                    {
                        resStructures.Add(mineComponent);
                    }
                }
                break;
            case ResourceType.Food:
                foreach (Structure structure in allocationStructures)
                {
                    Farm farmComponent = structure.GetComponent<Farm>();
                    if (farmComponent)
                    {
                        resStructures.Add(farmComponent);
                    }
                }
                break;
            default:
                break;
        }
        if (resStructures.Count > 0)
        {

            resStructures.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else
        {
            return;
        }
        foreach (ResourceStructure resStructure in resStructures)
        {
            for (int i = 0; i < 3; i++)
            {
                resStructure.AllocateVillager();
                villagersRemaining--;
                if (villagersRemaining == 0)
                {
                    break;
                }
            }
            if (villagersRemaining == 0)
            {
                break;
            }
        }
    }

    //
    // Date           : 29/07/2020
    // Author         : Sam
    // Input          : no parameters
    // Description    : Part of the Automatic Allocation System. Distributes villagers to resource structures fairly until either there are no more available villagers or all available structures are full.
    //
    private void AADistributeResources()
    {
        int villagersRemaining = GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }

        List<Farm> farms = new List<Farm>();
        List<LumberMill> lumberMills = new List<LumberMill>();
        List<Mine> mines = new List<Mine>();

        foreach (Structure structure in allocationStructures)
        {
            Farm farmComponent = structure.GetComponent<Farm>();
            if (farmComponent)
            {
                farms.Add(farmComponent);
            }
        }
        foreach (Structure structure in allocationStructures)
        {
            LumberMill lumberComponent = structure.GetComponent<LumberMill>();
            if (lumberComponent)
            {
                lumberMills.Add(lumberComponent);
            }
        }
        foreach (Structure structure in allocationStructures)
        {
            Mine mineComponent = structure.GetComponent<Mine>();
            if (mineComponent)
            {
                mines.Add(mineComponent);
            }
        }

        if (farms.Count > 0)
        {
            farms.Sort(ResourceStructure.SortTileBonusDescending());
        }
        if (lumberMills.Count > 0)
        {
            lumberMills.Sort(ResourceStructure.SortTileBonusDescending());
        }
        if (mines.Count > 0)
        {
            mines.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else if (farms.Count == 0 && lumberMills.Count == 0)
        {
            return;
        }

        List<Structure> farmStructures = new List<Structure>();
        List<Structure> lumberMillStructures = new List<Structure>();
        List<Structure> mineStructures = new List<Structure>();

        farmStructures.AddRange(farms);
        lumberMillStructures.AddRange(lumberMills);
        mineStructures.AddRange(mines);

        int farmCap = 0;
        int lumberCap = 0;
        int mineCap = 0;

        foreach (Farm farm in farms)
        {
            farmCap += 3 - farm.GetAllocated();
        }
        foreach (LumberMill lumberMill in lumberMills)
        {
            lumberCap += 3 - lumberMill.GetAllocated();
        }
        foreach (Mine mine in mines)
        {
            mineCap += 3 - mine.GetAllocated();
        }

        Vector3 velocity = GameManager.GetInstance().GetResourceVelocity();

        float foodProduction = velocity.z;
        float woodProduction = velocity.x;
        float metalProduction = velocity.y;

        ResourceType lowest = AAFindLowest(foodProduction, woodProduction, metalProduction);

        while (villagersRemaining > 0 && (lumberCap > 0 || mineCap > 0 || farmCap > 0))
        {
            while (villagersRemaining > 0 && farmCap > 0 && (lowest == ResourceType.Food || (lumberCap == 0 && mineCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(farmStructures);
                foodProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    farmCap--;
                    villagersRemaining--;
                }

                lowest = AAFindLowest(foodProduction, woodProduction, metalProduction);
            }
            while (villagersRemaining > 0 && lumberCap > 0 && (lowest == ResourceType.Wood || (farmCap == 0 && mineCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(lumberMillStructures);
                woodProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    lumberCap--;
                    villagersRemaining--;
                }

                lowest = AAFindLowest(foodProduction, woodProduction, metalProduction);
            }
            while (villagersRemaining > 0 && mineCap > 0 && (lowest == ResourceType.Metal || (farmCap == 0 && lumberCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(mineStructures);
                metalProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    mineCap--;
                    villagersRemaining--;
                }

                lowest = AAFindLowest(foodProduction, woodProduction, metalProduction);
            }
        }
    }

    //
    // Date           : 29/07/2020
    // Author         : Sam
    // Input          : List<Structure> _structures, the list of structures to allocate into sequentially
    // Description    : Part of the Automatic Allocation System. Allocates a single villager to the first structure in _structures that has space for it.
    //
    private float AAAlocateIntoNext(List<Structure> _structures)
    {
        for (int i = 0; i < _structures.Count; i++)
        {
            if (_structures[i].GetAllocated() < 3)
            {
                _structures[i].AllocateVillager();
                ResourceStructure resStructure = _structures[i].GetComponent<ResourceStructure>();
                if (resStructure)
                {
                    return resStructure.GetRPSPerVillager();
                }
            }
        }
        return 0.0f;
    }

    private ResourceType AAFindLowest(float _foodProd, float _woodProd, float _metalProd)
    {
        ResourceType lowest = ResourceType.Food;
        if (_metalProd < _woodProd && _metalProd < _foodProd)
        {
            lowest = ResourceType.Metal;
        }
        else if (_woodProd < _foodProd && _woodProd < _metalProd)
        {
            lowest = ResourceType.Wood;
        }
        return lowest;
    }

    public int GetStarveTicks()
    {
        return starveTicks;
    }

    public void SetStarveTicks(int _ticks)
    {
        starveTicks = _ticks;
    }

    public void AddStarveTicks(int _ticks)
    {
        starveTicks += _ticks;
        if (starveTicks >= 100)
        {
            if (availableVillagers == 0)
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
            else
            {
                villagers--;
                availableVillagers--;
            }
        }
    }
    public int GetVillagers()
    {
        return villagers;
    }

    public void SetAvailable(int _available)
    {
        availableVillagers = _available;
    }

    public void SetVillagers(int _villagers)
    {
        villagers = _villagers;
    }

    public void AddNewVillager()
    {
        villagers++;
        availableVillagers++;
    }

    public void RemoveVillagers(int _villagers)
    {
        villagers -= _villagers;
    }

    public int GetAvailable()
    {
        return availableVillagers;
    }

    public void OnStructureDestroyed(int _allocated)
    {
        villagers -= _allocated;
    }

    public bool VillagerAvailable()
    {
        return availableVillagers > 0;
    }

    public void OnVillagerAllocated()
    {
        availableVillagers--;
    }

    public void OnVillagerDeallocated()
    {
        availableVillagers++;
    }

    public void ReturnVillagers(int _villagers)
    {
        availableVillagers += _villagers;
    }

    public float GetFoodConsumptionPerSec()
    {
        return villagers * villagerHungerModifier / Longhaus.productionTime;
    }

    public int GetRationCost()
    {
        return villagers * -villagerHungerModifier;
    }
}
