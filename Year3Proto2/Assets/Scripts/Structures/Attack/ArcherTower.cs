using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTower : AttackStructure
{
    public GameObject arrow;
    public GameObject ballista;
    public float arrowDamage = 5f;
    public float fireRate = 2f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    private List<GameObject> spawnedArrows = new List<GameObject>();
    private const float arrowSpeed = 2.5f;

    void Start()
    {
        AttackStart();
        maxHealth = 300f;
        structureName = "Archer Tower";
        fireDelay = 1 / fireRate;
    }

    private void Update()
    {
        AttackUpdate();

        if (target != null)
        {
            Vector3 ballistaPosition = ballista.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = ballistaPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            ballista.transform.rotation = Quaternion.Slerp(ballista.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * arrowSpeed);

        }
    }

    public override void Attack(GameObject target)
    {
        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0) { Fire(); }
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
        fireCooldown = fireDelay;
        GameObject newArrow = Instantiate(arrow, ballista.transform.position, Quaternion.identity, transform);
        ArrowBehaviour arrowBehaviour = newArrow.GetComponent<ArrowBehaviour>();
        spawnedArrows.Add(newArrow);
        arrowBehaviour.target = target.transform;
        arrowBehaviour.damage = arrowDamage;
        arrowBehaviour.speed = arrowSpeed;
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
