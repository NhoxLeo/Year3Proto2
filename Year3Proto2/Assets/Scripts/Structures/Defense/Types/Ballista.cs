using UnityEngine;

public class Ballista : ProjectileDefenseStructure
{
    [SerializeField] private Transform ballista;

    [SerializeField] private GameObject arrowPrefab;

    private const int MetalCost = 2;
    private const float BaseMaxHealth = 500f;
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
        attackCost = new ResourceBundle(0, 0, superMan.GetResearchComplete(SuperManager.BallistaEfficiency) ? MetalCost / 2 : MetalCost);

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

    /*
     for (int i = 0; i < (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaSuper) ? 3 : 1); i++)
        {
            float damageFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f;
            targetPosition.x += projectileOffset;
            targetPosition.z += projectileOffset;

            arrow.Pierce = SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaSuper);
            arrow.SetDamage(arrow.GetDamage() * damageFactor * level);
            arrow.SetTarget(_target);
            GameObject newArrow = Instantiate(arrowPrefab, ballista.transform.position, Quaternion.identity);
            BoltBehaviour arrowBehaviour = newArrow.GetComponent<BoltBehaviour>();
            arrowBehaviour.Initialize(targetPosition, damage, ArrowSpeed, arrowPierce);
            GameManager.CreateAudioEffect("arrow", transform.position, 0.6f);
        }
     */

    public override void Launch(Transform _target)
    {
        float projectileOffset = 0.2f;
        int enemyCount = SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaSuper) ? 3 : 1;

        Vector3 targetPosition = _target.position;
        if (enemyCount > 1)
        {
            targetPosition.x -= projectileOffset * 2.0f;
            targetPosition.z -= projectileOffset * 2.0f;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            if (enemyCount > 1)
            {
                targetPosition.x += projectileOffset;
                targetPosition.z += projectileOffset;
            }

            GameObject newArrow = Instantiate(arrowPrefab, ballista.transform.position, Quaternion.identity);
            BoltBehaviour arrowBehaviour = newArrow.GetComponent<BoltBehaviour>();
            arrowBehaviour.Initialize(targetPosition, damage, ArrowSpeed, arrowPierce);
            GameManager.CreateAudioEffect("arrow", transform.position, SoundType.SoundEffect, 0.6f);
        }
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        projectileRate = allocatedVillagers * 0.5f;
        if (allocatedVillagers != 0)
        {
            projectileDelay = 1f / projectileRate;
            //projectileAmount = allocatedVillagers;
        }
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
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaFortification))
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
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f);
    }
}
