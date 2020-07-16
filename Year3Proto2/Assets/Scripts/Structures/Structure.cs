﻿using UnityEngine;
public enum StructureType
{
    resource,
    environment,
    storage,
    attack,
    defense,
    longhaus
};

public enum ResourceType
{
    wood,
    metal,
    food
}
public abstract class Structure : MonoBehaviour
{
    public TileBehaviour attachedTile = null;
    public string displayName;
    public Sprite icon;
    public bool isPlaced = false;
    public float sitHeight;
    public string structureName;
    static readonly public int foodAllocationMax = 5;
    static readonly public int foodAllocationMin = 1;
    protected int ID;
    protected int foodAllocation = 3;
    protected float health;
    protected Healthbar healthBar;
    protected float maxHealth = 100.0f;
    protected StructureType structureType;
    protected float timeSinceLastHit = Mathf.Infinity;
    protected GameManager gameMan;
    protected SuperManager superMan;
    StructureManager structMan;
    protected BuildingInfo buildingInfo;
    protected HUDManager HUDMan;
    public bool fromSaveData = false;
    public bool saveDataStartFrame = false;
    private GameObject destructionEffect;

    public void SetID(int _ID)
    {
        ID = _ID;
    }

    public int GetID()
    {
        return ID;
    }

    public int GetFoodAllocation()
    {
        return foodAllocation;
    }

    public void DecreaseFoodAllocation()
    {
        if (foodAllocation > foodAllocationMin)
        {
            SetFoodAllocationGlobal(foodAllocation - 1);
        }
    }

    public virtual void SetFoodAllocation(int _newFoodAllocation)
    {
        foodAllocation = _newFoodAllocation;
    }

    public abstract void SetFoodAllocationGlobal(int _allocation);

    public void IncreaseFoodAllocation()
    {
        if (foodAllocation < foodAllocationMax)
        {
            SetFoodAllocationGlobal(foodAllocation + 1);
        }
    }

    public virtual Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = Vector3.zero;
        return resourceDelta;
    }

    public bool Damage(float amount)
    {
        timeSinceLastHit = 0.0f;
        bool setInfo = health == maxHealth;
        health -= amount;
        if (setInfo) { buildingInfo.SetInfo(); }
        if (healthBar.gameObject.activeSelf == false) { healthBar.gameObject.SetActive(true); }
        
        GameManager.CreateAudioEffect("buildingHit", transform.position, .5f);

        if (structureType == StructureType.attack)
        {
            AttackStructure attackStructure = GetComponent<AttackStructure>();
            if (attackStructure.GetEnemies().Count == 0) attackStructure.DetectEnemies();
        }

        if (health <= 0f)
        {
            GameObject destroyedVFX = Instantiate(destructionEffect);
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

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public string GetStructureName()
    {
        return structureName;
    }

    public bool CanBeRepaired()
    {
        return (health < maxHealth) && (timeSinceLastHit >= 5.0f);
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
        if (structureType != StructureType.environment)
        {
            healthBar.gameObject.SetActive(true);
        }
    }

    public virtual void OnDeselected()
    {
        if (healthBar)
        {
            if (structureType != StructureType.environment)
            {
                healthBar.gameObject.SetActive(false);
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
                enemy.SetTargetNull();
            }
        }
    }

    public bool Repair(bool _mass = false)
    {
        if (structureType == StructureType.environment)
        {
            return true;
        }

        ResourceBundle repairCost = RepairCost();
        if (gameMan.playerResources.CanAfford(repairCost) &&
            timeSinceLastHit >= 5.0f &&
            !repairCost.IsEmpty())
        {
            GameManager.IncrementRepairCount();
            if (!_mass) { HUDMan.ShowResourceDelta(repairCost, true); }
            gameMan.playerResources.DeductResource(repairCost);
            health = maxHealth;
            return true;
        }
        return false;
    }

    public ResourceBundle RepairCost()
    {
        return structMan.structureDict[structureName].originalCost * (1.0f - (health / maxHealth));
    }

    public void SetHealthbar(Healthbar _healthBar)
    {
        healthBar = _healthBar;
    }

    protected virtual void Awake()
    {
        structMan = FindObjectOfType<StructureManager>();
        gameMan = FindObjectOfType<GameManager>();
        HUDMan = FindObjectOfType<HUDManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        health = maxHealth;
        destructionEffect = Resources.Load("DestructionEffect") as GameObject;
    }

    protected virtual void Start()
    {
        superMan = SuperManager.GetInstance();
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, LayerMask.GetMask("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(this);
        }
        GameObject healthBarInst = Instantiate(structMan.healthBarPrefab, structMan.canvas.transform.Find("HUD/BuildingHealthbars"));
        SetHealthbar(healthBarInst.GetComponent<Healthbar>());
        healthBar.target = gameObject;
        healthBar.fillAmount = 1.0f;
        healthBarInst.SetActive(false);
        // health is set in awake, so this is called after and will affect all structures
        if (superMan.CurrentLevelHasModifier(SuperManager.k_iPoorTimber)) { health = maxHealth *= 0.5f; }
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
                if (GetStructureType() == StructureType.longhaus) { gameMan.longhausDead = true; GlobalData.longhausDead = true; }
                GameManager.CreateAudioEffect("buildingDestroy", transform.position);
                structMan.DecreaseStructureCost(structureName);
                attachedTile.Detach();
                Destroy(gameObject);
            }
            else
            {
                healthBar.fillAmount = health / maxHealth;
            }
        }
    }

    private void OnDestroy()
    {
        OnDestroyed();
        if (healthBar) { Destroy(healthBar.gameObject); }
        if (attachedTile) { attachedTile.Detach(); }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}

