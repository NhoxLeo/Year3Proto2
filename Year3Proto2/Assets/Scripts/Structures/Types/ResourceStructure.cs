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
    private GameObject tileHighlight;
    public Dictionary<TileBehaviour.TileCode, GameObject> tileHighlights;

    protected void ResourceStart()
    {
        StructureStart();
        structureType = StructureType.resource;
        foodAllocation = 3;
        tileHighlight = Resources.Load("TileHighlight") as GameObject;
        tileHighlights = new Dictionary<TileBehaviour.TileCode, GameObject>();
    }

    protected void ResourceUpdate()
    {
        if (isPlaced)
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
        //Debug.Log(debug + foodAllocation);
    }

    public void DecreaseFoodAllocation()
    {
        string debug = gameObject.ToString() + " foodAlloc was " + foodAllocation.ToString() + " and is now ";
        foodAllocation--;
        if (foodAllocation < foodAllocationMin) { foodAllocation = foodAllocationMin; }
        //Debug.Log(debug + foodAllocation);
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

    public override void OnPlace()
    {
        tileBonus = 1;
        OnDeselected();
        tileHighlights.Clear(); 
        if (attachedTile)
        {
            // For each possible tile
            for (int i = 0; i < 4; i++)
            {
                if (attachedTile.adjacentTiles.ContainsKey((TileBehaviour.TileCode)i))
                {
                    if (attachedTile.adjacentTiles[(TileBehaviour.TileCode)i].GetPlayable())
                    {
                        GameObject newTileHighlight = Instantiate(tileHighlight, transform);
                        tileHighlights.Add((TileBehaviour.TileCode)i, newTileHighlight);
                        Vector3 highlightPos = attachedTile.adjacentTiles[(TileBehaviour.TileCode)i].transform.position;
                        highlightPos.y = 0.55f;
                        newTileHighlight.transform.position = highlightPos;
                        Structure adjStructure = attachedTile.adjacentTiles[(TileBehaviour.TileCode)i].GetAttached();
                        // If there is a structure on the tile...
                        if (adjStructure)
                        {
                            string adjStructType = "Forest Environment";
                            switch (resourceType)
                            {
                                case ResourceType.wood:
                                    adjStructType = "Forest Environment";
                                    break;
                                case ResourceType.metal:
                                    adjStructType = "Hill Environment";
                                    break;
                                case ResourceType.food:
                                    adjStructType = "Plains Environment";
                                    break;
                                default:
                                    break;
                            }

                            if (adjStructure.IsStructure(adjStructType))
                            {
                                newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.green);
                                tileBonus++;
                            }
                            else
                            {
                                newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.red);
                            }
                        }
                        else
                        {
                            newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.red);
                        }
                        newTileHighlight.SetActive(false);
                    }
                }
            }
        }
    }


    public override void OnSelected()
    {
        for (int i = 0; i < 4; i++)
        {
            if (tileHighlights.ContainsKey((TileBehaviour.TileCode)i))
            {
                tileHighlights[(TileBehaviour.TileCode)i].SetActive(true);
            }
        }
    }

    public override void OnDeselected()
    {
        for (int i = 0; i < 4; i++)
        {
            if (tileHighlights.ContainsKey((TileBehaviour.TileCode)i))
            { 
                tileHighlights[(TileBehaviour.TileCode)i].SetActive(false);
            }
        }
    }
}
