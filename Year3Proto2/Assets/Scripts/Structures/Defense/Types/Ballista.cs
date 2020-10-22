using UnityEngine;

public class Ballista : ProjectileDefenseStructure
{
    [SerializeField] private Transform ballista;

    [SerializeField] private GameObject arrowPrefab;

    private const int MetalCost = 2;
    private const float BaseMaxHealth = 400f;
    private const float BaseDamage = 30f;
    private const float ArrowSpeed = 12.5f;

    private float damage;
    private bool arrowPierce;
    private MeshRenderer ballistaMesh;

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
        attackCost = new ResourceBundle(0, 0, (arrowPierce ? 3 : 1) * (superMan.GetResearchComplete(SuperManager.BallistaEfficiency) ? MetalCost / 2 : MetalCost));

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);
        ballistaMesh = transform.GetChild(2).GetChild(0).GetComponent<MeshRenderer>();
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
        float projectileOffset = 0.2f;
        int enemyCount = arrowPierce ? 3 : 1;

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

            GameObject newArrow = Instantiate(arrowPrefab, projectileLocation.position, Quaternion.identity);
            BoltBehaviour arrowBehaviour = newArrow.GetComponent<BoltBehaviour>();
            arrowBehaviour.Initialize(targetPosition, damage, ArrowSpeed, arrowPierce);
            GameManager.CreateAudioEffect("arrow", transform.position, SoundType.SoundEffect, 0.6f);
        }
    }


    public override void OnAllocation()
    {
        base.OnAllocation();
        projectileRate = 0.2f + (allocatedVillagers * 0.1f);
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

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
        ballistaMesh.materials[0].SetColor("_BaseColor", _colour);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }
}
