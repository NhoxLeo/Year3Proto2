using UnityEngine;

public class Ballista : ProjectileDefenseStructure
{
    [SerializeField] private Transform ballista;

    [SerializeField] private GameObject arrowPrefab;

    private const int MetalCost = 2;
    private const float BaseMaxHealth = 350f;
    private const float BaseDamage = 10f;
    private const float ArrowSpeed = 12.5f;

    private float damage;
    private bool arrowPierce;

    protected override void Awake()
    {
        // set base stats
        base.Awake();
        damage = GetBaseDamage();
        structureName = StructureNames.Ballista;

        // research
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.BallistaRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
        arrowPierce = superMan.GetResearchComplete(SuperManager.BallistaSuper);
        attackCost = new ResourceBundle(0, superMan.GetResearchComplete(SuperManager.BallistaEfficiency) ? MetalCost / 2 : MetalCost, 0);

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);
    }

    protected override void Update()
    {
        base.Update();
        if (target)
        {
            Vector3 difference = ballista.position - target.position;
            difference.y = 0.0f;

            Quaternion rotation = Quaternion.LookRotation(difference);
            ballista.transform.rotation = Quaternion.Slerp(ballista.transform.rotation, rotation * Quaternion.AngleAxis(90.0f, Vector3.up), Time.deltaTime * 2.5f);
        }
    }

    public override void Launch(Transform _target)
    {
        GameObject newArrow = Instantiate(arrowPrefab, ballista.transform.position, Quaternion.identity);
        BoltBehaviour arrowBehaviour = newArrow.GetComponent<BoltBehaviour>();
        arrowBehaviour.Initialize(_target, damage, ArrowSpeed, arrowPierce);
        GameManager.CreateAudioEffect("arrow", transform.position);
        /*
        Vector3 position = transform.position;
        position.y = 1.25f;

        Transform projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        Arrow arrow = projectile.GetComponent<Arrow>();
        if (arrow)
        {
            float damageFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f;

            arrow.Pierce = SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaSuper);
            arrow.SetDamage(arrow.GetDamage() * damageFactor * level);
            arrow.SetTarget(_target);
        }
        */
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        projectileRate = allocatedVillagers * 0.5f;
        if (allocatedVillagers != 0)
        {
            projectileDelay = 1f / projectileRate;
        }
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
        damage = GetBaseDamage() * Mathf.Pow(1.25f, level - 1);
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
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaFortification))
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

    private float GetBaseDamage()
    {
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f);
    }
}
