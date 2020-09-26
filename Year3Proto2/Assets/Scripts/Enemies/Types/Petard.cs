using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Petard : Enemy
{
    private float explosionRadius = 0.35f;
    private bool barrelExploded = false;

    protected override void Awake()
    {
        base.Awake();
        enemyName = EnemyNames.Petard;
        structureTypes = new List<StructureType>()
        {
            StructureType.Storage,
            StructureType.Resource,
            StructureType.Longhaus,
            StructureType.Defense
        };
    }

    private void FixedUpdate()
    {
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
                        if (needToMoveAway)
                        {
                            if ((target.transform.position - transform.position).magnitude < 0.5f)
                            {
                                Vector3 newPosition = transform.position - (GetAvoidingMotionVector() * Time.fixedDeltaTime);
                                LookAtPosition(newPosition);
                                transform.position = newPosition;
                            }
                            else
                            {
                                needToMoveAway = false;
                                LookAtPosition(target.transform.position);
                            }
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
                    }
                    break;
                case EnemyState.Walk:
                    if (target)
                    {
                        updatePathTimer += Time.fixedDeltaTime;
                        if (updatePathTimer >= updatePathDelay)
                        {
                            if (RequestNewPath())
                            {
                                updatePathTimer = 0f;
                            }

                        }
                        // if the distance from the enemy to the target is greater than 1 unit (one tile), the enemy should follow a path to the target. If they don't have one, they should request a path.
                        // if the distance is less than 1 unit, go ahead as normal
                        float distanceToTarget = (transform.position - target.transform.position).magnitude;

                        if (distanceToTarget > 1f)
                        {
                            if (!hasPath)
                            {
                                enemyState = EnemyState.Idle;
                                break;
                            }
                            Vector3 newPosition = GetNextPositionPathFollow();
                            LookAtPosition(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            hasPath = false;

                            // get the motion vector for this frame
                            Vector3 newPosition = transform.position + (GetAvoidingMotionVector() * Time.fixedDeltaTime);
                            //Debug.DrawLine(transform.position, transform.position + GetMotionVector(), Color.green);
                            LookAtPosition(newPosition);
                            transform.position = newPosition;

                            // if we are close enough to the target, attack the target
                            if ((target.transform.position - transform.position).magnitude <= 0.6f)
                            {
                                LookAtPosition(target.transform.position);
                                enemyState = EnemyState.Action;
                                needToMoveAway = (target.transform.position - transform.position).magnitude < 0.5f;
                            }
                        }
                    }
                    else
                    {
                        enemyState = EnemyState.Idle;
                    }
                    break;
                case EnemyState.Idle:
                    RequestNewPath();
                    break;
            }
        }
        else
        {
            action = false;
        }
    }

    protected override void LookAtPosition(Vector3 _position)
    {
        base.LookAtPosition(_position);
        // fixing animation problems
        transform.forward = transform.right;
    }

    public override void Action()
    {
        SetOffBarrel();
    }

    public void SetOffBarrel()
    {
        if (!barrelExploded)
        {
            RaycastHit[] hitStructures = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("Structure"));
            GameObject explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
            explosion.transform.localScale *= 2f * explosionRadius;
            foreach (RaycastHit structureHit in hitStructures)
            {
                Structure structure = structureHit.transform.GetComponent<Structure>();
                if (structure)
                {
                    if (structure.GetStructureType() == StructureType.Environment)
                    {
                        continue;
                    }
                    float damageToThisStructure = damage * (transform.position - structure.transform.position).magnitude / explosionRadius;
                    float clamped = Mathf.Clamp(damageToThisStructure, damage * 0.3f, damage);
                    structure.Damage(clamped);
                }
            }
            GameManager.CreateAudioEffect("Explosion", transform.position, 0.6f);
            barrelExploded = true;
            Damage(health);
        }
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(PuffEffect);
        puff.transform.position = transform.position;
    }

    public void Initialize(int _level)
    {
        baseHealth = 15f;
        baseDamage = 100f;
        SetLevel(_level);
        finalSpeed = 0.4f;
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SwiftFootwork) ? 1.4f : 1.0f;
        currentSpeed = finalSpeed;
    }

    public override void SetLevel(int _level)
    {
        base.SetLevel(_level);
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            renderer.material = EnemyMaterials.Fetch(enemyName, level);
        }
    }
}
