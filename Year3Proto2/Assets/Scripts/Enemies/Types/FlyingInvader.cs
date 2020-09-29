using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInvader : Enemy
{
    private const float BaseHealth = 45f;
    private const float BaseDamage = 100f;
    private float barrelDropDelay = 4.0f;
    private float barrelDropTimer = 0.0f;
    private static GameObject Barrel = null;
    protected override void Awake()
    {
        base.Awake();
        structureTypes = new List<StructureType>()
        {
            StructureType.Storage,
            StructureType.Longhaus,
            StructureType.Defense
        };
        enemyName = EnemyNames.FlyingInvader;
        if (!Barrel)
        {
            Barrel = Resources.Load("FlyingInvaderBarrel") as GameObject;
        }

        GameObject healthBarInst = Instantiate(StructureManager.HealthBarPrefab, StructureManager.GetInstance().canvas.transform.Find("HUD/BuildingHealthbars"));
        healthbar = healthBarInst.GetComponent<Healthbar>();
        healthbar.target = gameObject;
        healthbar.fillAmount = 1f;
        healthBarInst.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, 2f, transform.position.z);
        if (stunned) return;

        if (!GlobalData.longhausDead)
        {
            switch (enemyState)
            {
                case EnemyState.Action:
                    if (!target)
                    {
                        enemyState = EnemyState.Idle;
                    }
                    else
                    {
                        if (structureTypes.Contains(target.GetStructureType()))
                        {
                            Action();
                        }
                        else
                        {
                            enemyState = EnemyState.Idle;
                        }
                    }
                    break;
                case EnemyState.Walk:
                    if (target)
                    {
                        Vector3 toTarget = target.transform.position - transform.position;
                        toTarget.y = 0f;
                        // get the motion vector for this frame
                        Vector3 newPosition = transform.position + (toTarget.normalized * finalSpeed * Time.fixedDeltaTime);
                        //Debug.DrawLine(transform.position, transform.position + GetMotionVector(), Color.green);
                        LookAtPosition(newPosition);
                        transform.position = newPosition;

                        // if we are close enough to the target, attack the target
                        if (toTarget.magnitude <= 0.25f)
                        {
                            enemyState = EnemyState.Action;
                        }
                    }
                    else
                    {
                        enemyState = EnemyState.Idle;
                    }
                    break;
                case EnemyState.Idle:
                    // find the next enemy
                    FindTarget();
                    break;
            }
        }
    }
    public override void Action()
    {
        barrelDropTimer -= Time.fixedDeltaTime;
        if (barrelDropTimer <= 0)
        {
            barrelDropTimer = barrelDropDelay;
            FlyingInvaderBarrel barrel = Instantiate(Barrel, transform.position, Quaternion.identity).GetComponent<FlyingInvaderBarrel>();
            barrel.Initialize(damage);
        }
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(PuffEffect);
        puff.transform.position = transform.position;
        puff.transform.localScale *= 2f;
    }

    private void FindTarget()
    {
        // starting from _signature.startTile, find the closest valid structure
        List<Structure> validStructures = new List<Structure>();
        for (int i = 0; i < structureTypes.Count; i++)
        {
            Structure[] structures = { };
            switch (structureTypes[i])
            {
                case StructureType.Defense:
                    structures = FindObjectsOfType<DefenseStructure>();
                    break;
                case StructureType.Longhaus:
                    structures = FindObjectsOfType<Longhaus>();
                    break;
                case StructureType.Storage:
                    structures = FindObjectsOfType<StorageStructure>();
                    break;
                case StructureType.Resource:
                    structures = FindObjectsOfType<ResourceStructure>();
                    break;
                default:
                    break;
            }

            validStructures.AddRange(structures);
        }

        // now that we have all the structures that the enemy can attack, let's find the closest structure.
        if (validStructures.Count == 0)
        {
            Debug.LogError("An Enemy tried to pathfind, and found no structures.");
        }

        // stop enemies from pathfinding to a structure that hasn't been placed yet
        validStructures.RemoveAll(structure => !structure.isPlaced);

        Structure closest = validStructures[0];
        float closestDistance = (validStructures[0].transform.position - transform.position).magnitude;

        for (int i = 1; i < validStructures.Count; i++)
        {
            float distance = (validStructures[i].transform.position - transform.position).magnitude;
            if (distance < closestDistance)
            {
                closest = validStructures[i];
                closestDistance = distance;
            }
        }

        target = closest;
        enemyState = EnemyState.Walk;
    }

    public void Initialize(int _level)
    {
        baseHealth = BaseHealth;
        baseDamage = BaseDamage;
        SetLevel(_level);
        finalSpeed = 0.25f;
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SwiftFootwork) ? 1.4f : 1.0f;
        currentSpeed = finalSpeed;
    }

    public override void SetLevel(int _level)
    {
        base.SetLevel(_level);
        Material[] materials = transform.GetChild(1).GetComponent<MeshRenderer>().materials;
        materials[1] = EnemyMaterials.Fetch(enemyName, level);
        transform.GetChild(1).GetComponent<MeshRenderer>().materials = materials;
        transform.GetChild(1).GetComponent<MeshRenderer>().materials[1] = EnemyMaterials.Fetch(enemyName, level);
        transform.GetChild(2).GetComponent<MeshRenderer>().material = EnemyMaterials.Fetch(EnemyNames.Petard, level);
        transform.GetChild(3).GetComponent<MeshRenderer>().material = EnemyMaterials.Fetch(EnemyNames.Petard, level);
        transform.GetChild(4).GetComponent<MeshRenderer>().material = EnemyMaterials.Fetch(EnemyNames.Petard, level);
        transform.GetChild(5).GetComponent<MeshRenderer>().material = EnemyMaterials.Fetch(EnemyNames.Petard, level);
    }
}
