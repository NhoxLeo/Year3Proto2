using UnityEngine;


public class Catapult : ProjectileDefenseStructure
{
    private const int MetalCost = 4;
    public GameObject boulder;

    private const float BoulderSpeed = 1f;
    private const float BaseMaxHealth = 470f;
    private const float BaseDamage = 15f;

    private float damage;
    private float boulderExplosionRadius = 0.375f;

    private MeshRenderer catapultMesh;
    protected override void Awake()
    {
        // set base stats
        base.Awake();
        damage = GetBaseDamage();
        structureName = StructureNames.Catapult;

        // research
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.CatapultRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
        if (superMan.GetResearchComplete(SuperManager.CatapultSuper))
        {
            boulderExplosionRadius *= 1.5f;
        }
        attackCost = new ResourceBundle(0, 0, superMan.GetResearchComplete(SuperManager.CatapultEfficiency) ? MetalCost / 2 : MetalCost);
        
        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);
        catapultMesh = transform.GetChild(2).GetComponent<MeshRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        if (target)
        {
            Vector3 difference = catapultMesh.transform.position - target.position;
            difference.y = 0.0f;

            Quaternion rotation = Quaternion.LookRotation(-difference);
            catapultMesh.transform.rotation = Quaternion.Slerp(catapultMesh.transform.rotation, rotation * Quaternion.AngleAxis(90.0f, Vector3.up), Time.deltaTime * 2.5f);
        }
    }

    public override void Launch(Transform _target)
    {
        GameObject newBoulder = Instantiate(boulder, projectileLocation.position, Quaternion.identity, transform);
        BoulderBehaviour boulderBehaviour = newBoulder.GetComponent<BoulderBehaviour>();
        boulderBehaviour.target = _target.position;
        boulderBehaviour.damage = damage;
        boulderBehaviour.speed = BoulderSpeed;
        boulderBehaviour.explosionRadius = boulderExplosionRadius;
        GameManager.CreateAudioEffect("catapultFire", transform.position, SoundType.SoundEffect, 0.6f);

        /*
        Vector3 position = transform.position;
        position.y = 1.5f;

        Transform projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        Boulder boulder = projectile.GetComponent<Boulder>();
        if (boulder)
        {
            float explosionFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultSuper) ? 1.5f : 1.0f;
            float damageFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultPower) ? 1.3f : 1.0f;

            boulder.ExplosionRadius = boulder.ExplosionRadius * explosionFactor;
            boulder.SetDamage(boulder.GetDamage() * damageFactor * level);
            boulder.SetTarget(_target);
        }
        */
    }

    public override void OnAllocation()
    {
        base.OnAllocation();
        projectileRate = 0.2f + (allocatedVillagers * 0.05f);
        if (allocatedVillagers != 0)
        {
            projectileDelay = 1f / projectileRate;
        }
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
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultFortification))
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
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultPower) ? 1.3f : 1.0f);
    }

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
        catapultMesh.materials[0].SetColor("_BaseColor", _colour);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }
}
