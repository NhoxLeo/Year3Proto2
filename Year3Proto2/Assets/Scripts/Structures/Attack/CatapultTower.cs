using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    private GameObject spawnedBoulder;

    private float speed = 0.8f; 
    private float arcFactor = 0.45f;
    private float distanceTravelled;

    private Vector3 current;
    private Vector3 origin;

    private Vector3 final;


    void Start()
    {
        AttackStart();
        structureName = "Catapult Tower";
    }

    private void Update()
    {
        AttackUpdate();
    }

    public override void Attack(GameObject target)
    {
        if (attachedTile == null && spawnedBoulder != null)
        { 
            enemies.Clear();
            Destroy(spawnedBoulder);
        }

        if (spawnedBoulder == null)
        {
            Vector3 initialPosition = transform.position + new Vector3(0.0f, transform.localScale.y / 2.0f, 0.0f);
            spawnedBoulder = Instantiate(boulder, initialPosition, Quaternion.identity, transform);
            origin = current = initialPosition;
            distanceTravelled = 0.0f;

            Vector3 position = target.transform.position;
            position.y = -0.5f;

            final = position;
        }
        else
        {
            if (target == null) Destroy(spawnedBoulder.gameObject);

            Vector3 direction = final - current;
            current += direction.normalized * speed * Time.deltaTime;
            distanceTravelled += speed * Time.deltaTime;

            float totalDistance = Vector3.Distance(origin, final);
            float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
            spawnedBoulder.transform.position = current + new Vector3(0, heightOffset, 0);

            if (spawnedBoulder.transform.position.y <= 0)
            {
                Destroy(spawnedBoulder);

                foreach (GameObject enemy in new List<GameObject>(enemies))
                {
                    if(Vector3.Distance(enemy.transform.position, spawnedBoulder.transform.position) < 1.0f)
                    {
                        enemies.Remove(enemy);
                        Destroy(enemy);
                    }
                }
            }
        }

        if(enemies.Count <= 0)
        {
            Destroy(spawnedBoulder);
        }
    }
}
