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
    }

    protected override void Start()
    {
        base.Start();
        cannons = GetComponentsInChildren<FreezeTowerCannon>();
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        StartCoroutine(Refresh());
    }

    IEnumerator Refresh()
    {
        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.GetTargets().ForEach(target => {
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy) enemy.Slow(false);
            });
        }

        yield return null;
    }
}