using System;
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

        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
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
        bool moreEnemiesThanBolts = enemies.Count > lightningAmount;
        float enemyAmount = moreEnemiesThanBolts ? lightningAmount : enemies.Count;

        // finds targets all at once, then fires on them between delays
        List<Transform> enemiesToStrike = new List<Transform>();
        for (int i = 0; i < enemyAmount; i++)
        {
            enemiesToStrike.Add(enemies[i]);
        }

        for (int i = 0; i < enemiesToStrike.Count; i++)
        {
            if (!enemiesToStrike[i])
            {
                continue;
            }
            Enemy enemy = enemiesToStrike[i].GetComponent<Enemy>();
            if (enemy)
            {
                StrikeEnemy(enemiesToStrike[i]);
            }
            yield return new WaitForSeconds(seconds);
        }

        yield return null;
    }

    private void StrikeEnemy(Transform _target)
    {
        // Location to be updated by random crystal location.
        Vector3 location = transform.position;
        location.y = 1.5f; 
        LightningBolt lightningBolt = Instantiate(lightning, location, Quaternion.identity);
        lightningBolt.Fire(_target);
    }

    public float GetFireRate()
    {
        return lightningAmount * (1f / lightningDelay);
    }
}
