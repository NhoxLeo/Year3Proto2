using UnityEngine;

public class Ballista : ProjectileDefenseStructure
{
    [SerializeField] private Transform ballista;

    private const int MetalCost = 2;
    private const float MaxHealth = 350;

    protected override void Awake()
    {
        base.Awake();

        maxHealth = MaxHealth;
        health = maxHealth;

        structureName = StructureNames.Ballista;

        if(SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaFortification))
        {
            health = maxHealth *= 1.5f;
        }

        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        attackCost = new ResourceBundle(0, SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaEfficiency) ? MetalCost / 2 : MetalCost, 0);
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
