using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SortTileBonusDescendingHelper : IComparer
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
    public class SortTileBonusDescendingHelper : IComparer<ResourceStructure>
    {
        public int Compare(ResourceStructure _structureA, ResourceStructure _structureB)
        {
            if (_structureA.GetTileBonus() < _structureB.GetTileBonus())
            {
                return 1;
            }
            if (_structureA.GetTileBonus() > _structureB.GetTileBonus())
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public static IComparer<ResourceStructure> SortTileBonusDescending()
    {
        return new SortTileBonusDescendingHelper();
    }

    public float productionTime = 2f;
    protected Dictionary<TileBehaviour.TileCode, GameObject> tileHighlights;
    protected Dictionary<TileBehaviour.TileCode, GameObject> fences;
    protected Dictionary<TileBehaviour.TileCode, GameObject> closedFences;
    protected int batchSize = 1;
    protected float remainingTime = 2f;
    protected ResourceType resourceType;
    protected int tileBonus = 0;
    protected static GameObject TileHighlight = null;
    protected static GameObject Fencing = null;
    protected VillagerAllocation villagerWidget;

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
        //EnableFogMask();
        tileBonus = 1;
        OnDeselected();
        if (tileHighlights != null)
        {
            tileHighlights.Clear();
        }

        string adjStructType = "Forest Environment";
        switch (resourceType)
        {
            case ResourceType.Metal:
                adjStructType = "Hills Environment";
                break;
            case ResourceType.Food:
                adjStructType = "Plains Environment";
                break;
            default:
                break;
        }

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
                            if (adjStructure.IsStructure(adjStructType))
                            {
                                EnvironmentStructure envStructure = adjStructure.GetComponent<EnvironmentStructure>();
                                if (!envStructure.GetExploited())
                                {
                                    envStructure.SetExploited(true);
                                    envStructure.SetExploiterID(ID);
                                    newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.green);
                                    tileBonus++;
                                    fences[(TileBehaviour.TileCode)i].SetActive(true);
                                    closedFences[(TileBehaviour.TileCode)i].SetActive(false);
                                }
                                else
                                {
                                    if (envStructure.GetExploiterID() == ID)
                                    {
                                        newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.green);
                                        tileBonus++;
                                        fences[(TileBehaviour.TileCode)i].SetActive(true);
                                        closedFences[(TileBehaviour.TileCode)i].SetActive(false);
                                    }
                                    else
                                    {
                                        newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.red);
                                        fences[(TileBehaviour.TileCode)i].SetActive(false);
                                        closedFences[(TileBehaviour.TileCode)i].SetActive(true);
                                    }
                                }
                            }
                            else
                            {
                                newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.red);
                                fences[(TileBehaviour.TileCode)i].SetActive(false);
                                closedFences[(TileBehaviour.TileCode)i].SetActive(true);
                            }
                        }
                        else
                        {
                            newTileHighlight.GetComponent<MeshRenderer>().material.SetColor("_UnlitColor", Color.red);
                            fences[(TileBehaviour.TileCode)i].SetActive(false);
                            closedFences[(TileBehaviour.TileCode)i].SetActive(true);
                        }
                        newTileHighlight.SetActive(false);
                    }
                }
                else
                {
                    fences[(TileBehaviour.TileCode)i].SetActive(false);
                    closedFences[(TileBehaviour.TileCode)i].SetActive(true);
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

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        // For each possible tile
        for (int i = 0; i < 4; i++)
        {
            if (attachedTile)
            {
                Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToAttached = attachedTile.GetAdjacentTiles();
                if (adjacentsToAttached.ContainsKey((TileBehaviour.TileCode)i))
                {
                    if (adjacentsToAttached[(TileBehaviour.TileCode)i].GetPlayable())
                    {
                        Structure adjStructure = adjacentsToAttached[(TileBehaviour.TileCode)i].GetAttached();
                        // If there is a structure on the tile...
                        if (adjStructure)
                        {
                            EnvironmentStructure envStructure = adjStructure.GetComponent<EnvironmentStructure>();
                            if (envStructure)
                            {
                                if (envStructure.GetExploited())
                                {
                                    if (envStructure.GetExploiterID() == ID)
                                    {
                                        envStructure.SetExploited(false);
                                        envStructure.SetExploiterID(-1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public int GetTileBonus()
    {
        return tileBonus;
    }

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Resource;
        tileHighlights = new Dictionary<TileBehaviour.TileCode, GameObject>();
        fences = new Dictionary<TileBehaviour.TileCode, GameObject>
        {
            { TileBehaviour.TileCode.north, transform.GetChild(0).gameObject },
            { TileBehaviour.TileCode.east, transform.GetChild(1).gameObject },
            { TileBehaviour.TileCode.south, transform.GetChild(2).gameObject },
            { TileBehaviour.TileCode.west, transform.GetChild(3).gameObject }
        };
        closedFences = new Dictionary<TileBehaviour.TileCode, GameObject>
        {
            { TileBehaviour.TileCode.north, transform.GetChild(4).gameObject },
            { TileBehaviour.TileCode.east, transform.GetChild(5).gameObject },
            { TileBehaviour.TileCode.south, transform.GetChild(6).gameObject },
            { TileBehaviour.TileCode.west, transform.GetChild(7).gameObject }
        };
        villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);
    }

    private GameObject GetTileHighlight()
    {
        if (!TileHighlight)
        {
            TileHighlight = Resources.Load("TileHighlight") as GameObject;
        }
        return TileHighlight;
    }

    private GameObject GetFencing()
    {
        if (!Fencing)
        {
            Fencing = Resources.Load("Fencing") as GameObject;
        }
        return Fencing;
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
            case ResourceType.Food:
                resourceDelta += new Vector3(0f, 0f, tileBonus * batchSize * allocatedVillagers / productionTime);
                break;
            case ResourceType.Metal:
                resourceDelta += new Vector3(0f, tileBonus * batchSize * allocatedVillagers / productionTime, 0f);
                break;
            case ResourceType.Wood:
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
