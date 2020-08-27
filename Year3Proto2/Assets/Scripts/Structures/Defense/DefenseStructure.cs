using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    protected List<GameObject> enemies;
    private Transform attackingRange;

    public List<GameObject> GetEnemies()
    {
        return enemies ?? (enemies = new List<GameObject>());
    }

    public void DetectEnemies()
    {
        GetEnemies();
        SphereCollider rangeCollider = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
            if (distanceFromEnemy <= rangeCollider.radius)
            {
                if (!enemies.Contains(enemy.gameObject)) { enemies.Add(enemy.gameObject); }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        attackingRange = transform.Find("Range");
    }

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;
        enemies = new List<GameObject>();
        DetectEnemies();
    }

    protected override void Update()
    {
        base.Update();
        enemies.RemoveAll(enemy => !enemy);
    }

    public override void ShowRangeDisplay(bool _active)
    {
        base.ShowRangeDisplay(_active);
        attackingRange.GetChild(0).gameObject.SetActive(_active);
    }

    public List<Transform> GetEnemies()
    {
        return enemies;
    }

    public abstract void CheckResearch();
}
