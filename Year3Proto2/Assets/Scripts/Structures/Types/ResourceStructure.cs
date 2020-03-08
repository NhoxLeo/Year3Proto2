using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceStructure : Structure
{
    protected ResourceType resourceType;
    protected int foodAllocation;
    protected int foodAllocationMin = 1;
    static protected int foodAllocationMax = 5;
    protected int tileBonus = 0;
    public float productionTime = 3f;
    protected float remainingTime = 3f;
    protected int batchSize = 2;


    protected void ResourceStart()
    {
        StructureStart();
        structureType = StructureType.resource;
        foodAllocation = 3;
    }

    protected void ResourceUpdate()
    {
        StructureUpdate();
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = productionTime;
            GameManager game = FindObjectOfType<GameManager>();
            game.AddBatch(new Batch(tileBonus * batchSize * foodAllocation, resourceType));
            game.AddBatch(new Batch(-foodAllocation, ResourceType.food));
        }
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public void IncreaseFoodAllocation()
    {
        string debug = gameObject.ToString() + " foodAlloc was " + foodAllocation.ToString() + " and is now ";
        foodAllocation++;
        if (foodAllocation > foodAllocationMax) { foodAllocation = foodAllocationMax; }
        Debug.Log(debug + foodAllocation);
    }

    public void DecreaseFoodAllocation()
    {
        string debug = gameObject.ToString() + " foodAlloc was " + foodAllocation.ToString() + " and is now ";
        foodAllocation--;
        if (foodAllocation < foodAllocationMin) { foodAllocation = foodAllocationMin; }
        Debug.Log(debug + foodAllocation);
    }
    public void SetFoodAllocationMax()
    {
        foodAllocation = foodAllocationMax;
    }

    public void SetFoodAllocationMin()
    {
        foodAllocation = foodAllocationMin;
    }

    public int GetFoodAllocation()
    {
        return foodAllocation;
    }

    public virtual int GetProductionVolume()
    {
        return tileBonus * batchSize * foodAllocation;
    }

    public static int GetFoodAllocationMax()
    {
        return foodAllocationMax;
    }
}
