﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invader : Enemy
{
    public float scale = 0.0f;

    protected override void Awake()
    {
        base.Awake();
        enemyName = EnemyNames.Invader;
        structureTypes = new List<StructureType>()
        {
            StructureType.Resource,
            StructureType.Storage,
            StructureType.Longhaus,
            StructureType.Defense
        };
    }

    protected override void LookAtPosition(Vector3 _position)
    {
        base.LookAtPosition(_position);
        // fixing animation problems
        transform.right = transform.forward;
    }

    private void FixedUpdate()
    {
        if (stunned) return;
        if (!GlobalData.longhausDead)
        {
            switch (enemyState)
            {
                case EnemyState.Action:
                    if (defending)
                    {
                        Action();
                    }
                    else
                    {
                        if (!target)
                        {
                            animator.SetBool("Attack", false);
                            enemyState = EnemyState.Idle;
                        }
                        else
                        {
                            if (needToMoveAway)
                            {
                                if ((target.transform.position - transform.position).magnitude < (scale * 0.04f) + 0.5f)
                                {
                                    Vector3 newPosition = transform.position - (GetAvoidingMotionVector() * Time.fixedDeltaTime);
                                    LookAtPosition(newPosition);
                                    transform.position = newPosition;
                                }
                                else
                                {
                                    needToMoveAway = false;
                                    LookAtPosition(target.transform.position);
                                    animator.SetBool("Attack", true);
                                }
                            }
                            else
                            {
                                if (structureTypes.Contains(target.GetStructureType()))
                                {
                                    if (!animator.GetBool("Attack"))
                                    {
                                        animator.SetBool("Attack", true);
                                    }
                                    Action();
                                }
                                else
                                {
                                    animator.SetBool("Attack", false);
                                    enemyState = EnemyState.Idle;
                                }
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
                                animator.SetBool("Attack", false);
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
                            if ((target.transform.position - transform.position).magnitude <= (scale * 0.04f) + 0.5f)
                            {
                                animator.SetBool("Attack", true);
                                LookAtPosition(target.transform.position);
                                enemyState = EnemyState.Action;
                                needToMoveAway = (target.transform.position - transform.position).magnitude < (scale * 0.04f) + 0.45f;
                                if (needToMoveAway)
                                {
                                    animator.SetBool("Attack", false);
                                }
                            }
                        }
                    }
                    else
                    {
                        animator.SetBool("Attack", false);
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

    public void Initialize(int _level, float _scale)
    {
        SetScale(_scale);
        SetLevel(_level);
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SwiftFootwork) ? 1.4f : 1.0f;
        currentSpeed = finalSpeed;
    }

    public void SetScale(float _scale)
    {
        scale = _scale;
        transform.localScale *= _scale + 0.3f;
        baseDamage = _scale * 2.0f;
        baseHealth = _scale * 15f;
        finalSpeed = 0.4f + ((1f / _scale) / 10.0f);

        currentSpeed = finalSpeed;

        if (!animator) { animator = GetComponent<Animator>(); }

        animator.SetFloat("AttackSpeed", 1f / _scale);
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(PuffEffect);
        puff.transform.position = transform.position;
        puff.transform.localScale *= scale;
    }


    public override void Action()
    {
        if (defending)
        {
            bool defend = true;
            if (defenseTarget)
            { 
                if (defenseTarget.GetReturnHome() || defenseTarget.GetHealth() <= 0)
                {
                    defend = false;
                }
            }
            else
            {
                defend = false;
            }
            if (defend)
            {
                if (defenseTarget.GetHealth() > 0)
                {
                    LookAtPosition(defenseTarget.transform.position);
                    action = true;
                }
            }
            else { ForgetSoldier(); }
        }
        else
        {
            if (target.GetHealth() > 0) { action = true; }
            else { ForgetSoldier(); }
        }
        
    }

    public void SwingContact()
    {
        if (action)
        {
            if (defending)
            {
                if (defenseTarget)
                {
                    if (defenseTarget.ApplyDamage(damage))
                    {
                        ForgetSoldier();
                    }
                }
            }
            else
            {
                if (target)
                {
                    target.Damage(damage);
                }
            }
        }
    }

    public override void SetLevel(int _level)
    {
        base.SetLevel(_level);
        GetComponentInChildren<SkinnedMeshRenderer>().material = EnemyMaterials.Fetch(enemyName, level);
    }
}
