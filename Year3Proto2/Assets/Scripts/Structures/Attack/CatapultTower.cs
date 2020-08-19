using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    public GameObject catapult;
    public float boulderDamage = 5f;
    public float boulderExplosionRadius = 0.25f;
    private float boulderSpeed = 1.0f;
    private float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    public float GetFirerate()
    {
        return fireRate;
    }

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Catapult];
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultFortification)) { health = maxHealth *= 1.5f; }
        maxHealth = 450f;
        health = maxHealth;
    }

    protected override void Start()
    {
        base.Start();
        SetFirerate();
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.CatapultRange)) 
        { 
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f; 
        }
        if (superMan.GetResearchComplete(SuperManager.CatapultRange)) 
        { 
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f; 
        }
        attackCost = new ResourceBundle(0, superMan.GetResearchComplete(SuperManager.CatapultEfficiency) ? 4 : 8, 0);
        if (superMan.GetResearchComplete(SuperManager.CatapultPower))
        {
            boulderDamage *= 1.3f;
        }
        if (superMan.GetResearchComplete(SuperManager.CatapultSuper)) 
        { 
            boulderExplosionRadius *= 1.5f; 
        }
    }

    protected override void Update()
    {
        base.Update();
        if (target && isPlaced)
        {
            Vector3 catapultPosition = catapult.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = catapultPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            catapult.transform.rotation = Quaternion.Slerp(catapult.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * 2.5f);
        }
    }

    public override void AllocateVillager()
    {
        base.AllocateVillager();
        SetFirerate();
    }

    public override void DeallocateVillager()
    {
        base.DeallocateVillager();
        SetFirerate();
    }

    public override void DeallocateAll()
    {
        base.DeallocateAll();
        SetFirerate();
    }

    public override void SetAllocated(int _allocated)
    {
        base.SetAllocated(_allocated);
        SetFirerate();
    }

    public override void Attack(GameObject target)
    {
        fireCooldown += Time.deltaTime;
        if (fireCooldown >= fireDelay)
        {
            if (GameManager.GetInstance().playerResources.AttemptPurchase(new ResourceBundle(0, 15, 0)))
            {
                Fire();
            }
        }
    }


    void Fire()
    {
        fireCooldown = 0;
        GameObject newBoulder = Instantiate(boulder, catapult.transform.position, Quaternion.identity, transform);
        BoulderBehaviour boulderBehaviour = newBoulder.GetComponent<BoulderBehaviour>();
        boulderBehaviour.target = target.transform.position;
        boulderBehaviour.damage = boulderDamage;
        boulderBehaviour.speed = boulderSpeed;
        boulderBehaviour.puffEffect = puffPrefab;
        boulderBehaviour.explosionRadius = boulderExplosionRadius;
        GameManager.CreateAudioEffect("catapultFire", transform.position);
    }

    public override void OnPlace()
    {
        base.OnPlace();
        CatapultTower[] catapultTowers = FindObjectsOfType<CatapultTower>();
        if (catapultTowers.Length >= 2)
        {
            CatapultTower other = (catapultTowers[0] == this) ? catapultTowers[1] : catapultTowers[0];
        }
    }

    void SetFirerate()
    {
        /*
        switch (allocatedVillagers)
        {
            case 0:
                fireRate = 0f;
                break;
            case 1:
                fireRate = 0.25f;
                break;
            case 2:
                fireRate = 1f / 3f;
                break;
            case 3:
                fireRate = 0.5f;
                break;
            case 4:
                fireRate = 1f / 1.5f;
                break;
            case 5:
                fireRate = 1.0f;
                break;
        }
        */
        fireRate = 1f / 3f;
        fireDelay = 1f / fireRate;
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();
        if (target)
        {
            resourceDelta -= attackCost * fireRate;
        }
        return resourceDelta;
    }
}
