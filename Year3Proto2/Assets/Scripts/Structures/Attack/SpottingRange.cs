using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpottingRange : MonoBehaviour
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
                Enemy enemyComponent = other.transform.parent.GetComponent<Enemy>();
                if (enemyComponent)
                {
                    enemyComponent.AddObserver();
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
                Enemy enemyComponent = other.transform.parent.GetComponent<Enemy>();
                if (enemyComponent)
                {
                    enemyComponent.RemoveObserver();
                }
            }
        }
    }
}
