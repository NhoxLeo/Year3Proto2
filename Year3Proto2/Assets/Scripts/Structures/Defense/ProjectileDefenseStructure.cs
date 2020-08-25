using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileDefenseStructure : DefenseStructure
{
    [Header("Projectile")]
    [SerializeField] protected Transform projectilePrefab;
    [SerializeField] private float projectileTime;
    [SerializeField] private float projectileDelay;
    [SerializeField] private float projectileRate;

    public override void CheckResearch()
    {
        throw new System.NotImplementedException();
    }

    public override bool Launch()
    {
        throw new System.NotImplementedException();
    }

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
                    if (Launch())
                    {
                        projectileTime = 0.0f;
                        projectileRate = allocatedVillagers * 0.5f + projectileDelay;
                        projectileDelay = projectileRate;
                    }
                }
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

    public float GetFireRate()
    {
        return projectileRate;
    }
}
