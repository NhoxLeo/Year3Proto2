﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public enum Priority
{
    Food,
    Wood,
    Metal
}

public class VillagerManager : MonoBehaviour
{
    private static VillagerManager instance;

    private readonly List<Structure> allocationStructures = new List<Structure>();
    private int villagers = 0;
    private int availableVillagers = 0;
    private int villagersManAllocated = 0;
    private float starveTicks = 0;
    private const float villagerHungerModifier = 2f;
    private readonly Priority[] priorityOrder = new Priority[3] { Priority.Food, Priority.Wood, Priority.Metal };

    private void Awake()
    {
        instance = this;
        villagers = 5;
        availableVillagers = 5;
    }

    public static VillagerManager GetInstance()
    {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (availableVillagers < 0)
        {
            Debug.LogError("available villagers was negative...");
            RefreshAllocStructs();
            foreach (Structure structure in allocationStructures)
            {
                structure.DeallocateAll();
            }
            RedistributeVillagers();
        }
    }

    public static string PriorityToString(Priority _priority)
    {
        if (_priority == Priority.Food)
        {
            return "Food";
        }
        if (_priority == Priority.Wood)
        {
            return "Wood";
        }
        return "Metal";
    }

    public void RefreshAllocStructs()
    {
        allocationStructures.Clear();
        allocationStructures.AddRange(StructureManager.GetInstance().GetPlayerStructures());
        allocationStructures.RemoveAll(structure => structure.GetStructureType() != StructureType.Resource);
        allocationStructures.RemoveAll(structure => structure.GetManualAllocation());
    }

    public void SetPriorities(int _food, int _wood, int _metal)
    {
        priorityOrder[_food] = Priority.Food;
        priorityOrder[_wood] = Priority.Wood;
        priorityOrder[_metal] = Priority.Metal;
        RedistributeVillagers();

        TutorialManager tutMan = TutorialManager.GetInstance();
        if (tutMan.State == TutorialManager.TutorialState.VillagerPriority)
        {
            tutMan.GoToNext();
        }
    }

