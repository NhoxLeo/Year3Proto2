using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SortTileBonusDescendingHelper<T> : IComparer
{
    int IComparer.Compare(object a, object b)
    {
        ResourceStructure structureA = (ResourceStructure)a;
        ResourceStructure structureB = (ResourceStructure)b;
        if (structureA.GetTileBonus() < structureB.GetTileBonus())
            return 1;
        if (structureA.GetTileBonus() > structureB.GetTileBonus())
            return -1;
        else
            return 0;
    }
}

public abstract class ResourceStructure : Structure
{
    public float productionTime = 3f;
    public Dictionary<TileBehaviour.TileCode, GameObject> tileHighlights;
    protected int batchSize = 1;
    protected float remainingTime = 3f;
    protected ResourceType resourceType;
    protected int tileBonus = 0;
    private GameObject tileHighlight;

    private void EnableFogMask()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).DOScale(Vector3.one * 2.0f, 1.0f).SetEase(Ease.OutQuint);
    }

    public virtual int GetProductionVolume()
    {
        return tileBonus * batchSize * allocatedVillagers;
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
            if (tileHighlights != null)
            {
                if (tileHighlights.ContainsKey((TileBehaviour.TileCode)i))
                {
                    tileHighlights[(TileBehaviour.TileCode)i].SetActive(false);
                }
            }
        }
        FindObjectOfType<HUDManager>().HideAllVillagerWidgets();
    }

    public override void OnPlace()
    {
        base.OnPlace();
        EnableFogMask();
        tileBonus = 1;
        OnDeselected();
        if (tileHighlights != null) { tileHighlights.Clear(); }
        if (attachedTile)
        {
            // For each possible tile
            for (int i = 0; i < 4; i++)
            {

                Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToAttached = attachedTile.GetAdjacentTiles();
                if (adjacentsToAttached.ContainsKey((TileBehaviour.TileCode)i))
                {
                    if (adjacentsToAttached[(TileBehaviour.TileCode)i].GetPlayable())
                    {
                        GameObject newTileHighlight = Instantiate(GetTileHighlight(), transform);
                        tileHighlights.Add((TileBehaviour.TileCode)i, newTileHighlight);
                        Vector3 highlightPos = adjacentsToAttached[(TileBehaviour.TileCode)i].transform.position;
                        highlightPos.y = 0.55f;
                        newTileHighlight.transform.position = highlightPos;
                        Structure adjStructure = adjacentsToAttached[(TileBehaviour.TileCode)i].GetAttached();
                        // If there is a structure on the tile...
                        if (adjStructure)
                        {
                            string adjStructType = "Forest Environment";
                            switch (resourceType)
                            {
                                case ResourceType.metal:
                                    adjStructType = "Hills Environment";
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
        FindObjectOfType<HUDManager>().ShowOneVillagerWidget(villagerWidget);
    }

    public int GetTileBonus()
    {
        return tileBonus;
    }

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.resource;
        tileHighlights = new Dictionary<TileBehaviour.TileCode, GameObject>();
        villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocataionWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);
    }

    private GameObject GetTileHighlight()
    {
        if (tileHighlight == null)
        {
            tileHighlight = Resources.Load("TileHighlight") as GameObject;
        }
        return tileHighlight;
    }

    protected override void Update()
    {
        base.Update();
        if (isPlaced)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                gameMan.AddBatch(new ResourceBatch(tileBonus * batchSize * allocatedVillagers, resourceType));
            }
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        switch (resourceType)
        {
            case ResourceType.food:
                resourceDelta += new Vector3(0f, 0f, tileBonus * batchSize * allocatedVillagers / productionTime);
                break;
            case ResourceType.metal:
                resourceDelta += new Vector3(0f, tileBonus * batchSize * allocatedVillagers / productionTime, 0f);
                break;
            case ResourceType.wood:
                resourceDelta += new Vector3(tileBonus * batchSize * allocatedVillagers / productionTime, 0f, 0f);
                break;
        }

        return resourceDelta;
    }

    public float GetResourcePerVillPerSec()
    {
        return batchSize * tileBonus / productionTime;
    }
}
