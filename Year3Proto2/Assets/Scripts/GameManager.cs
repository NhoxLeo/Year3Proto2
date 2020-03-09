using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Batch
{
    public float age;
    public int amount;
    public ResourceType type;

    public Batch(int _amount, ResourceType _type)
    {
        age = 0f;
        amount = _amount;
        type = _type;
    }

    public void AddTime(float _time)
    {
        age += _time;
    }
}

public struct PlayerData
{
    private int rWood;
    private int rMetal;
    private int rFood;

    private int rWoodMax;
    private int rMetalMax;
    private int rFoodMax;

    public PlayerData(int _startAmount, int _maxAmount)
    {
        rWood = _startAmount;
        rMetal = _startAmount;
        rFood = _startAmount;

        rWoodMax = _maxAmount;
        rMetalMax = _maxAmount;
        rFoodMax = _maxAmount;
    }

    public void AddBatch(Batch _batch)
    {
        switch (_batch.type)
        {
            case ResourceType.wood:
                rWood += _batch.amount;
                if (rWood > rWoodMax) { rWood = rWoodMax; }
                break;
            case ResourceType.metal:
                rMetal += _batch.amount;
                if (rMetal > rMetalMax) { rMetal = rMetalMax; }
                break;
            case ResourceType.food:
                rFood += _batch.amount;
                if (rFood > rFoodMax) { rFood = rFoodMax; }
                if (rFood < 0)
                {
                    //lose condition
                }
                break;
        }
    }

    public bool ResourceIsFull(ResourceType _type)
    {
        switch(_type)
        {
            case ResourceType.wood:
                return (rWood == rWoodMax);
            case ResourceType.metal:
                return (rMetal == rMetalMax);
            case ResourceType.food:
                return (rFood == rFoodMax);
        }
        return false;
    }

    public int GetResource(ResourceType _type)
    {
        int value = 0;
        switch (_type)
        {
            case ResourceType.wood:
                value = rWood;
                break;
            case ResourceType.metal:
                value = rMetal;
                break;
            case ResourceType.food:
                value = rFood;
                break;
        }
        return value;
    }

    public void SetMaximum(ResourceType _type, int _newMax)
    {
        switch (_type)
        {
            case ResourceType.wood:
                rWoodMax = _newMax;
                break;
            case ResourceType.metal:
                rMetalMax = _newMax;
                break;
            case ResourceType.food:
                rFoodMax = _newMax;
                break;
        }
    }

    public void DeductResource(ResourceType _type, int _deduction)
    {
        switch (_type)
        {
            case ResourceType.wood:
                rWood -= _deduction;
                break;
            case ResourceType.metal:
                rMetal -= _deduction;
                break;
            case ResourceType.food:
                rFood -= _deduction;
                break;
        }
    }

    public bool CanAfford(ResourceBundle _cost)
    {
        return rWood >= _cost.woodCost && rMetal >= _cost.metalCost && rFood >= _cost.foodCost;
    }

    public bool AttemptPurchase(ResourceBundle _cost)
    {
        if (CanAfford(_cost))
        {
            rWood -= _cost.woodCost;
            rMetal -= _cost.metalCost;
            rFood -= _cost.foodCost;
            return true;
        }
        else return false;
    }
}

public class GameManager : MonoBehaviour
{
    private float batchMaxAge = 3.0f;

    private List<Batch> recentBatches;

    int recentWood
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.wood)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }
    int recentMetal
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.metal)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }
    int recentFood
    {
        get
        {
            int runningTotal = 0;
            foreach (Batch batch in recentBatches)
            {
                if (batch.type == ResourceType.food)
                {
                    runningTotal += batch.amount;
                }
            }
            return runningTotal;
        }
    }

    public PlayerData playerData;

    // Start is called before the first frame update
    void Start()
    {
        playerData = new PlayerData(500, 500);
        CalculateStorageMaximum();
        recentBatches = new List<Batch>();
    }

    // Update is called once per frame
    void Update()
    {
        List<Batch> batchesToRemove = new List<Batch>();
        for (int i = 0; i < recentBatches.Count; i++)
        {
            recentBatches[i].AddTime(Time.deltaTime);
            if (recentBatches[i].age >= batchMaxAge)
            {
                batchesToRemove.Add(recentBatches[i]);
            }
        }
        foreach (Batch batch in batchesToRemove)
        {
            recentBatches.Remove(batch);
        }
    }

    public void AddBatch(Batch _newBatch)
    {
        playerData.AddBatch(_newBatch);
        recentBatches.Add(_newBatch);
    }

    public float GetWoodVelocity(int _seconds)
    {
        return recentWood * _seconds / batchMaxAge;
    }

    public float GetMetalVelocity(int _seconds)
    {
        return recentMetal * _seconds / batchMaxAge;
    }

    public float GetFoodVelocity(int _seconds)
    {
        return recentFood * _seconds / batchMaxAge;
    }

    public void CalculateStorageMaximum()
    {
        int newWoodMax = Longhaus.woodStorage;
        int newMetalMax = Longhaus.metalStorage;
        int newFoodMax = Longhaus.foodStorage;

        // gets every structure and adds to find totals.
        StorageStructure[] storageStructures = FindObjectsOfType<StorageStructure>();
        foreach (StorageStructure storageStructure in storageStructures)
        {
            switch (storageStructure.GetResourceType())
            {
                case ResourceType.wood:
                    newWoodMax += storageStructure.storage;
                    break;
                case ResourceType.metal:
                    newMetalMax += storageStructure.storage;
                    break;
                case ResourceType.food:
                    newFoodMax += storageStructure.storage;
                    break;
            }
        }

        playerData.SetMaximum(ResourceType.wood, newWoodMax);
        playerData.SetMaximum(ResourceType.metal, newMetalMax);
        playerData.SetMaximum(ResourceType.food, newFoodMax);
    }

    public void OnStructurePlace()
    {
        // Update all Lumber Mills
        LumberMill[] lumberMills = FindObjectsOfType<LumberMill>();
        foreach (LumberMill lumberMill in lumberMills)
        {
            lumberMill.CalculateTileBonus();
        }
        // Update all Mines
        Mine[] mines = FindObjectsOfType<Mine>();
        foreach (Mine mine in mines)
        {
            mine.CalculateTileBonus();
        }
        // Update all Farms
        Farm[] farms = FindObjectsOfType<Farm>();
        foreach (Farm farm in farms)
        {
            farm.CalculateTileBonus();
        }

        // Update all enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.Next();
        }
    }
}
