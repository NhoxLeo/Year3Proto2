﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : AttackStructure
{
    public GameObject boulder;
    public GameObject catapult;
    public float boulderDamage = 5f;
    public float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    private List<GameObject> spawnedBoulders = new List<GameObject>();

    private float speed = 0.8f;


    void Start()
    {
        AttackStart();
        structureName = "Catapult Tower";
        maxHealth = 450f;
        health = maxHealth;
        SetFirerate();
    }

    private void Update()
    {
        AttackUpdate();
        if (target)
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
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0)
        {
            GameManager game = FindObjectOfType<GameManager>();
            if (game.playerData.CanAfford(new ResourceBundle(3, 0, 0)))
            {
                Fire();
                game.AddBatch(new Batch(-3, ResourceType.food));
            }
        }
    }


    void Fire()
    {
        fireCooldown = fireDelay;
        GameObject newBoulder = Instantiate(boulder, catapult.transform.position, Quaternion.identity, transform);
        BoulderBehaviour boulderBehaviour = newBoulder.GetComponent<BoulderBehaviour>();
        spawnedBoulders.Add(newBoulder);
        boulderBehaviour.target = target.transform.position;
        GameManager.CreateAudioEffect("catapultFire", transform.position);
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
