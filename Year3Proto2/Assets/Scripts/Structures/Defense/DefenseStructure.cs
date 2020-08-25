using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Header("Attributes")]
    [SerializeField] private Transform attackingRange;

    [Header("Projectile")]
    [SerializeField] protected Transform projectilePrefab;
    [SerializeField] private float projectileTime;
    [SerializeField] private float projectileDelay;
    [SerializeField] private float projectileRate;

    protected ResourceBundle attackCost;
    protected Transform enemy;
    protected List<Transform> enemies = new List<Transform>();

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;
        projectileTime = projectileDelay;

        DetectEnemies();
        CheckResearch();

        /*villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);*/
    }

    protected override void Update()
    {
        base.Update();
        if (attachedTile && enemies.Count > 0)
        {
            enemies.RemoveAll(enemy => !enemy);
            if (!enemy)
            {
                enemy = GetClosestEnemy();
            }
            else
            {
                projectileTime += Time.deltaTime;
                if (projectileTime >= projectileDelay && GameManager.GetInstance().playerResources.AttemptPurchase(attackCost))
                {
                    if(Launch())
                    {
                        projectileTime = 0.0f;
                        projectileRate = allocatedVillagers * 0.5f + projectileDelay;
                        projectileDelay = projectileRate;
                    }
                }
            }
        }
    }

    public abstract bool Launch();

    private Transform GetClosestEnemy()
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

    private void DetectEnemies()
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

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();
        if (enemy)
        {
            resourceDelta -= attackCost * projectileRate;
        }
        return resourceDelta;
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

    public float GetFireRate()
    {
        return projectileRate;
    }

    public List<Transform> GetEnemies()
    {
        return enemies;
    }

    public abstract void CheckResearch();
}
