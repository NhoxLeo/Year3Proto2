using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : DefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private LightningBolt lightning;
    [SerializeField] private float lightningAmount = 2.0f;
    [SerializeField] private float lightningDelay = 4f;
    [SerializeField] private float lightningStartDelay = 0.6f;
    [SerializeField] private Transform lightningStartPosition;

    private const float BaseMaxHealth = 420f;
    private const float BaseDamage = 5f;
    private const float LightningIntraDelay = 0.6f;

    private float damage;
    private float time;
    private bool superAbility;

    private Color normalEmissiveColour;

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

        superAbility = superMan.GetResearchComplete(SuperManager.LightningTowerSuper);

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
        targetableEnemies.Add(EnemyNames.BatteringRam);

        normalEmissiveColour = meshRenderer.materials[0].GetColor("_EmissiveColor");
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
                    StartCoroutine(Strike(LightningIntraDelay));
                    time = lightningDelay;
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
                List<Transform> previousTargets = null;
                if (superAbility)
                {
                    previousTargets = new List<Transform>();
                    Transform currentTarget = enemiesToStrike[i];
                    for (int j = 0; j < 3; j++)
                    {
                        currentTarget = StrikeEnemy(currentTarget, ref previousTargets);
                        if (currentTarget == null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    StrikeEnemy(enemiesToStrike[i], ref previousTargets);
                }
            }
            yield return new WaitForSeconds(seconds);
        }
        yield return null;
    }

    private Transform StrikeEnemy(Transform _target, ref List<Transform> _previousTargets)
    {
        LightningBolt lightningBolt = Instantiate(lightning);
        GameManager.CreateAudioEffect("Zap", _target.position, SoundType.SoundEffect, 0.6f);
        return lightningBolt.Fire(lightningStartPosition.position, _target, ref _previousTargets, damage); 
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
        float oldMaxHealth = GetTrueMaxHealth() / SuperManager.ScalingFactor;
        float difference = GetTrueMaxHealth() - oldMaxHealth;
        health += difference;
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
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    private float GetBaseDamage()
    {
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.LightningTowerPower) ? 1.3f : 1.0f);
    }

    public override void SetColour(Color _colour)
    {
        meshRenderer.materials[0].SetColor("_BaseColor", _colour);
        meshRenderer.materials[0].SetColor("_EmissiveColor", _colour);
        meshRenderer.materials[1].SetColor("_BaseColor", _colour);
        if (_colour == Color.white)
        {
            meshRenderer.materials[0].SetColor("_EmissiveColor", normalEmissiveColour);
        }
    }
}
