using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    AttackStructure parentStructure;
    int enemyStructureColliderLayer;

    private void Start()
    {
        GetParent();
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
    }

    private void GetParent()
    {
        parentStructure = transform.parent.GetComponent<AttackStructure>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                if (other.transform.parent.GetComponent<Enemy>())
                {
                    if (!parentStructure) { GetParent(); }
                    if (!parentStructure.GetEnemies().Contains(other.transform.parent.gameObject))
                    {
                        parentStructure.GetEnemies().Add(other.transform.parent.gameObject);
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
                    if (!parentStructure) { GetParent(); }
                    if (parentStructure.GetEnemies().Contains(other.transform.parent.gameObject))
                    {
                        parentStructure.GetEnemies().Remove(other.transform.parent.gameObject);
                    }
                }
            }
        }
    }
}
