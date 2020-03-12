using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    //public GameObject catapult;

    private GameObject spawnedBoulder;

    private float speed = 0.8f; 
    private float arcFactor = 0.60f;
    private float distanceTravelled;

    private Vector3 current;
    private Vector3 origin;

    private Vector3 final;


    void Start()
    {
        AttackStart();
        structureName = "Catapult Tower";
        maxHealth = 450f;
        health = maxHealth;
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
            if (spawnedBoulder) Destroy(spawnedBoulder);
        }

        if (enemies.Count <= 0)
        {
            if (spawnedBoulder) Destroy(spawnedBoulder);
        }

        if (target == null)
        {
            if (spawnedBoulder) Destroy(spawnedBoulder);
        }
        else
        {
            /*Vector3 catapultPosition = catapult.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = catapultPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            catapult.transform.rotation = Quaternion.Slerp(catapult.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * speed);
            */
        }

        if (spawnedBoulder == null)
        {
            Vector3 initialPosition = transform.position + new Vector3(0.0f, transform.localScale.y / 2.0f, 0.0f);
            spawnedBoulder = Instantiate(boulder, initialPosition, Quaternion.identity, transform);
            GameManager.CreateAudioEffect("catapultFire", transform.position);
            origin = current = initialPosition;
            distanceTravelled = 0.0f;
            final = target.transform.position;
        }
        else
        {
            if (spawnedBoulder)
            {
                Vector3 direction = final - current;
                current += direction.normalized * speed * Time.deltaTime;
                distanceTravelled += speed * Time.deltaTime;

                float totalDistance = Vector3.Distance(origin, final);
                float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
                spawnedBoulder.transform.position = current + new Vector3(0, heightOffset, 0);

                if (spawnedBoulder.transform.position.y <= 0.51f)
                {
                    foreach (GameObject enemy in new List<GameObject>(enemies))
                    {
                        if (Vector3.Distance(enemy.transform.position, spawnedBoulder.transform.position) < 1.0f)
                        {
                            enemies.Remove(enemy);
                            Destroy(enemy);
                        }
                    }

                    Instantiate(Resources.Load("Explosion") as GameObject, spawnedBoulder.transform.position, Quaternion.identity);
                    Destroy(spawnedBoulder);
                }
            }
        }
    }
}
