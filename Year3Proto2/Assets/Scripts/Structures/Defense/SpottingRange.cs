using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpottingRange : MonoBehaviour
{
    int enemyStructureColliderLayer;
    List<GameObject> enemiesBeingObserved = new List<GameObject>();

    private void Start()
    {
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                if (!enemiesBeingObserved.Contains(other.transform.parent.gameObject))
                {
                    enemiesBeingObserved.Add(other.transform.parent.gameObject);
                    Enemy enemyComponent = other.transform.parent.GetComponent<Enemy>();
                    if (enemyComponent)
                    {
                        enemyComponent.SeenByObserver(this);
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
                if (enemiesBeingObserved.Contains(other.transform.parent.gameObject))
                {
                    enemiesBeingObserved.Remove(other.transform.parent.gameObject);
                    Enemy enemyComponent = other.transform.parent.GetComponent<Enemy>();
                    if (enemyComponent)
                    {
                        enemyComponent.LostByObserver(this);
                    }
                }
            }
        }
    }
}
