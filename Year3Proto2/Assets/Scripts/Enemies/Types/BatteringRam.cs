using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteringRam : Enemy
{
    private const float delay = 2f;
    private float timer = delay;

    protected override void Awake()
    {
        base.Awake();
        enemyName = EnemyNames.BatteringRam;
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

    public override void Action()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            timer = delay;
            target.Damage(damage);
        }
    }

    protected override void LookAtPosition(Vector3 _position)
    {
        base.LookAtPosition(_position);
        // fixing animation problems
        transform.right = transform.forward;
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(PuffEffect);
        puff.transform.localScale *= 4f;
        puff.transform.position = transform.position;
    }

    public void Initialize(int _level)
    {
        baseHealth = 100f;
        baseDamage = 50f;
        SetLevel(_level);
        finalSpeed = 0.25f;
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SwiftFootwork) ? 1.4f : 1.0f;
        currentSpeed = finalSpeed;
    }
}
