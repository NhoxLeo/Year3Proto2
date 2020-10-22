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
    protected const float BaseMaxHealth = 200f;

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
    protected int batchSize = 1;
    protected float remainingTime = 2f;
    protected ResourceType resourceType;
    protected int tileBonus = 0;
    protected static GameObject TileHighlight = null;
    protected static GameObject Fencing = null;
    private GameObject[] villagers = new GameObject[3];
    private bool[] workingVillagersActive = new bool[4];
    private List<int> validWorkingVillagers = new List<int>();

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
    }

    public override void OnPlace()
    {
        base.OnPlace();
        tileBonus = 1;
        OnDeselected();
        if (tileHighlights != null)
        {
            tileHighlights.Clear();
        }
        if (validWorkingVillagers != null)
        {
            validWorkingVillagers.Clear();
        }

        string adjStructType = StructureNames.LumberEnvironment;
        switch (resourceType)
        {
            case ResourceType.Metal:
                adjStructType = StructureNames.MetalEnvironment;
                break;
            case ResourceType.Food:
                adjStructType = StructureNames.FoodEnvironment;
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
                        
                        Structure adjStructure = adjacentsToAttached[(TileBehaviour.TileCode)i].GetAttached();
                        bool tileCounted = false;
                        float height = 2f;
                        // If there is a structure on the tile...
                        if (adjStructure)
                        {
                            if (adjStructure.IsStructure(adjStructType))
                            {
                                EnvironmentStructure envStructure = adjStructure.GetComponent<EnvironmentStructure>();
                                height = envStructure.GetBonusHighlightSitHeight();
                                if (!envStructure.GetExploited())
                                {
                                    envStructure.SetExploited(true);
                                    envStructure.SetExploiterID(ID);
                                }
                                // the structure will now definitely be exploited
                                if (envStructure.GetExploiterID() == ID)
                                {
                                    tileCounted = true;
                                }
                            }
                        }
                        if (tileCounted)
                        {
                            GameObject workingVillager = transform.Find("Working Villager " + i.ToString()).gameObject;
                            if (!workingVillager.activeSelf)
                            {
                                workingVillager.SetActive(true);
                            }
                            validWorkingVillagers.Add(i);
                            workingVillagersActive[i] = true;

                            GameObject newTileHighlight = Instantiate(StructureManager.GetBonusHighlight(), transform);
                            tileHighlights.Add((TileBehaviour.TileCode)i, newTileHighlight);
                            Vector3 highlightPos = adjacentsToAttached[(TileBehaviour.TileCode)i].transform.position;
                            highlightPos.y = StructureManager.BonusHighlightSitHeight;
                            newTileHighlight.transform.position = highlightPos;
                            SuperManager.SetBonusHighlightHeight(newTileHighlight.transform, resourceType == ResourceType.Wood ? height * 0.5f : height);

                            tileBonus++;
                            AdjacentOnPlaceEvent((TileBehaviour.TileCode)i, true);
                            newTileHighlight.SetActive(false);
                        }
                        else
                        {
                            AdjacentOnPlaceEvent((TileBehaviour.TileCode)i, false);
                        }
                    }
                    else
                    {
                        AdjacentOnPlaceEvent((TileBehaviour.TileCode)i, false);
                    }
                }
                else
                {
                    AdjacentOnPlaceEvent((TileBehaviour.TileCode)i, false);
                }
            }
        }
        RefreshVillagers();
    }

    protected virtual void AdjacentOnPlaceEvent(TileBehaviour.TileCode _side, bool _exploit)
    {

    }

    protected virtual Vector3 GetWorkLocation()
    {
        return Vector3.zero;
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

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        if (!attachedTile)
        {
            return;
        }

        // For each possible tile
        for (int i = 0; i < 4; i++)
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

    public int GetTileBonus()
    {
        return tileBonus;
    }

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Resource;
        tileHighlights = new Dictionary<TileBehaviour.TileCode, GameObject>();
        villagers[0] = transform.Find("Villager 1").gameObject;
        villagers[1] = transform.Find("Villager 2").gameObject;
        villagers[2] = transform.Find("Villager 3").gameObject;
        transform.Find("Working Villager 0").GetComponent<VillagerAnimation>().SetID(0);
        transform.Find("Working Villager 1").GetComponent<VillagerAnimation>().SetID(1);
        transform.Find("Working Villager 2").GetComponent<VillagerAnimation>().SetID(2);
        transform.Find("Working Villager 3").GetComponent<VillagerAnimation>().SetID(3);
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
                //GameManager.GetInstance().AddBatch(new ResourceBatch(tileBonus * batchSize * allocatedVillagers, resourceType));
            }
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        switch (resourceType)
        {
            case ResourceType.Food:
                resourceDelta += new Vector3(tileBonus * batchSize * allocatedVillagers / productionTime, 0f, 0f);
                break;
            case ResourceType.Wood:
                resourceDelta += new Vector3(0f, tileBonus * batchSize * allocatedVillagers / productionTime, 0f);
                break;
            case ResourceType.Metal:
                resourceDelta += new Vector3(0f, 0f, tileBonus * batchSize * allocatedVillagers / productionTime);
                break;
        }

        return resourceDelta;
    }

    public float GetRPSPerVillager()
    {
        return batchSize * tileBonus / productionTime;
    }

    public override void OnAllocation()
    {
        base.OnAllocation();
        RefreshVillagers();
    }

    public void RefreshVillagers()
    {
        int allocated = 0;
        workingVillagersActive[0] = false;
        workingVillagersActive[1] = false;
        workingVillagersActive[2] = false;
        workingVillagersActive[3] = false;
        for (int i = 0; i < validWorkingVillagers.Count; i++)
        {
            if (allocatedVillagers > i)
            {
                GameObject workingVillager = transform.Find("Working Villager " + validWorkingVillagers[i].ToString()).gameObject;
                if (!workingVillager.activeSelf)
                {
                    workingVillager.SetActive(true);
                }
                workingVillagersActive[validWorkingVillagers[i]] = true;
                allocated++;
            }
        }
        int remainingVillagers = allocatedVillagers - allocated;
        for (int i = 0; i < villagers.Length; i++)
        {
            villagers[i].SetActive(remainingVillagers > i);
        }
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    public bool GetWorkingVillagerActive(int _id)
    {
        if (workingVillagersActive.Length > _id)
        {
            return workingVillagersActive[_id];
        }
        return false;
    }
}
