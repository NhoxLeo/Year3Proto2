using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Barracks : DefenseStructure
{
    private GameObject soldierPrefab;
    private int maxSoldiers = 3;
    [HideInInspector]
    public List<Soldier> soldiers;
    private float trainTime = 20f;
    private float timeTrained = 0f;

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

    public override void OnPlace()
    {
        base.OnPlace();
        Barracks[] barracks = FindObjectsOfType<Barracks>();
        if (barracks.Length >= 2)
        {
            Barracks other = (barracks[0] == this) ? barracks[1] : barracks[0];
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateCapacity();
    }

    protected override void Awake()
    {
        base.Awake();
        SuperManager superMan = SuperManager.GetInstance();
        structureName = StructureNames.Barracks;
        soldiers = new List<Soldier>();
        soldierPrefab = Resources.Load("Soldier") as GameObject;
        maxHealth = 200f;
        if (superMan.GetResearchComplete(SuperManager.BarracksFortification))
        {
            maxHealth *= 1.5f;
        }
        health = maxHealth;
        if (superMan.GetResearchComplete(SuperManager.BarracksSuper))
        {
            trainTime = 10f;
            float soldierMaxHealth = 30f * (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth) ? 1.5f : 1.0f);
            SetHealRate(soldierMaxHealth / trainTime);
        }
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
        }
    }

    private void SpawnSoldier()
    {
        SuperManager superMan = SuperManager.GetInstance();
        Soldier newSoldier = Instantiate(soldierPrefab).GetComponent<Soldier>();
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

        GameManager.CreateAudioEffect("ResourceLoss", newSoldier.transform.position, 0.3f);
    }

    public void LoadSoldier(SuperManager.SoldierSaveData _saveData)
    {
        SuperManager superMan = SuperManager.GetInstance();
        Soldier newSoldier = Instantiate(soldierPrefab).GetComponent<Soldier>();
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
    public override void CheckResearch()
    {
        throw new System.NotImplementedException();
    }
}
