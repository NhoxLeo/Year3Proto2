using UnityEngine;

public class ShockWaveTower : DefenseStructure
{
    [Header("Shockwave Tower")]
    [SerializeField] private float startDelay = 1.2f;
    [SerializeField] private Transform particle;
    private float timer = 0.0f;

    private const float BaseMaxHealth = 350.0f;
    private const float MinimumDelay = 3.0f;

    private float delay = MinimumDelay;

    private GameObject boulderModel;
    private MeshRenderer boulderMesh;
    private Vector3 restingPosition;
    private Vector3 readyPosition;

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

        boulderModel = transform.GetChild(3).gameObject;
        boulderMesh = boulderModel.GetComponent<MeshRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        timer = startDelay;
    }

    protected override void Update()
    {
        base.Update();
        if(isPlaced)
        {
            restingPosition = transform.GetChild(4).position;
            readyPosition = transform.GetChild(5).position;
            if (allocatedVillagers > 0)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    timer = 0;
                }
                if (enemies.Count > 0 && timer <= 0.0f)
                {
                    timer = delay;
                    GameManager.CreateAudioEffect("Thud", transform.position, SoundType.SoundEffect, 0.6f);
                    Instantiate(particle, transform.position, particle.rotation);
                    enemies.ForEach(transform => {
                        Enemy enemy = transform.GetComponent<Enemy>();
                        if (enemy)
                        {
                            if (enemy.enemyName != EnemyNames.BatteringRam)
                            {
                                if (enemy.enemyName == EnemyNames.Petard)
                                {
                                    enemy.GetComponent<Petard>().SetOffBarrel();
                                }
                                bool super = SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTowerSuper);
                                float distance = (this.transform.position - transform.position).magnitude;
                                float damage =  5.0f * (1.0f / distance); // Balance total damage
                                enemy.Stun(super ? damage : 0);
                            }
                        }
                    });
                }
            }
            float yPoint = 1f - (timer / delay);
            boulderModel.transform.position = Vector3.Lerp(restingPosition, readyPosition, yPoint);
        }

    }

    public float GetAttackRate()
    {
        return allocatedVillagers == 0 ? 0f : 1f / delay;
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        delay = MinimumDelay + (3 - allocatedVillagers) * 0.5f;
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
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
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTowerFortification))
        {
            maxHealth *= 1.5f;
        }

        // level
        maxHealth *= Mathf.Pow(SuperManager.ScalingFactor, level - 1);

        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
        boulderMesh.materials[0].SetColor("_BaseColor", _colour);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }
}
