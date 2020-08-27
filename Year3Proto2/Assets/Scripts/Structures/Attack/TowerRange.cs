using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    private DefenseStructure defenseStructure;
    private int enemyStructureColliderLayer;

    private void Start()
    {
        GetParent();
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
    }

    private void GetParent()
    {
        defenseStructure = transform.parent.GetComponent<DefenseStructure>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                if (other.transform.parent.GetComponent<Enemy>())
                {
                    if (!defenseStructure) { GetParent(); }
                    if (!defenseStructure.GetEnemies().Contains(other.transform.parent))
                    {
                        defenseStructure.GetEnemies().Add(other.transform.parent);
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
                if (other.transform.parent.GetComponent<Enemy>())
                {
                    if (!defenseStructure) { GetParent(); }
                    if (defenseStructure.GetEnemies().Contains(other.transform.parent))
                    {
                        defenseStructure.GetEnemies().Remove(other.transform.parent);
                    }
                }
            }
        }
    }
}
