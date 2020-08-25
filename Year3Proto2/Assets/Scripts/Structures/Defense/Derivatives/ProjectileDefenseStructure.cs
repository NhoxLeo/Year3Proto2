using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileDefenseStructure : DefenseStructure
{
    [Header("Projectile")]
    [SerializeField] protected Transform projectilePrefab;
    [SerializeField] private float projectileTime;
    [SerializeField] private float projectileDelay;
    [SerializeField] private float projectileRate;
    [SerializeField] private float projectileRateFactor;

    protected override void Update()
    {
        base.Update();
        if (attachedTile && enemies.Count > 0)
        {
            projectileTime += Time.deltaTime;
            if (projectileTime >= projectileDelay && GameManager.GetInstance().playerResources.AttemptPurchase(attackCost))
            {
                enemies.RemoveAll(enemy => !enemy);
                GetTargetedEnemies().ForEach(target => Launch(target));
   
                projectileTime = 0.0f;
                projectileRate = allocatedVillagers * projectileRateFactor + projectileDelay;
                projectileDelay = projectileRate;
            }
        }
    }

    private List<Transform> GetTargetedEnemies()
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

    public abstract void Launch(Transform _target);

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();
        if(enemies.Count > 0)
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
