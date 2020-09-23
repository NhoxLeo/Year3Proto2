using UnityEngine;

public class ShockWaveTower : DefenseStructure
{
    [Header("Shockwave Tower")]
    [SerializeField] private float startDelay = 1.2f;
    [SerializeField] private Transform particle;
    private float time = 0.0f;

    private const float BaseMaxHealth = 400f;
    private const float MinimumDelay = 3f;

    private float delay = MinimumDelay;

    protected override void Awake()
    {
        // set base stats
        base.Awake();
        structureName = StructureNames.ShockwaveTower;


        // research
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);
    }

    protected override void Start()
    {
        base.Start();
        time = startDelay;
    }

    protected override void Update()
    {
        base.Update();
        if(isPlaced && enemies.Count > 0)
        {
            if (allocatedVillagers > 0)
            {
                time -= Time.deltaTime;
                if(time <= 0.0f)
                {
                    time = delay;

                    Instantiate(particle, transform.position, particle.rotation);
                    enemies.ForEach(transform => {
                        Enemy enemy = transform.GetComponent<Enemy>();
                        if (enemy)
                        {
                            if (enemy.enemyName != EnemyNames.BatteringRam)
                            {
                                enemy.Stun();
                            }
                        }
                    });
                }
            }
        }
        else
        {
            time = startDelay;
        }
    }

    public float GetAttackRate()
    {
        return 1f / delay;
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        delay = MinimumDelay + (3 - allocatedVillagers) * 0.5f;
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
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
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTowerFortification))
        {
            maxHealth *= 1.5f;
        }

        // level
        maxHealth *= Mathf.Pow(1.25f, level - 1);

        // poor timber multiplier
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.PoorTimber))
        {
            maxHealth *= 0.5f;
        }

        return maxHealth;
    }
}
