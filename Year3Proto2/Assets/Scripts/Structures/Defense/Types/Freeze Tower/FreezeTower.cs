using UnityEngine;

public class FreezeTower : DefenseStructure
{
    private FreezeTowerCannon[] cannons;
    private float freezeEffect;
    protected override void Awake()
    {
        base.Awake();

        // Properties

        structureName = StructureNames.FreezeTower;
        attackCost = new ResourceBundle(0, 4, 0);
        maxHealth = 450.0f;
        health = maxHealth;

        // Targetable Enemies

        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);

        // Research Completed

        SuperManager superManager = SuperManager.GetInstance();
        if (superManager.GetResearchComplete(SuperManager.FreezeTowerFortification)) 
        { 
            health = maxHealth *= 1.5f; 
        }

        if (superManager.GetResearchComplete(SuperManager.FreezeTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        attackCost = new ResourceBundle(0, superManager.GetResearchComplete(SuperManager.FreezeTowerEfficiency) ? 3 : 6, 0);
    }

    protected override void Start()
    {
        base.Start();
        SuperManager superManager = SuperManager.GetInstance();
        cannons = GetComponentsInChildren<FreezeTowerCannon>();
        freezeEffect = superManager.GetResearchComplete(SuperManager.FreezeTowerSlowEffect) ? 1.0f : 1.3f;
        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.Setup(
                freezeEffect,
                superManager.GetResearchComplete(SuperManager.FreezeTowerSuper)
            );
        }
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.GetTargets().ForEach(target => {
                if (target)
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(false, 0.0f);
                    }
                }
            });
        }
    }

    public float GetFreezeEffect()
    {
        return 1f - (0.6f * (1f / freezeEffect));
    }
}