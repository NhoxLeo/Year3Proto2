using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Barracks : DefenseStructure
{
    private static GameObject SoldierPrefab;
    private int maxSoldiers = 3;
    [HideInInspector]
    public List<Soldier> soldiers;
    private float trainTime = 20f;
    private float timeTrained = 0f;

    private const float BaseMaxHealth = 350f;
    private const float BaseDamage = 3f;

    public float GetTimeTrained()
    {
        return timeTrained;
    }

    public void SetTimeTrained(float _timeTrained)
    {
        timeTrained = _timeTrained;
    }

    public float GetTroopCapacity()
    {
        return maxSoldiers;
    }

    private void UpdateCapacity()
    {
        //maxSoldiers = allocatedVillagers;
        maxSoldiers = 3;
        // recall excess soldiers
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (i >= maxSoldiers)
            {
                soldiers[i].SetReturnHome(true);
            }
            else
            {
                soldiers[i].SetReturnHome(false);
            }
        }
    }

    private void SetHealRate(float _healRate)
    {
        foreach (Soldier soldier in soldiers)
        {
            soldier.SetHealRate(_healRate);
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateCapacity();
    }

    protected override void Awake()
    {
        // set base stats
        base.Awake();
        structureName = StructureNames.Barracks;

        // research
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.BarracksSuper))
        {
            trainTime = 10f;
            float soldierMaxHealth = 30f * (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth) ? 1.5f : 1.0f);
            SetHealRate(soldierMaxHealth / trainTime);
        }

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);

        // soldier stuff
        if (!SoldierPrefab)
        {
            SoldierPrefab = Resources.Load("Soldier") as GameObject;
        }
        soldiers = new List<Soldier>();
    }

    protected override void Update()
    {
        if (attachedTile != null)
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

            soldiers.RemoveAll(soldier => !soldier);
        }
    }

    private void SpawnSoldier()
    {
        SuperManager superMan = SuperManager.GetInstance();
        Soldier newSoldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
        newSoldier.SetBarracksID(ID);
        newSoldier.SetHome(this);
        //float vectorSampler = Random.Range(0f, 1f);
        //newSoldier.transform.position = transform.position + (transform.right * vectorSampler) - (transform.forward * (1f - vectorSampler));
        newSoldier.transform.position = transform.position + (transform.right * Random.Range(-0.1f, 0.1f)) + (transform.forward * Random.Range(-0.1f, 0.1f));

        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth))
        {
            newSoldier.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else
        {
            newSoldier.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        soldiers.Add(newSoldier);

        GameManager.CreateAudioEffect("ResourceLoss", newSoldier.transform.position, SoundType.SoundEffect, 0.6f);
    }

    public void LoadSoldier(SuperManager.SoldierSaveData _saveData)
    {
        SuperManager superMan = SuperManager.GetInstance();
        Soldier newSoldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
        newSoldier.SetBarracksID(ID);
        newSoldier.SetHome(this);
        newSoldier.transform.position = _saveData.position;
        newSoldier.transform.rotation = _saveData.orientation;
        newSoldier.SetState(_saveData.state);
        newSoldier.SetHealth(_saveData.health);
        newSoldier.SetReturnHome(_saveData.returnHome);

        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth))
        {
            newSoldier.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else
        {
            newSoldier.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }

        soldiers.Add(newSoldier);
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
        Soldier.SetDamage(GetBaseDamage() * Mathf.Pow(SuperManager.ScalingFactor, level - 1));
        health = GetTrueMaxHealth();
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // fortification upgrade
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.BarracksFortification))
        {
            maxHealth *= 1.5f;
        }

        // level
        maxHealth *= Mathf.Pow(SuperManager.ScalingFactor, level - 1);

        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    private float GetBaseDamage()
    {
        return BaseDamage * (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f);
    }
}
