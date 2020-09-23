using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileDefenseStructure : DefenseStructure
{
    [Header("Projectile")]
    [SerializeField] protected Transform projectilePrefab;
    [SerializeField] protected Transform projectileLocation;
    [SerializeField] protected int projectileAmount;
    [SerializeField] private float projectileTime;
    [SerializeField] protected float projectileDelay;
    [SerializeField] protected float projectileRate;
    [SerializeField] protected float projectileRateFactor;

    protected Transform target;

    protected override void Update()
    {
        base.Update();
        if (attachedTile && enemies.Count > 0 && allocatedVillagers != 0)
        {
            projectileTime += Time.deltaTime;
            if (projectileTime >= projectileDelay && GameManager.GetInstance().playerResources.AttemptPurchase(attackCost))
            {
                GetTargetedEnemies().ForEach(target => {
                    Launch(target);
                    this.target = target;
                });
                projectileTime = 0.0f;
            }
        }
    }

    private List<Transform> GetTargetedEnemies()
    {
        List<Transform> closestEnemies = new List<Transform>();
          
        float closestDistanceSqr;

        for (int i = 0; i < projectileAmount; i++)
        {
            closestDistanceSqr = Mathf.Infinity;
            foreach (Transform target in enemies)
            {
                if (closestEnemies.Count >= projectileAmount) break;
                if (!closestEnemies.Contains(target))
                {
                    if (target.GetComponent<Enemy>().IsBeingObserved())
                    {
                        Vector3 directionToTarget = target.transform.position - transform.position;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            closestEnemies.Add(target);
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
