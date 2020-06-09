using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    public GameObject catapult;
    public float boulderDamage = 5f;
    public float boulderExplosionRadius = 0.25f;
    private float boulderSpeed = 1.0f;
    public float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;


    protected override void Awake()
    {
        base.Awake();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Catapult];
        maxHealth = 450f;
        health = maxHealth;
    }

    protected override void Start()
    {
        base.Start();
        SetFirerate();
        if (superMan.GetResearchComplete(SuperManager.k_iCatapultRange)) { GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f; }
        if (superMan.GetResearchComplete(SuperManager.k_iCatapultFortification)) { health = maxHealth *= 1.5f; }
        attackCost = new ResourceBundle(0, superMan.GetResearchComplete(SuperManager.k_iCatapultEfficiency) ? 8 : 16, 0);
        if (superMan.GetResearchComplete(SuperManager.k_iCatapultPower))
        {
            boulderDamage *= 1.3f;
        }
        if (superMan.GetResearchComplete(SuperManager.k_iCatapultSuper)) { boulderExplosionRadius *= 1.5f; }
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

    public override void Attack(GameObject target)
    {
        fireCooldown += Time.deltaTime;
        if (fireCooldown >= fireDelay)
        {
            if (gameMan.playerResources.AttemptPurchase(new ResourceBundle(0, 15, 0)))
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

    public override void IncreaseFoodAllocation()
    {
        base.IncreaseFoodAllocation();
        SetFirerate();
    }

    public override void DecreaseFoodAllocation()
    {
        base.DecreaseFoodAllocation();
        SetFirerate();
    }


    void SetFirerate()
    {
        switch (foodAllocation)
        {
            case 1:
                fireRate = 0.25f;
                break;
            case 2:
                fireRate = 1f / 3.5f;
                break;
            case 3:
                fireRate = 1f / 3f;
                break;
            case 4:
                fireRate = 1f / 2.5f;
                break;
            case 5:
                fireRate = 0.5f;
                break;
        }
        fireDelay = 1 / fireRate;
    }
}
