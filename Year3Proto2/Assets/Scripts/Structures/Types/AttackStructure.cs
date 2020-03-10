using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    protected GameObject target = null;
    protected List<GameObject> enemies;


    protected void AttackStart()
    {
        StructureStart();
        structureType = StructureType.attack;
        enemies = new List<GameObject>();
    }

    protected void AttackUpdate()
    {
        if (attachedTile != null)
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
        }
    }

    public abstract void Attack(GameObject target);

    public List<GameObject> GetEnemies()
    {
        return enemies;
    }
}
