﻿using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Range(1, 5)] protected int level = 1;
    protected ResourceBundle attackCost;
    protected List<Transform> enemies = new List<Transform>();
    protected Transform target;

    private Transform attackingRange;

    public void DetectEnemies()
    {
        SphereCollider rangeCollider = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
            if (distanceFromEnemy <= rangeCollider.radius)
            {
                if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Defense;
        attackingRange = transform.Find("Range");
    }

    protected override void Start()
    {
        base.Start();
        DetectEnemies();
        CheckResearch();
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

    public abstract void CheckLevel();

    public abstract void CheckResearch();
}
