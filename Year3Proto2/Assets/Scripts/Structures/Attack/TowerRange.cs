using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRange : MonoBehaviour
{
    AttackStructure parent;

    private void Start()
    {
        GetParent();
    }

    private void GetParent()
    {
        parent = transform.parent.GetComponent<AttackStructure>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (parent == null) { GetParent(); }
        // if we're talking about the enemy's trigger collider...
        if (other.gameObject.GetComponent<Enemy>() != null)
        {
            // ignore it
            return;
        }
        // if we're talking about the enemy's structure specific trigger collider...
        if (other.transform.parent)
        {
            if (other.transform.parent.gameObject.GetComponent<Enemy>() != null)
            {
                if (!parent.GetEnemies().Contains(other.transform.parent.gameObject))
                {
                    parent.GetEnemies().Add(other.transform.parent.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parent == null) { GetParent(); }
        // if we're talking about the enemy's trigger collider...
        if (other.gameObject.GetComponent<Enemy>() != null)
        {
            // ignore it
            return;
        }
        // if we're talking about the enemy's structure specific trigger collider...
        if (other.transform.parent)
        {
            if (other.transform.parent.gameObject.GetComponent<Enemy>() != null)
            {
                if (parent.GetEnemies().Contains(other.transform.parent.gameObject))
                {
                    parent.GetEnemies().Remove(other.transform.parent.gameObject);
                }
            }
        }
        
    }
}
