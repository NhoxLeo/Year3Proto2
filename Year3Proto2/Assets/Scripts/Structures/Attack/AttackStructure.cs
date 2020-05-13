using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    protected List<GameObject> enemies;
    protected GameObject target = null;
    public float consumptionTime = 2f;
    protected float remainingTime = 2f;
    protected GameObject puffPrefab;

    public abstract void Attack(GameObject target);

    public List<GameObject> GetEnemies()
    {
        return enemies;
    }

    protected void AttackStart()
    {
        puffPrefab = Resources.Load("EnemyPuffEffect") as GameObject;
        StructureStart();
        structureType = StructureType.attack;
        enemies = new List<GameObject>();
    }

    protected void AttackUpdate()
    {
        if (isPlaced)
        {
            StructureUpdate();

            if (enemies.Count > 0)
            {
                enemies.RemoveAll(enemy => enemy == null);
                if (target == null)
                { 
                    float closestDistanceSqr = Mathf.Infinity;
                    Vector3 currentPosition = transform.position;

                    GameObject nearestEnemy = null;

                    foreach (GameObject enemy in enemies)
                    {
                        Vector3 directionToTarget = enemy.transform.position - currentPosition;
                        float dSqrToTarget = directionToTarget.sqrMagnitude;
                        if (dSqrToTarget < closestDistanceSqr)
                        {
                            closestDistanceSqr = dSqrToTarget;
                            nearestEnemy = enemy;
                        }
                    }

                    if (nearestEnemy != null) target = nearestEnemy;
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
                if (gameMan.playerData.CanAfford(new ResourceBundle(foodAllocation, 0, 0)))
                {
                    gameMan.AddBatch(new Batch(-foodAllocation, ResourceType.food));
                }
            }
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
    }
}
