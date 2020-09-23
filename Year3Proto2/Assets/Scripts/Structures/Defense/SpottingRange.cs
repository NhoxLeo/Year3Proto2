using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpottingRange : MonoBehaviour
{
    int enemyStructureColliderLayer;

    private void Start()
    {
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
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
