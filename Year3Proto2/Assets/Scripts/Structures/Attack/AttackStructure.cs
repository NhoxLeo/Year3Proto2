using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    protected List<GameObject> enemies;
    protected GameObject target = null;
    protected GameObject puffPrefab;
    protected ResourceBundle attackCost;
    private Transform attackingRange;

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

    protected override void Awake()
    {
        base.Awake();
        attackingRange = transform.Find("Range");
    }

    protected override void Start()
    {
        base.Start();
        puffPrefab = Resources.Load("EnemyPuffEffect") as GameObject;
        structureType = StructureType.Attack;
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
        }
    }

    public override void ShowRangeDisplay(bool _active)
    {
        base.ShowRangeDisplay(_active);
        attackingRange.GetChild(0).gameObject.SetActive(_active);
    }

    public override void OnSelected()
    {
        base.OnSelected();
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
    }
}
