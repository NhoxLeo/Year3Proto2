using UnityEngine;


public class Catapult : ProjectileDefenseStructure
{
    private const int MetalCost = 4;
    private const float MaxHealth = 450.0f;
    public GameObject boulder;
    public float boulderDamage = 5f;
    public float boulderExplosionRadius = 0.25f;
    private float boulderSpeed = 1.0f;

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureNames.Catapult;
        maxHealth = MaxHealth;
        health = maxHealth;

        SuperManager superMan = SuperManager.GetInstance();

        if (superMan.GetResearchComplete(SuperManager.CatapultFortification))
        {
            health = maxHealth *= 1.5f;
        }

        if (superMan.GetResearchComplete(SuperManager.CatapultRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        if (superMan.GetResearchComplete(SuperManager.CatapultPower))
        {
            boulderDamage *= 1.3f;
        }
        if (superMan.GetResearchComplete(SuperManager.CatapultSuper))
        {
            boulderExplosionRadius *= 1.5f;
        }

        attackCost = new ResourceBundle(0, superMan.GetResearchComplete(SuperManager.CatapultEfficiency) ? MetalCost / 2 : MetalCost, 0);
        
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
    }

    public override void Launch(Transform _target)
    {
        Vector3 position = transform.position;
        position.y = 1.5f;

        GameObject newBoulder = Instantiate(boulder, position, Quaternion.identity, transform);
        BoulderBehaviour boulderBehaviour = newBoulder.GetComponent<BoulderBehaviour>();
        boulderBehaviour.target = _target.position;
        boulderBehaviour.damage = boulderDamage;
        boulderBehaviour.speed = boulderSpeed;
        boulderBehaviour.explosionRadius = boulderExplosionRadius;
        GameManager.CreateAudioEffect("catapultFire", transform.position);

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
        projectileRate = allocatedVillagers * 0.167f;
        if (allocatedVillagers != 0)
        {
            projectileDelay = 1f / projectileRate;
        }
    }
}
