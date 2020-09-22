using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : DefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private LightningBolt lightning;
    [SerializeField] private float lightningAmount;
    [SerializeField] private float lightningDelay = 2.0f;
    [SerializeField] private float lightningStartDelay = 0.6f;

    private float time;
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 300.0f;
        health = maxHealth;
        time = lightningStartDelay;
        structureName = StructureNames.LightningTower;
    }

    protected override void Start()
    {
        base.Start();
        time = lightningStartDelay;
    }

    protected override void Update()
    {
        base.Update();
        if(isPlaced && enemies.Count > 0)
        {
            time -= Time.deltaTime;
            if (time <= 0.0f)
            {
                time = lightningDelay;
                StartCoroutine(Strike(0.4f));
            }
        } 
        else
        {
            time = lightningStartDelay;
        }
    }

    IEnumerator Strike(float seconds)
    {
        List<Transform> enemies = GetEnemies();
        float enemyAmount = enemies.Count >= lightningAmount ? lightningAmount : enemies.Count;
        for (int i = 0; i < enemyAmount; i++)
        {
            if (enemies[i] == null) break;

            yield return new WaitForSeconds(seconds);
            Transform transform = enemies[i];
            Enemy enemy = transform.GetComponent<Enemy>();
            if(enemy)
            {
                Vector3 location = this.transform.position;
                location.y = 1.5f;
                LightningBolt lightningBolt = Instantiate(lightning, location, Quaternion.identity);
                lightningBolt.Fire(transform);
            }
        }

        yield return null;
    }
}