    public void IncreasePriority(Priority _priority)
    {
        // if the first element is the priority to be increased...
        if (priorityOrder[0] == _priority)
        {
            //Debug.Log(PriorityToString(_priority) + " was already first priority.");
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
                //Debug.Log(PriorityToString(_priority) + " is now priority " + i.ToString() + " and " +
                    //PriorityToString(priorityOrder[i]) + " is now priority " + (i + 1).ToString());
            }
        }
        RedistributeVillagers();
    }

    public void RedistributeVillagers()
    {
        RefreshAllocStructs();
        foreach (Structure structure in allocationStructures)
        {
            structure.DeallocateAll();
        }
        int autoAllocatedVillagers = 0;
        foreach (Priority priority in priorityOrder)
        {
            switch (priority)
            {
                case Priority.Food:
                    // put all into food
                    autoAllocatedVillagers += AAFillResourceType(ResourceType.Food);
                    break;
                case Priority.Wood:
                    // first even out with food
                    autoAllocatedVillagers += AAProduceMinimumFood();

                    // then put all into wood
                    autoAllocatedVillagers += AAFillResourceType(ResourceType.Wood);
                    break;
                case Priority.Metal:
                    // first even out with food
                    autoAllocatedVillagers += AAProduceMinimumFood();

                    // then put all into wood
                    autoAllocatedVillagers += AAFillResourceType(ResourceType.Metal);
                    break;
                default:
                    break;
            }
        }
        availableVillagers = villagers - (autoAllocatedVillagers + villagersManAllocated);

    }

    private int AAProduceMinimumFood()
    {
        int villagersAllocated = 0;
        int villagersRemaining = GetAvailable();
        if (villagersRemaining == 0)
        {
            return 0;
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
        float foodProductionPerSec = Longhaus.GetFoodProductionPerSec();
        for (int i = 0; i < farms.Count; i++)
        {
            Farm farm = farms[i];
            int allocated = farm.GetAllocated();
            if (allocated > 0)
            {
                foodProductionPerSec += farm.GetRPSPerVillager() * allocated;
            }
            if (foodProductionPerSec >= foodConsumptionPerSec)
            {
                return 0;
            }
        }
        if (farms.Count > 0)
        {
            farms.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else
        {
            return 0;
        }

        foreach (Farm farm in farms)
        {
            for (int i = 0; i < 3; i++)
            {
                if (farm.AutomaticallyAllocate())
                {
                    villagersAllocated++;
                }
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
        return villagersAllocated;
    }

    private int AAFillResourceType(ResourceType _resource)
    {
        int villagersAllocated = 0;
        // fill up the relevant structures until out of villagers or out of structures
        int villagersRemaining = GetAvailable();
        if (villagersRemaining == 0)
        {
            return 0;
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
            return 0;
        }
        foreach (ResourceStructure resStructure in resStructures)
        {
            for (int i = resStructure.GetAllocated(); i < 3; i++)
            {
                if (resStructure.AutomaticallyAllocate())
                {
                    villagersAllocated++;
                }
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
        return villagersAllocated;
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

        Vector3 velocity = GameManager.GetInstance().CalculateResourceVelocity();

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
                _structures[i].AutomaticallyAllocate();
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

    public float GetStarveTicks()
    {
        return starveTicks;
    }

    public void SetStarveTicks(float _ticks)
    {
        starveTicks = _ticks;
    }

    public void AddStarveTicks(float _ticks)
    {
        starveTicks += _ticks;
        if (starveTicks >= 100f)
        {
            starveTicks = 0f;
            if (availableVillagers == 0)
            {
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
                    Structure structure = populated[UnityEngine.Random.Range(0, populated.Count)];
                    structure.ManuallyAllocate(structure.GetAllocated() - 1);
                }
            }
            RemoveVillagers(1, false);
            MessageBox.GetInstance().ShowMessage("A villager just starved!", 2.5f);
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

    public void RemoveVillagers(int _villagers, bool _wereManual)
    {
        villagers -= _villagers;
        if (_wereManual)
        {
            villagersManAllocated -= _villagers;
        }
        RedistributeVillagers();
        InfoManager.RecordVillagerDeath(_villagers);
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

    public void ReturnVillagers(int _villagers, bool _wereManual)
    {
        availableVillagers += _villagers;
        if (_wereManual)
        {
            villagersManAllocated -= _villagers;
        }
    }

    public void MarkAsAllocated(int _villagers)
    {
        availableVillagers -= _villagers;
    }

    public float GetFoodConsumptionPerSec()
    {
        return villagers * villagerHungerModifier / Longhaus.productionTime;
    }

    public float GetRationCost()
    {
        return villagers * -villagerHungerModifier;
    }

    public int GetManuallyAllocated()
    {
        return villagersManAllocated;
    }

    public void SetManuallyAllocated(int _allocated)
    {
        villagersManAllocated = _allocated;
    }

    public void MarkVillagersAsManAlloc(int _manuallyAllocated)
    {
        villagersManAllocated += _manuallyAllocated;
    }

    public int TryGetVillForManAlloc(int _numVillagers)
    {
        if (availableVillagers >= _numVillagers)
        {
            villagersManAllocated += _numVillagers;
            availableVillagers -= _numVillagers;
            return _numVillagers;
        }
        else
        {
            if (villagers - villagersManAllocated >= _numVillagers)
            {
                RefreshAllocStructs();
                foreach (Structure structure in allocationStructures)
                {
                    structure.DeallocateAll();
                }
                availableVillagers -= _numVillagers;
                // mark them as manually allocated
                villagersManAllocated += _numVillagers;
                return _numVillagers;
            }
            else
            {
                // return as many as possible
                // mark all as manually allocated
                int result = villagers - villagersManAllocated;
                if (result != 0)
                {
                    RefreshAllocStructs();
                    foreach (Structure structure in allocationStructures)
                    {
                        structure.DeallocateAll();
                    }
                    availableVillagers -= result;
                    villagersManAllocated += result;
                }
                return result;
            }
        }
    }

    public void TrainVillager()
    {
        ResourceBundle cost = GetVillagerTrainCost();
        if (FindObjectOfType<GameManager>().playerResources.AttemptPurchase(cost))
        {
            InfoManager.RecordNewAction();
            InfoManager.RecordResourcesSpent(cost);
            HUDManager.GetInstance().ShowResourceDelta(cost, true);
            AddNewVillager();
            RedistributeVillagers();

            TutorialManager tutMan = TutorialManager.GetInstance();
            if (tutMan.State == TutorialManager.TutorialState.TrainVillager)
            {
                tutMan.GoToNext();
            }
        }
    }

    public ResourceBundle GetVillagerTrainCost()
    {
        return new ResourceBundle(50 + 10 * villagers, 0, 0);
    }

    public List<Priority> GetPriorities()
    {
        return new List<Priority>()
        {
            priorityOrder[0],
            priorityOrder[1],
            priorityOrder[2]
        };
    }

    public void LoadPriorities(List<Priority> _priorities)
    {
        priorityOrder[0] = _priorities[0];
        priorityOrder[1] = _priorities[1];
        priorityOrder[2] = _priorities[2];
        //RedistributeVillagers();
        int food = 0;
        int wood = 0;
        int metal = 0;
        for (int i = 0; i < 3; i++)
        {
            switch (priorityOrder[i])
            {
                case Priority.Food:
                    food = i;
                    break;
                case Priority.Wood:
                    wood = i;
                    break;
                case Priority.Metal:
                    metal = i;
                    break;
                default:
                    break;
            }
        }
        FindObjectOfType<VillagerPriority>().LoadCardPriorites(food, wood, metal);
    }

    public string GetVillagerDebugInfo()
    {
        string heading = "Villager Stats:";
        string villagerText = "\nTotal villagers: " + villagers.ToString();
        string manuallyAllocated = "\nVillagers Manually Allocated: " + villagersManAllocated.ToString();
        string available = "\nVillagers Available: " + availableVillagers.ToString();
        string ticks = "\nStarve Ticks: " + starveTicks.ToString();
        return heading + villagerText + manuallyAllocated + available + ticks;
    }
}
