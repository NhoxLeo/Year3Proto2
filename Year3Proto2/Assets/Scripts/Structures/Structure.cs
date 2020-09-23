using UnityEngine;
public enum StructureType
{
    Resource,
    Environment,
    Storage,
    Defense,
    Longhaus
};

public enum ResourceType
{
    Wood,
    Metal,
    Food
}

public abstract class Structure : MonoBehaviour
{
    public TileBehaviour attachedTile = null;
    public string displayName;
    public Sprite icon;
    public bool isPlaced = false;
    public float sitHeight;
    public string structureName;
    protected int ID;
    protected float health;
    protected Healthbar healthBar;
    protected StructureType structureType;
    protected float timeSinceLastHit = Mathf.Infinity;
    protected BuildingInfo buildingInfo;
    public bool fromSaveData = false;
    public bool saveDataStartFrame = false;
    private static GameObject DestructionEffect;
    protected int allocatedVillagers = 0;
    protected static int villagerCapacity = 3;
    protected VillagerAllocation villagerWidget = null;
    private bool manualAllocation = false;

    public void HandleAllocation(int _villagers)
    {
        if (allocatedVillagers == _villagers && manualAllocation)
        {
            VillagerManager villMan = VillagerManager.GetInstance();
            villMan.ReturnFromManual(allocatedVillagers);
            allocatedVillagers = 0;
            manualAllocation = false;
            RefreshWidget();
            villMan.RedistributeVillagers();
            return;
        }

        ManuallyAllocate(_villagers);
    }

    public void ManuallyAllocate(int _villagers)
    {
        VillagerManager villMan = VillagerManager.GetInstance();
        bool previousManualAllocation = manualAllocation;
        manualAllocation = true;
        int change = _villagers - allocatedVillagers;
        if (change < 0)
        {
            // we are returning villagers to the manager's control.
            if (previousManualAllocation)
            {
                villMan.ReturnFromManual(-change);
            }
            else
            {
                villMan.ReturnVillagers(-change);
            }
            allocatedVillagers = _villagers;
            villMan.RedistributeVillagers();
        }
        else if (change > 0)
        {
            if (!previousManualAllocation)
            {
                villMan.MarkVillagersAsManAlloc(allocatedVillagers);
            }
            // try to get the number necessary from the villagerMan.
            int villagersGiven = villMan.TryGetVillForManAlloc(change);
            // if we got any
            if (villagersGiven > 0)
            {
                allocatedVillagers += villagersGiven;
                villMan.RedistributeVillagers();
            }
        }
        else if (change == 0 && !previousManualAllocation)
        {
            villMan.MarkVillagersAsManAlloc(_villagers);
        }
        OnAllocation();
        RefreshWidget();
    }

    public void RefreshWidget()
    {
        if (villagerWidget)
        {
            villagerWidget.SetManualIndicator(manualAllocation ? allocatedVillagers : -1);
            villagerWidget.SetAutoIndicator(manualAllocation ? -1 : allocatedVillagers);
        }
    }

    public virtual void AutomaticallyAllocate()
    {
        VillagerManager villMan = VillagerManager.GetInstance();
        if (villMan.VillagerAvailable() && allocatedVillagers < villagerCapacity)
        {
            manualAllocation = false;
            allocatedVillagers += 1;
            RefreshWidget();
            OnAllocation();
            villMan.OnVillagerAllocated();
        }
    }

    public bool GetManualAllocation()
    {
        return manualAllocation;
    }

    public void SetAllocationWidget(VillagerAllocation _widget)
    {
        villagerWidget = _widget;
    }

    public void SetWidgetVisibility(bool _visibility)
    {
        villagerWidget.SetVisibility(_visibility);
    }

    public int GetAllocated()
    {
        return allocatedVillagers;
    }

    public virtual void SetAllocated(int _allocated)
    {
        allocatedVillagers = _allocated;
        OnAllocation();
    }

    public virtual void DeallocateAll()
    {
        VillagerManager.GetInstance().ReturnVillagers(allocatedVillagers);
        allocatedVillagers = 0;
        OnAllocation();
        RefreshWidget();
    }

    public void SetID(int _ID)
    {
        ID = _ID;
    }

    public int GetID()
    {
        return ID;
    }

