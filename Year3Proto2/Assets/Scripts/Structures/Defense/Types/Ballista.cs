using UnityEngine;

public class Ballista : ProjectileDefenseStructure
{
    [SerializeField] private Transform ballista;

    [SerializeField] private GameObject arrowPrefab;
    public static bool arrowPierce;
    private float arrowDamage = 10f;
    private float arrowSpeed = 12.5f;

    private const int MetalCost = 2;
    private const float MaxHealth = 350;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = MaxHealth;
        health = maxHealth;

        structureName = StructureNames.Ballista;

        SuperManager superMan = SuperManager.GetInstance();

        if (superMan.GetResearchComplete(SuperManager.BallistaFortification))
        {
            health = maxHealth *= 1.5f;
        }

        if (superMan.GetResearchComplete(SuperManager.BallistaRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
        arrowPierce = superMan.GetResearchComplete(SuperManager.BallistaSuper);
        attackCost = new ResourceBundle(0, superMan.GetResearchComplete(SuperManager.BallistaEfficiency) ? MetalCost / 2 : MetalCost, 0);

        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.FlyingInvader);
        targetableEnemies.Add(EnemyNames.Petard);
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
        arrowBehaviour.Initialize(_target, arrowDamage, arrowSpeed, arrowPierce);
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
}
