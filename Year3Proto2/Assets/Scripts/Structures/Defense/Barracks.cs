using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : DefenseStructure
{
    private GameObject soldierPrefab;
    private GameObject puffEffect;
    private int maxSoldiers = 5;
    [HideInInspector]
    public List<Soldier> soldiers;
    private float trainTime = 20f;
    private float timeTrained = 0f;

    public float GetTrainTime()
    {
        return trainTime;
    }

    private void UpdateHealAndTrainRate()
    {
        switch (foodAllocation)
        {
            case 1:
                trainTime = 30f;
                break;
            case 2:
                trainTime = 25f;
                break;
            case 3:
                trainTime = 20f;
                break;
            case 4:
                trainTime = 15f;
                break;
            case 5:
                trainTime = 10f;
                break;
        }
        float soldierMaxHealth = 30f * (superMan.GetResearchComplete(SuperManager.k_iBarracksSoldierHealth) ? 1.5f : 1.0f);
        SetHealRate(soldierMaxHealth / trainTime);
    }

    private void SetHealRate(float _healRate)
    {
        foreach (Soldier soldier in soldiers)
        {
            soldier.healRate = _healRate;
        }
    }

    public override void SetFoodAllocation(int _newFoodAllocation)
    {
        base.SetFoodAllocation(_newFoodAllocation);
        UpdateHealAndTrainRate();
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        foreach (Barracks barracks in FindObjectsOfType<Barracks>())
        {
            barracks.SetFoodAllocation(_allocation);
        }
    }

    public override void OnPlace()
    {
        base.OnPlace();
        Barracks[] barracks = FindObjectsOfType<Barracks>();
        if (barracks.Length >= 2)
        {
            Barracks other = (barracks[0] == this) ? barracks[1] : barracks[0];
            SetFoodAllocation(other.foodAllocation);
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateHealAndTrainRate();
    }

    protected override void Awake()
    {
        base.Awake();
        superMan = SuperManager.GetInstance();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Barracks];
        soldiers = new List<Soldier>();
        soldierPrefab = Resources.Load("Soldier") as GameObject;
        maxHealth = 200f;
        if (superMan.GetResearchComplete(SuperManager.k_iBarracksFortification))
        {
            maxHealth *= 1.5f;
        }
        health = maxHealth;
        if (superMan.GetResearchComplete(SuperManager.k_iBallistaSuper))
        {
            maxSoldiers = 8;
        }
        puffEffect = Resources.Load("EnemyPuffEffect") as GameObject;
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
        Soldier newSoldier = Instantiate(soldierPrefab).GetComponent<Soldier>();
        newSoldier.home = this;
        float vectorSampler = Random.Range(0f, 1f);
        newSoldier.transform.position = transform.position + (transform.right * vectorSampler) - (transform.forward * (1f - vectorSampler));
        newSoldier.puffEffect = puffEffect;

        if (superMan.GetResearchComplete(SuperManager.k_iBarracksSoldierDamage))
        {
            newSoldier.damage *= 1.3f;
        }
        if (superMan.GetResearchComplete(SuperManager.k_iBarracksSoldierHealth))
        {
            newSoldier.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
            newSoldier.maxHealth *= 1.5f;
            newSoldier.health *= 1.5f;
        }
        else
        {
            newSoldier.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        if (superMan.GetResearchComplete(SuperManager.k_iBarracksSoldierSpeed))
        {
            newSoldier.movementSpeed *= 1.3f;
        }

        soldiers.Add(newSoldier);

        GameManager.CreateAudioEffect("ResourceLoss", newSoldier.transform.position, 0.3f);
    }
}
