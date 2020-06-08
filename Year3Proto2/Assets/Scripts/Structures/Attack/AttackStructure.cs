using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    protected List<Transform> units;
    protected Transform target = null;
    public float consumptionTime = 2f;
    protected float remainingTime = 2f;
    protected GameObject puffPrefab;
    protected ResourceBundle attackCost;

    public abstract void Attack(Transform target);

    public List<Transform> GetEnemies()
    {
        return units ?? (units = new List<Transform>());
    }

    public void DetectEnemies()
    {
        GetEnemies();
        SphereCollider rangeCollider = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
        foreach(Unit unit in FindObjectsOfType<Unit>())
        {
            if (unit.GetType() == UnitType.ENEMY)
            {
                float distanceFromEnemy = (unit.transform.position - transform.position).magnitude;
                if (distanceFromEnemy <= rangeCollider.radius)
                {
                    if (!units.Contains(unit.transform)) { units.Add(transform); }
                }
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        puffPrefab = Resources.Load("EnemyPuffEffect") as GameObject;
        structureType = StructureType.attack;
        units = new List<Transform>();
        DetectEnemies();
    }

    protected override void Update()
    {
        base.Update();
        if (isPlaced)
        {
            if (units.Count > 0)
            {
                units.RemoveAll(unit => !unit);
                if (!target)
                { 
                    float closestDistanceSqr = Mathf.Infinity;
                    Vector3 currentPosition = transform.position;

                    Transform nearestEnemy = null;

                    foreach (Transform unit in units)
                    {
                        Vector3 directionToTarget = unit.position - currentPosition;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            nearestEnemy = unit;
                        }
                    }

                    if (nearestEnemy) target = nearestEnemy;
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
                if (gameMan.playerResources.CanAfford(new ResourceBundle(foodAllocation, 0, 0)))
                {
                    gameMan.AddBatch(new ResourceBatch(-foodAllocation, ResourceType.food));
                }
            }
        }
    }

    public void ShowRangeDisplay(bool _active)
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(_active);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        ShowRangeDisplay(true);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        ShowRangeDisplay(false);
    }
}
