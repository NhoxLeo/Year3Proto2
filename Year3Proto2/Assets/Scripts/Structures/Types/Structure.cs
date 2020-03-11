using UnityEngine;
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
    public string structureName;
    public Sprite icon;
    public float sitHeight;
    public string displayName;
    public TileBehaviour attachedTile;
    public bool isPlaced;
    protected float health;
    protected float maxHealth = 100.0f;
    //private bool isBuilt;
    protected Healthbar healthBar;

    protected StructureType structureType;

    protected void StructureStart()
    {
        health = maxHealth;
        isPlaced = false;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1 << LayerMask.NameToLayer("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(this);
        }
        StructureManager structMan = FindObjectOfType<StructureManager>();
        GameObject healthBarInst = Instantiate(structMan.healthBarPrefab, structMan.canvas.transform.Find("HUD/BuildingHealthbars"));
        SetHealthbar(healthBarInst.GetComponent<Healthbar>());
        healthBar.target = gameObject;
        healthBar.fillAmount = 1.0f;
        healthBarInst.SetActive(false);
    }

    protected void StructureUpdate()
    {
        if(health <= 0.0f)
        {
            GameManager.CreateAudioEffect("buildingDestroy", transform.position);
            attachedTile.Detach();
            Destroy(gameObject);
        }
        healthBar.fillAmount = health / maxHealth;
    }

    public StructureType GetStructureType()
    {
        return structureType;
    }

    public bool IsStructure(string _structureName)
    {
        return _structureName == structureName;
    }

    public string GetStructureName()
    {
        return structureName;
    }

    public void Damage(float amount)
    {
        bool setInfo = health == maxHealth;
        health -= amount;
        if (setInfo) { FindObjectOfType<BuildingInfo>().SetInfo(); }
        if (healthBar.gameObject.activeSelf == false) { healthBar.gameObject.SetActive(true); }
        GameManager.CreateAudioEffect("buildingHit", transform.position, .5f);
    }

    public float GetHealth()
    {
        return health;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public ResourceBundle RepairCost()
    {
        return FindObjectOfType<StructureManager>().structureDict[structureName].resourceCost * (1.0f - (health / maxHealth));
    }

    public void Repair()
    {
        health = maxHealth;
    }

    private void OnDestroy()
    {
        if (healthBar) { Destroy(healthBar.gameObject); }
        if (attachedTile)
        {
            attachedTile.Detach();
        }
    }

    public virtual void OnPlace()
    {

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
        if (structureType != StructureType.environment)
        {
            healthBar.gameObject.SetActive(false);
        }
    }
    
    public void SetHealthbar(Healthbar _healthBar)
    {
        healthBar = _healthBar;
    }
}

