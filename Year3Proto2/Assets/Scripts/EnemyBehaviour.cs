using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    [Header("Enemy")]

    [SerializeField] [Tooltip("The movement speed of the enemy.")]
    private float enemySpeed;
    [SerializeField] [Tooltip("The range of the enemy.")]
    private float enemyRange;

    private GameObject playerBase;

    // Start is called before the first frame update
    void Start()
    {
        playerBase = GameObject.Find("Base");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToBase = playerBase.transform.position - transform.position;
        Vector3 directionXZ = directionToBase;
        directionXZ.y = 0f;
        if (directionXZ.magnitude > enemyRange)
        {
            directionXZ = directionXZ.normalized;
            transform.position += directionXZ * Time.deltaTime * enemySpeed;
        }
    }
}
