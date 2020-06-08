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
                Unit unit = other.transform.parent.GetComponent<Unit>();
                if(unit)
                {
                    if(unit.GetType() == UnitType.ENEMY)
                    {
                        if (!parentStructure) { GetParent(); }
                        if (!parentStructure.GetEnemies().Contains(other.transform.parent))
                        {
                            parentStructure.GetEnemies().Add(other.transform.parent);
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
                Unit unit = other.transform.parent.GetComponent<Unit>();
                if (unit)
                {
                    if (unit.GetType() == UnitType.ENEMY)
                    {
                        if (!parentStructure) { GetParent(); }
                        if (parentStructure.GetEnemies().Contains(other.transform.parent))
                        {
                            parentStructure.GetEnemies().Remove(other.transform.parent);
                        }
                    }
                }
            }
        }
    }
}
