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
        if (!parent.GetEnemies().Contains(other.gameObject) && other.gameObject.GetComponent<Enemy>() != null)
        {
            parent.GetEnemies().Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parent == null) { GetParent(); }
        if (parent.GetEnemies().Contains(other.gameObject) && other.gameObject.GetComponent<Enemy>() != null)
        {
            parent.GetEnemies().Remove(other.gameObject);
        }
    }
}