    public virtual Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = Vector3.zero;
        return resourceDelta;
    }

    public bool Damage(float amount)
    {
        timeSinceLastHit = 0.0f;
        bool setInfo = health == GetTrueMaxHealth();
        health -= amount;
        if (setInfo) { buildingInfo.SetInfo(); }
        if (healthBar.gameObject.activeSelf == false) { healthBar.gameObject.SetActive(true); }
        
        GameManager.CreateAudioEffect("buildingHit", transform.position, .5f);

        if (structureType == StructureType.Defense)
        {
            DefenseStructure defenseStructure = GetComponent<DefenseStructure>();
            //if (defenseStructure.GetEnemies().Count == 0) defenseStructure.DetectEnemies();
        }

        if (health <= 0f)
        {
            VillagerManager.GetInstance().RemoveVillagers(allocatedVillagers);
            GameObject destroyedVFX = Instantiate(DestructionEffect);
            destroyedVFX.transform.position = transform.position;
        }

        return health <= 0f;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float _health)
    {
        health = _health;
    }

    public string GetStructureName()
    {
        return structureName;
    }

    public bool CanBeRepaired()
    {
        return (health < GetTrueMaxHealth()) && (timeSinceLastHit >= 5.0f);
    }

    public StructureType GetStructureType()
    {
        return structureType;
    }

    public bool IsStructure(string _structureName)
    {
        return _structureName == structureName;
    }

    public virtual void OnAnyPlaced()
    {

    }

    public virtual void OnSelected()
    {
        if (structureType != StructureType.Environment)
        {
            healthBar.gameObject.SetActive(true);
            ShowRangeDisplay(true);
            RefreshWidget();
            if (villagerWidget)
            {
                villagerWidget.SetVisibility(true);
            }
        }
    }

    public virtual void OnDeselected()
    {
        if (structureType != StructureType.Environment)
        {
            if (healthBar)
            {
                healthBar.gameObject.SetActive(false);
            }
            ShowRangeDisplay(false);
            if (villagerWidget)
            {
                villagerWidget.SetVisibility(false);
            }
        }
    }

    public void HideHealthbar()
    {
        healthBar.gameObject.SetActive(false);
    }

    public virtual void OnPlace() 
    {
    
    }

    protected virtual void OnDestroyed()
    {
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (this == enemy.GetTarget())
            {
                enemy.SetTarget(null);
            }
        }
        PathManager.GetInstance().ClearPaths();
    }

    public bool Repair(bool _mass = false)
    {
        if (structureType == StructureType.Environment)
        {
            return true;
        }
        GameManager gameMan = GameManager.GetInstance();
        ResourceBundle repairCost = RepairCost();
        if (gameMan.playerResources.CanAfford(repairCost) && timeSinceLastHit >= 5.0f && !repairCost.IsEmpty())
        {
            GameManager.IncrementRepairCount();
            if (!_mass) { HUDManager.GetInstance().ShowResourceDelta(repairCost, true); }
            gameMan.playerResources.DeductResourceBundle(repairCost);
            health = GetTrueMaxHealth();
            return true;
        }
        return false;
    }

    public ResourceBundle RepairCost()
    {
        return StructureManager.GetInstance().structureDict[structureName].originalCost * (1.0f - (health / GetTrueMaxHealth()));
    }

    public void SetHealthbar(Healthbar _healthBar)
    {
        healthBar = _healthBar;
    }

    protected virtual void Awake()
    {
        health = GetTrueMaxHealth();

        buildingInfo = FindObjectOfType<BuildingInfo>();
        if (!DestructionEffect)
        {
            DestructionEffect = Resources.Load("DestructionEffect") as GameObject;
        }
    }

    protected virtual void Start()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, LayerMask.GetMask("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(this);
        }

        StructureManager structMan = StructureManager.GetInstance();

        GameObject healthBarInst = Instantiate(structMan.healthBarPrefab, structMan.canvas.transform.Find("HUD/BuildingHealthbars"));
        SetHealthbar(healthBarInst.GetComponent<Healthbar>());
        healthBar.target = gameObject;
        healthBar.fillAmount = 1.0f;
        healthBarInst.SetActive(false);
        // health is set in awake, so this is called after and will affect all structures
        //if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.PoorTimber)) { health = GetBaseMaxHealth() * 0.5f; }
    }

    protected virtual void Update()
    {
        if (isPlaced)
        {
            if (fromSaveData && !saveDataStartFrame)
            {
                OnPlace();
                saveDataStartFrame = true;
            }

            timeSinceLastHit += Time.deltaTime;
            if (health <= 0.0f)
            {
                if (GetStructureType() == StructureType.Longhaus) { GameManager.GetInstance().longhausDead = true; GlobalData.longhausDead = true; }
                OnDestroyed();
                attachedTile.Detach();
                GameManager.CreateAudioEffect("buildingDestroy", transform.position);
                StructureManager.GetInstance().OnStructureDestroyed(this);
                Destroy(gameObject);
            }
            else
            {
                healthBar.fillAmount = health / GetTrueMaxHealth();
            }
            RefreshWidget();
        }
    }

    private void OnDestroy()
    {
        if (healthBar) { Destroy(healthBar.gameObject); }
        if (attachedTile) { attachedTile.Detach(); }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public virtual void ShowRangeDisplay(bool _active)
    {

    }

    public virtual void OnAllocation()
    {

    }

    public abstract float GetBaseMaxHealth();

    public abstract float GetTrueMaxHealth();
}

