﻿using UnityEngine;
public enum StructureType
{
    resource,
    environment,
    attack,
    storage,
    defense,
    longhaus
};

public abstract class Structure : MonoBehaviour
{
    public TileBehaviour attachedTile = null;
    public string displayName;
    public Sprite icon;
    public bool isPlaced;
    public float sitHeight;
    public string structureName;
    static protected int foodAllocationMax = 5;
    static protected int foodAllocationMin = 1;
    protected int foodAllocation = 3;
    protected float health;
    protected Healthbar healthBar;
    protected float maxHealth = 100.0f;
    protected StructureType structureType;
    protected float timeSinceLastHit = Mathf.Infinity;
    protected GameManager gameMan;
    StructureManager structMan;
    protected BuildingInfo buildingInfo;
    protected HUDManager HUDMan;

    public static int GetFoodAllocationMax()
    {
        return foodAllocationMax;
    }

    public void Damage(float amount)
    {
        timeSinceLastHit = 0.0f;
        bool setInfo = health == maxHealth;
        health -= amount;
        if (setInfo) { buildingInfo.SetInfo(); }
        if (healthBar.gameObject.activeSelf == false) { healthBar.gameObject.SetActive(true); }
        GameManager.CreateAudioEffect("buildingHit", transform.position, .5f);
    }

    public virtual void DecreaseFoodAllocation()
    {
        string debug = gameObject.ToString() + " foodAlloc was " + foodAllocation.ToString() + " and is now ";
        foodAllocation--;
        if (foodAllocation < foodAllocationMin) { foodAllocation = foodAllocationMin; }
        //Debug.Log(debug + foodAllocation);
    }

    public int GetFoodAllocation()
    {
        return foodAllocation;
    }

    public float GetHealth()
    {
        return health;
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

    public virtual void IncreaseFoodAllocation()
    {
        string debug = gameObject.ToString() + " foodAlloc was " + foodAllocation.ToString() + " and is now ";
        foodAllocation++;
        if (foodAllocation > foodAllocationMax) { foodAllocation = foodAllocationMax; }
        //Debug.Log(debug + foodAllocation);
    }
    public bool IsStructure(string _structureName)
    {
        return _structureName == structureName;
    }

    public virtual void OnAnyPlaced()
    {

    }

    public virtual void OnDeselected()
    {
        if (structureType != StructureType.environment)
        {
            healthBar.gameObject.SetActive(false);
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

    }

    public virtual void OnSelected()
    {
        if (structureType != StructureType.environment)
        {
            healthBar.gameObject.SetActive(true);
        }
    }

    public bool Repair(bool _mass = false)
    {
        if (structureType == StructureType.environment)
        {
            return true;
        }

        ResourceBundle repairCost = RepairCost();
        if (gameMan.playerData.CanAfford(repairCost) &&
            timeSinceLastHit >= 5.0f &&
            !repairCost.IsEmpty())
        {
            GameManager.IncrementRepairCount();
            if (!_mass) { HUDMan.ShowResourceDelta(repairCost, true); }
            gameMan.playerData.DeductResource(repairCost);
            health = maxHealth;
            return true;
        }
        return false;
    }

    public ResourceBundle RepairCost()
    {
        return structMan.structureDict[structureName].originalCost * (1.0f - (health / maxHealth));
    }

    public void SetFoodAllocationMax()
    {
        foodAllocation = foodAllocationMax;
    }

    public void SetFoodAllocationMin()
    {
        foodAllocation = foodAllocationMin;
    }
    public void SetHealthbar(Healthbar _healthBar)
    {
        healthBar = _healthBar;
    }

    protected void StructureStart()
    {
        health = maxHealth;
        isPlaced = false;
        structMan = FindObjectOfType<StructureManager>();
        gameMan = FindObjectOfType<GameManager>();
        HUDMan = FindObjectOfType<HUDManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1 << LayerMask.NameToLayer("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(this);
        }
        GameObject healthBarInst = Instantiate(structMan.healthBarPrefab, structMan.canvas.transform.Find("HUD/BuildingHealthbars"));
        SetHealthbar(healthBarInst.GetComponent<Healthbar>());
        healthBar.target = gameObject;
        healthBar.fillAmount = 1.0f;
        healthBarInst.SetActive(false);
    }

    protected void StructureUpdate()
    {
        timeSinceLastHit += Time.deltaTime;
        if (health <= 0.0f)
        {
            GameManager.CreateAudioEffect("buildingDestroy", transform.position);
            structMan.DecreaseStructureCost(structureName);
            attachedTile.Detach();
            Destroy(gameObject);
        }
        healthBar.fillAmount = health / maxHealth;
    }
    private void OnDestroy()
    {
        OnDestroyed();
        if (healthBar) { Destroy(healthBar.gameObject); }
        if (attachedTile)
        {
            attachedTile.Detach();
        }
    }
}

