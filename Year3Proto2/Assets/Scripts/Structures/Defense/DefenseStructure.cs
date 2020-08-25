using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Header("Attributes")]
    [SerializeField] protected Transform attackingRange;
    [SerializeField] protected int enemyTargetAmount;

    protected ResourceBundle attackCost;
    protected List<Transform> enemies = new List<Transform>();
    protected List<Transform> targetedEnemies = new List<Transform>();

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;

        DetectEnemies();
        CheckResearch();

        /*villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);*/
    }

    public List<Transform> GetTargetedEnemies()
    {
        List<Transform> closestEnemies = new List<Transform>();

        int foundEnemies = 0;
        float closestDistanceSqr;

        while (foundEnemies < (enemies.Count > enemyTargetAmount ? enemyTargetAmount : enemies.Count))
        {
            closestDistanceSqr = Mathf.Infinity;

            foreach (Transform enemy in enemies)
            {
                if (!targetedEnemies.Contains(enemy))
                {
                    if (enemy.GetComponent<Enemy>().IsBeingObserved())
                    {
                        Vector3 directionToTarget = enemy.transform.position - transform.position;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            closestEnemies.Add(enemy);
                            foundEnemies += 1;
                        }
                    }
                }
            }
        }

        return closestEnemies;
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
