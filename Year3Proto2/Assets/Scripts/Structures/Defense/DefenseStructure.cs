using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Header("Attributes")]
    [SerializeField] protected Transform attackingRange;
    [SerializeField] protected int enemyTargetAmount = 1;

    protected Transform target;
    protected ResourceBundle attackCost;
    protected List<Transform> enemies = new List<Transform>();

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;

        DetectEnemies();
        CheckResearch();

        /*villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);*/
    }

    protected void DetectEnemies()
    {
        SphereCollider rangeCollider = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
            if (distanceFromEnemy <= rangeCollider.radius)
            {
                if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
            }
        }
    }

    public override void ShowRangeDisplay(bool _active)
    {
        base.ShowRangeDisplay(_active);
        attackingRange.GetChild(0).gameObject.SetActive(_active);
    }

    public List<Transform> GetEnemies()
    {
        return enemies;
    }

    public abstract void CheckResearch();
}
