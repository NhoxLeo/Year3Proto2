using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    [Header("Tower")]

    [SerializeField] [Tooltip("The attack speed of the tower.")]
    private float towerAttackSpeed = 1;
    private float towerAttackCooldown = 1;
    private float towerAttackDelay = 1;

    [SerializeField] [Tooltip("The range of the tower.")]
    private float towerRange = 2;

    private List<GameObject> enemiesWithinRange;

    // Start is called before the first frame update
    void Start()
    {
        towerAttackCooldown = 1f / towerAttackSpeed;
        towerAttackDelay = 0f;
        enemiesWithinRange = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        towerAttackDelay -= Time.deltaTime;
        List<int> enemiesToDestroy = new List<int>();
        for (int i = 0; i < enemiesWithinRange.Count; i++)
        {
            if (towerAttackDelay <= 0f)
            {
                towerAttackDelay = towerAttackCooldown;
                enemiesToDestroy.Add(i);
            }
        }
        //enemiesWithinRange.Clear();
        for (int i = enemiesToDestroy.Count - 1; i >= 0; i--)
        {
            GameObject enemy = enemiesWithinRange[enemiesToDestroy[i]];
            enemiesWithinRange.Remove(enemy);
            Destroy(enemy);
        }
        enemiesToDestroy.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (!enemiesWithinRange.Contains(other.gameObject))
            {
                enemiesWithinRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (enemiesWithinRange.Contains(other.gameObject))
            {
                enemiesWithinRange.Remove(other.gameObject);
            }
        }
    }
}
