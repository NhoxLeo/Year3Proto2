using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : DefenseStructure
{
    private GameObject soldierPrefab;
    private int maxSoldiers = 5;
    private List<Soldier> soldiers;
    private float trainTime = 20f;
    private float timeTrained = 0f;

    private void UpdateHealAndTrainRate()
    {
        switch (foodAllocation)
        {
            case 1:
                trainTime = 30f;
                SetHealRate(1f);
                break;
            case 2:
                trainTime = 25f;
                SetHealRate(1.2f);
                break;
            case 3:
                trainTime = 20f;
                SetHealRate(1.5f);
                break;
            case 4:
                trainTime = 15f;
                SetHealRate(2f);
                break;
            case 5:
                trainTime = 10f;
                SetHealRate(3f);
                break;
        }

    }

    private void SetHealRate(float _healRate)
    {
        foreach (Soldier soldier in soldiers)
        {
            soldier.healRate = _healRate;
        }
    }

    public override void IncreaseFoodAllocation()
    {
        base.IncreaseFoodAllocation();
        UpdateHealAndTrainRate();
    }

    public override void DecreaseFoodAllocation()
    {
        base.DecreaseFoodAllocation();
        UpdateHealAndTrainRate();
    }

    protected override void Start()
    {
        base.Start();
        UpdateHealAndTrainRate();
    }

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Barracks];

        maxHealth = 200f;
        health = maxHealth;
    }

    protected override void Update()
    {
        base.Update();
        if (soldiers.Count < maxSoldiers)
        {
            timeTrained += Time.deltaTime;
            if (timeTrained >= trainTime)
            {
                timeTrained = 0f;
                SpawnSoldier();
            }
        }
    }

    private void SpawnSoldier()
    {
        // instantiate
        // set home
        // disable a random shield
    }

    private void OnTriggerEnter(Collider other)
    {
        Soldier soldier = other.gameObject.GetComponent<Soldier>();
        if (soldier)
        {
            if (soldier.home == this)
            {
                soldier.canHeal = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Soldier soldier = other.gameObject.GetComponent<Soldier>();
        if (soldier)
        {
            if (soldier.home == this)
            {
                soldier.canHeal = false;
            }
        }
    }
}
