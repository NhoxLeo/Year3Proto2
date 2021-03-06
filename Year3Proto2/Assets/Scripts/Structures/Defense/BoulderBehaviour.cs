﻿using UnityEngine;

public class BoulderBehaviour : MonoBehaviour
{
    public Vector3 origin = Vector3.zero;
    public Vector3 current = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public float damage = 5f;
    public float speed = 0.8f;
    public float explosionRadius = 0.5f;
    public bool smallBoulder = false;
    private float arcFactor = 0.60f;
    private float distanceTravelled = 0f;
    private float timePassed = 0f;
    [SerializeField] private Transform boulder;

    // Start is called before the first frame update
    void Start()
    {
        origin = current = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        Vector3 direction = target - current;
        current += direction.normalized * speed * Time.deltaTime;
        distanceTravelled += speed * Time.deltaTime;

        float totalDistance = Vector3.Distance(origin, target);
        float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
        transform.position = current + new Vector3(0, heightOffset, 0);

        if (transform.position.y <= 0.5f)
        {
            Contact();
        }
    }

    private void Contact()
    {
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultSuper) && !smallBoulder && boulder != null)
        {
            for (int i = 0; i < 4; i++)
            {
                float distance = 0.32f;
                Vector3 instantiatePosition = transform.position;
                instantiatePosition.y = 0.55f;

                Vector3 position = new Vector3(instantiatePosition.x + (Mathf.Sin(i * 90f * Mathf.Deg2Rad) * distance), 0.5f, instantiatePosition.z + (Mathf.Cos(i * 90f * Mathf.Deg2Rad) * distance));


                Transform newBoulder = Instantiate(boulder, instantiatePosition, Quaternion.identity);
                BoulderBehaviour boulderBehaviour = newBoulder.GetComponent<BoulderBehaviour>();
                boulderBehaviour.smallBoulder = true;
                boulderBehaviour.target = position;
                boulderBehaviour.damage = damage * 0.2f;
                boulderBehaviour.speed = speed * 0.5f;
                boulderBehaviour.arcFactor = 0.82f;
                boulderBehaviour.explosionRadius = explosionRadius * 0.2f;
            }
        }


        RaycastHit[] hitEnemies = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("EnemyStructureCollider"));
        GameObject explosion = Instantiate(GameManager.GetExplosion(1), transform.position, Quaternion.identity);
        explosion.transform.localScale *= 2f * explosionRadius;
        foreach (RaycastHit enemyHit in hitEnemies)
        {
            Enemy enemy = enemyHit.transform.GetComponent<Enemy>();
            if (enemy.GetName() == EnemyNames.Petard)
            {
                enemy.GetComponent<Petard>().SetOffBarrel();
                continue;
            }
            if (enemy)
            {
                enemy.Damage(damage);
            }
        }
        Destroy(gameObject);
        GameManager.CreateAudioEffect("Explosion", transform.position, SoundType.SoundEffect, smallBoulder ? 0.1f : 0.6f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("Hill"))
        {
            if (!smallBoulder || (smallBoulder && timePassed > 0.2f))
            {
                Contact();
            }
        }
    }
    /*
    private void OnCollisionStay(Collision collision)
    {
        if (!smallBoulder || (smallBoulder && timePassed > 0.2f))
        {
            Contact();
        }
    }
    */
}
