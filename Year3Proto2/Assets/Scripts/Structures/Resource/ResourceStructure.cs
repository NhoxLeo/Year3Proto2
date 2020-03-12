using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceStructure : Structure
{
    public float productionTime = 3f;
    public Dictionary<TileBehaviour.TileCode, GameObject> tileHighlights;
    protected int batchSize = 1;
    protected float remainingTime = 3f;
    protected ResourceType resourceType;
    protected int tileBonus = 0;
    private GameObject tileHighlight;
    public virtual int GetProductionVolume()
    {
        return tileBonus * batchSize * foodAllocation;
    }

    public ResourceType GetResourceType()
    {
        return resourceType;
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        for (int i = 0; i < 4; i++)
        {
            if (tileHighlights.ContainsKey((TileBehaviour.TileCode)i))
            {
                tileHighlights[(TileBehaviour.TileCode)i].SetActive(false);
            }
        }
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
        base.OnSelected();
        for (int i = 0; i < 4; i++)
        {
            if (tileHighlights.ContainsKey((TileBehaviour.TileCode)i))
            {
                tileHighlights[(TileBehaviour.TileCode)i].SetActive(true);
            }
        }
    }

    protected void ResourceStart()
    {
        StructureStart();
        structureType = StructureType.resource;
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
                if (structureName != "Farm")
                {
                    if (game.playerData.CanAfford(new ResourceBundle(foodAllocation, 0, 0)))
                    {
                        game.AddBatch(new Batch(tileBonus * batchSize * foodAllocation, resourceType));
                        game.AddBatch(new Batch(-foodAllocation, ResourceType.food));
                    }
                }
                else
                {
                    game.AddBatch(new Batch(tileBonus * batchSize * foodAllocation, resourceType));
                }
            }
        }
    }
}
