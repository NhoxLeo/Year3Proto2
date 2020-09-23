using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    DefenseStructure defenseParent;
    int enemyStructureColliderLayer;

    private void Start()
    {
        GetParent();
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
    }

    private void GetParent()
    {
        defenseParent = transform.parent.GetComponent<DefenseStructure>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                Enemy enemy = other.transform.parent.GetComponent<Enemy>();
                if (enemy)
                {
                    if (!defenseParent) { GetParent(); }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.enemyName))
                    {
                        if (!defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Add(other.transform.parent);
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                Enemy enemy = other.transform.parent.GetComponent<Enemy>();
                if (enemy)
                {
                    if (!defenseParent) { GetParent(); }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.enemyName))
                    {
                        if (defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Remove(other.transform.parent);
                        }
                    }
                }
            }
        }
    }
}
