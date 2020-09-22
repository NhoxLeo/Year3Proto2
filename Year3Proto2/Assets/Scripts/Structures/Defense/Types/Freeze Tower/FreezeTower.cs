using System.Collections;

public class FreezeTower : DefenseStructure
{
    private FreezeTowerCannon[] cannons;

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureNames.FreezeTower;
        attackCost = new ResourceBundle(0, 4, 0);
        maxHealth = 450.0f;
        health = maxHealth;

        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
    }

    protected override void Start()
    {
        base.Start();
        cannons = GetComponentsInChildren<FreezeTowerCannon>();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        Refresh();
    }

    private void Refresh()
    {
        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.GetTargets().ForEach(target => {
                if (target)
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(false);
                    }
                }
            });
        }
    }
}