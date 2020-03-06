using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackStructure : Structure
{
    private GameObject target = null;
    protected List<GameObject> enemies;

    protected void AttackStart()
    {
        StructureStart();
        structureType = StructureType.attack;
        enemies = new List<GameObject>();
    }

    private void Update()
    {
        if(enemies.Count > 0)
        {
            if(target == null)
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
                if(nearestEnemy != null) target = nearestEnemy;
            }
            else 
            {
                Attack(target); 
            }
        }
    }

    public abstract void Attack(GameObject target);

    private void OnTriggerEnter(Collider other)
    {
        if (!enemies.Contains(other.gameObject) && other.gameObject.GetComponent<Enemy>() != null)
        {
            Debug.Log(other.name);
            enemies.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemies.Contains(other.gameObject) && other.gameObject.GetComponent<Enemy>() != null)
        {
            enemies.Remove(other.gameObject);
        }
    }
}
