using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour
{
    public Transform target;
    public float damage;
    public float speed;
    public GameObject puffEffect;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
        transform.LookAt(target.position);
        if (Vector3.Distance(transform.position, target.position) <= 0.05f)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            enemy.DealDamage(damage);
            Destroy(gameObject);
        }

    }
}
