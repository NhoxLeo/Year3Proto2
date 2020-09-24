using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : DefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private LightningBolt lightning;
    [SerializeField] private float lightningAmount = 2f;
    [SerializeField] private float lightningDelay = 4f;
    [SerializeField] private float lightningStartDelay = 0.6f;
    [SerializeField] private Transform lightningStartPosition;

    private const float BaseMaxHealth = 300f;
    private const float BaseDamage = 5f;

    private float damage;
    private float time;
    private bool sparkDamage;

    protected override void Awake()
    {
        // set base stats
        base.Awake();
        damage = GetBaseDamage();
        structureName = StructureNames.LightningTower;

        // research
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.LightningTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        sparkDamage = superMan.GetResearchComplete(SuperManager.LightningTowerSuper);

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
        targetableEnemies.Add(EnemyNames.BatteringRam);
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
            if (allocatedVillagers > 0)
            {
                time -= Time.deltaTime;
                if (time <= 0.0f)
                {
                    time = lightningDelay;
                    StartCoroutine(Strike(0.3f));
                }
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
        LightningBolt lightningBolt = Instantiate(lightning, lightningStartPosition.position, Quaternion.identity);
        lightningBolt.Initialize(_target, damage, sparkDamage);
        lightningBolt.Fire();
        GameManager.CreateAudioEffect("Zap", _target.position, 0.6f);
    }

    public float GetFireRate()
    {
        return lightningAmount * (1f / lightningDelay);
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        lightningAmount = allocatedVillagers * 2f;
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
        damage = GetBaseDamage() * Mathf.Pow(SuperManager.ScalingFactor, level - 1);
        health = GetTrueMaxHealth();
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // fortification upgrade
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.LightningTowerFortification))
        {
            maxHealth *= 1.5f;
        }

        // level
        maxHealth *= Mathf.Pow(SuperManager.ScalingFactor, level - 1);

        // poor timber multiplier
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.PoorTimber))
        {
            maxHealth *= 0.5f;
        }

        return maxHealth;
    }

    private float GetBaseDamage()
    {
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.LightningTowerPower) ? 1.3f : 1.0f);
    }
}
