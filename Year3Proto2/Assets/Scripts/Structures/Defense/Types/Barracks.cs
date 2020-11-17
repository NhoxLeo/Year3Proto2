using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Barracks : DefenseStructure
{
    private static GameObject SoldierPrefab;
    private List<Soldier> soldiers = new List<Soldier>();
    private float trainTime = 6f;
    private float timeTrained = 0f;
    private const float BaseMaxHealth = 320f;
    private const float BaseSoldierDamage = 4f;
    private const float BaseSoldierHealth = 30f;
    private const float BaseSoldierMovementSpeed = 0.55f;
    private bool soldierDamageUpgrade = false;
    private bool soldierHealthUpgrade = false;
    private bool soldierSpeedUpgrade = false;
    private Color normalEmissiveColour;
    private List<Vector3> restLocations = new List<Vector3>();
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
        return allocatedVillagers;
    }

    protected override void Start()
    {
        base.Start();
        RefreshRestPositions();
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
            trainTime *= 0.5f;
        }
        soldierDamageUpgrade = superMan.GetResearchComplete(SuperManager.BarracksSoldierDamage);
        soldierHealthUpgrade = superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth);
        soldierSpeedUpgrade = superMan.GetResearchComplete(SuperManager.BarracksSoldierSpeed);

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader); 
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);

        // soldier stuff
        if (!SoldierPrefab)
        {
            SoldierPrefab = Resources.Load("Soldier") as GameObject;
        }
        normalEmissiveColour = meshRenderer.materials[0].GetColor("_EmissiveColor");
    }

    protected override void Update()
    {
        base.Update();
        if (isPlaced)
        {
            soldiers.RemoveAll(soldier => !soldier);
            if (soldiers.Count < allocatedVillagers)
            {
                timeTrained += Time.deltaTime;
                if (timeTrained >= trainTime)
                {
                    timeTrained = 0f;
                    SpawnSoldier();
                }
            }
        }
    }

    private Soldier NewSoldierShared(float _health)
    {
        SuperManager superMan = SuperManager.GetInstance();
        Soldier newSoldier = Instantiate(SoldierPrefab).GetComponent<Soldier>();
        newSoldier.SetHome(this);
        newSoldier.SetBarracksID(ID);
        newSoldier.SetHealth(_health);
        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth))
        {
            newSoldier.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else
        {
            newSoldier.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        soldiers.Add(newSoldier);
        return newSoldier;
    }

    private void SpawnSoldier()
    {
        Soldier newSoldier = NewSoldierShared(GetSoldierMaxHealth());
        newSoldier.transform.position = transform.position + (transform.right * Random.Range(-0.1f, 0.1f)) + (transform.forward * Random.Range(-0.1f, 0.1f));
    }

    public void LoadSoldier(SuperManager.SoldierSaveData _saveData)
    {
        Soldier newSoldier = NewSoldierShared(_saveData.health);
        newSoldier.transform.position = _saveData.position;
        newSoldier.transform.rotation = _saveData.orientation;
        newSoldier.SetState(_saveData.state);
        newSoldier.SetReturnHome(_saveData.returnHome);
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
        soldiers.RemoveAll(soldier => !soldier);
        foreach (Soldier soldier in soldiers)
        {
            soldier.OnSetLevel();
        }
        float oldMaxHealth = GetTrueMaxHealth() / SuperManager.ScalingFactor;
        float difference = GetTrueMaxHealth() - oldMaxHealth;
        health += difference;
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

    public override void SetColour(Color _colour)
    {
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        else
        {
            meshRenderer.materials[0].SetColor("_EmissiveColor", _colour);
            if (_colour == Color.white)
            {
                meshRenderer.materials[0].SetColor("_EmissiveColor", normalEmissiveColour);
            }
        }
        meshRenderer.materials[0].SetColor(colourReference, _colour);
        meshRenderer.materials[1].SetColor(colourReference, _colour);
    }

    public override void OnAllocation()
    {
        base.OnAllocation();
        soldiers.RemoveAll(soldier => !soldier);
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (i >= allocatedVillagers)
            {
                soldiers[i].VillagerDeallocated();
            }
            else
            {
                soldiers[i].SetReturnHome(false);
            }
        }
    }

    public void OnSoldierDeath(Soldier _soldier)
    {
        soldiers.Remove(_soldier);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        SetMaterials(SuperManager.GetInstance().GetSnow());
        RefreshRestPositions();
    }

    public float GetSoldierMaxHealth()
    {
        return BaseSoldierHealth * (soldierHealthUpgrade ? 1.5f : 1f) * Mathf.Pow(SuperManager.ScalingFactor, level - 1);
    }

    public float GetSoldierDamage()
    {
        return BaseSoldierDamage * (soldierDamageUpgrade ? 1.3f : 1f) * Mathf.Pow(SuperManager.ScalingFactor, level - 1);
    }

    public float GetSoldierMovementSpeed()
    {
        return BaseSoldierMovementSpeed * (soldierSpeedUpgrade ? 1.3f : 1f);
    }

    public float GetSoldierHealRate()
    {
        return GetSoldierMaxHealth() / trainTime * 1.5f;
    }

    public void AlertSoldiers()
    {
        foreach (Soldier soldier in soldiers)
        {
            soldier.TryFindNewTarget();
        }
    }

    public Vector3 GetRestLocation(Soldier _soldier)
    {
        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] == _soldier)
            {
                if (restLocations.Count >= i + 1)
                {
                    return restLocations[i];
                }
            }
        }
        return transform.position;
    }

    public void UpdateSoldierRestLocations()
    {
        for (int i = 0; i < soldiers.Count; i++)
        {
            soldiers[i].SetRestLocation(restLocations[i]);
        }
    }

    public void RefreshRestPositions()
    {
        restLocations.Clear();
        restLocations.Add(transform.GetChild(2).position);
        restLocations.Add(transform.GetChild(3).position);
        restLocations.Add(transform.GetChild(4).position);
    }
}
