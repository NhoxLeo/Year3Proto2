using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulderPrefab;
    public GameObject catapult;
    public float boulderDamage = 5f;
    public float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    private List<GameObject> spawnedBoulders = new List<GameObject>();

    private float speed = 0.8f;


    protected override void Awake()
    {
        base.Awake();
        structureName = "Catapult Tower";
        maxHealth = 450f;
        health = maxHealth;
    }

    protected override void Start()
    {
        base.Start();
        SetFirerate();
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
