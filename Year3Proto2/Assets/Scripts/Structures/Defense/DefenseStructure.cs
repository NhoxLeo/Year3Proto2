using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Header("Attributes")]
    [SerializeField] protected Transform attackingRange;

    protected ResourceBundle attackCost;
    protected Transform enemy;
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

    public abstract bool Launch();

    protected Transform GetClosestEnemy()
    {
        float closestDistanceSqr = Mathf.Infinity;

        Transform nearestSpottedEnemy = null;

        foreach (Transform enemy in enemies)
        {
            if (enemy.GetComponent<Enemy>().IsBeingObserved())
            {
                Vector3 directionToTarget = enemy.transform.position - transform.position;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    nearestSpottedEnemy = enemy;
                }
            }
        }
        return nearestSpottedEnemy;
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

    public override void OnSelected()
    {
        base.OnSelected();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
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
