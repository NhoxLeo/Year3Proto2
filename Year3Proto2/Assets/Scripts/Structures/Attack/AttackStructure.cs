using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class AttackStructure : Structure
{
    protected List<GameObject> enemies;
    protected GameObject target = null;
    public float consumptionTime = 2f;
    protected float remainingTime = 2f;
    protected GameObject puffPrefab;
    protected ResourceBundle attackCost;

    public abstract void Attack(GameObject target);

    public List<GameObject> GetEnemies()
    {
        return enemies ?? (enemies = new List<GameObject>());
    }

    public void DetectEnemies()
    {
        GetEnemies();
        SphereCollider rangeCollider = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
            if (distanceFromEnemy <= rangeCollider.radius)
            {
                if (!enemies.Contains(enemy.gameObject)) { enemies.Add(enemy.gameObject); }
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        puffPrefab = Resources.Load("EnemyPuffEffect") as GameObject;
        structureType = StructureType.attack;
        enemies = new List<GameObject>();
        DetectEnemies();
    }

    protected override void Update()
    {
        base.Update();
        if (isPlaced)
        {
            if (enemies.Count > 0)
            {
                enemies.RemoveAll(enemy => !enemy);
                if (!target)
                {
                    float closestDistanceSqr = Mathf.Infinity;

                    GameObject nearestSpottedEnemy = null;

                    foreach (GameObject enemy in enemies)
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

                    if (nearestSpottedEnemy) target = nearestSpottedEnemy;
                }
                else
                {
                    Attack(target);
                }
            }

            // Food consumption
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = consumptionTime;
                if (gameMan.playerResources.CanAfford(new ResourceBundle(0, 0, foodAllocation)))
                {
                    gameMan.AddBatch(new ResourceBatch(-foodAllocation, ResourceType.food));
                }
            }
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta -= new Vector3(0f, 0f, foodAllocation / consumptionTime);

        return resourceDelta;
    }

    public void ShowRangeDisplay(bool _active)
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(_active);
        transform.GetChild(1).GetChild(0).gameObject.SetActive(_active);
    }

    private void EnableFogMask()
    {
        transform.GetChild(1).GetChild(1).DOScale(Vector3.one * 1.0f, 1.0f).SetEase(Ease.OutQuint);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        ShowRangeDisplay(true);
        EnableFogMask();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        ShowRangeDisplay(false);
    }
}
