using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    protected ResourceBundle attackCost;
    protected List<Transform> enemies = new List<Transform>();
    protected List<string> targetableEnemies = new List<string>();

    private Transform attackingRange;
    private Transform spottingRange = null;
    protected int level = 1;

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Defense;
        attackingRange = transform.Find("Range");
        spottingRange = transform.Find("SpottingRange");
    }

    protected override void Start()
    {
        base.Start();
        if (isPlaced)
        {
            CapsuleCollider capsule = GetComponentInChildren<TowerRange>().GetComponent<CapsuleCollider>();
            SphereCollider sphere = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
            if (capsule)
            {
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
                    if (distanceFromEnemy <= capsule.radius)
                    {
                        if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
                    }
                }
            }
            else
            {
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
                    if (distanceFromEnemy <= sphere.radius)
                    {
                        if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
                    }
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        enemies.RemoveAll(enemy => !enemy);
        
        if (level < 3)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                StructureManager structMan = StructureManager.GetInstance();
                if (structMan.StructureIsSelected(this))
                {
                    if (GameManager.GetInstance().playerResources.AttemptPurchase(structMan.QuoteUpgradeCostFor(this)))
                    {
                        LevelUp();
                    }
                }
            }
        }
        
    }

    public override void ShowRangeDisplay(bool _active)
    {
        base.ShowRangeDisplay(_active);
        spottingRange.GetChild(0).gameObject.SetActive(_active);
        attackingRange.GetChild(0).gameObject.SetActive(_active);
    }

    public List<Transform> GetEnemies()
    {
        enemies.RemoveAll(enemy => !enemy);
        return enemies;
    }

    public List<string> GetTargetableEnemies()
    {
        return targetableEnemies;
    }

    public void SetLevel(int _level)
    {
        level = _level;
        OnSetLevel();
    }

    public void LevelUp()
    {
        HUDManager.GetInstance().ShowResourceDelta(StructureManager.GetInstance().QuoteUpgradeCostFor(this), true);
        SetLevel(level + 1);
    }

    public int GetLevel()
    {
        return level;
    }

    protected virtual void OnSetLevel()
    {

    }
}
