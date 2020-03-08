using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    private GameObject spawnedBoulder;

    private float speed = 0.6f; 
    private float arcFactor = 0.45f;
    private float distanceTravelled;

    private Vector3 current;
    private Vector3 origin;


    void Start()
    {
        AttackStart();
    }

    private void Update()
    {
        AttackUpdate();
    }

    public override void Attack(GameObject target)
    {
        if(spawnedBoulder == null)
        {
            Vector3 initialPosition = transform.position + new Vector3(0.0f, transform.localScale.y / 2.0f, 0.0f);
            spawnedBoulder = Instantiate(boulder, initialPosition, Quaternion.identity, transform);
            origin = current = initialPosition;
            distanceTravelled = 0.0f;
        }
        else
        {
            Vector3 direction = target.transform.position - current;
            current += direction.normalized * speed * Time.deltaTime;
            distanceTravelled += speed * Time.deltaTime;

            float totalDistance = Vector3.Distance(origin, target.transform.position);
            float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
            spawnedBoulder.transform.position = current + new Vector3(0, heightOffset, 0);

            if (Vector3.Distance(spawnedBoulder.transform.position, target.transform.position) < 0.01f)
            {
                enemies.Remove(target);

                Destroy(spawnedBoulder);
                Destroy(target);
            }
        }
    }
}
