using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTower : AttackStructure
{
    public const int k_CostArrowBase = 6;
    public GameObject arrow;
    public GameObject ballista;
    public float arrowDamage = 5f;
    private float arrowSpeed = 2.5f;
    public float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    protected override void Awake()
    {
        base.Awake();
        maxHealth = 350f;
        health = maxHealth;
        structureName = "Archer Tower";
    }

    protected override void Start()
    {
        base.Start();
        SetFirerate();
        if (superMan.GetResearchComplete(SuperManager.k_iBallistaRange)) { GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f; }
        if (superMan.GetResearchComplete(SuperManager.k_iBallistaFortification)) { health = maxHealth *= 1.5f; }
        bool efficiencyUpgrade = superMan.GetResearchComplete(SuperManager.k_iBallistaEfficiency);
        int woodCost = efficiencyUpgrade ? (k_CostArrowBase / 2) : k_CostArrowBase;
        attackCost = new ResourceBundle(woodCost, 0, 0);
        if (superMan.GetResearchComplete(SuperManager.k_iBallistaPower))
        {
            arrowDamage *= 1.3f;
            arrowSpeed *= 1.3f;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (target && isPlaced)
        {
            Vector3 ballistaPosition = ballista.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = ballistaPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            ballista.transform.rotation = Quaternion.Slerp(ballista.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * 2.5f);

        }
    }

    public override void Attack(GameObject target)
    {
        fireCooldown += Time.deltaTime;
        if (fireCooldown >= fireDelay)
        {
            if (gameMan.playerResources.AttemptPurchase(attackCost))
            {
                Fire();
            }
        }
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

    void Fire()
    {
        fireCooldown = 0;
        GameObject newArrow = Instantiate(arrow, ballista.transform.position, Quaternion.identity, transform);
        ArrowBehaviour arrowBehaviour = newArrow.GetComponent<ArrowBehaviour>();
        arrowBehaviour.target = target.transform;
        arrowBehaviour.damage = arrowDamage;
        arrowBehaviour.speed = arrowSpeed;
        arrowBehaviour.puffEffect = puffPrefab;
        GameManager.CreateAudioEffect("arrow", transform.position);
    }

    void SetFirerate()
    {
        switch (foodAllocation)
        {
            case 1:
                fireRate = 0.5f;
                break;
            case 2:
                fireRate = 2f / 3f;
                break;
            case 3:
                fireRate = 1f;
                break;
            case 4:
                fireRate = 1.5f;
                break;
            case 5:
                fireRate = 2f;
                break;
        }
        fireDelay = 1 / fireRate;
    }
}
