﻿using System.Collections.Generic;
using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : HeavyInvader.cs
// Description  : Inherited class of Enemy
// Author       : Tjeu Vreeburg, Samuel Fortune
// Mail         : tjeu.vreeburg@gmail.com

public class HeavyInvader : Enemy
{
    private const float BaseHealth = 105f;
    private const float BaseDamage = 10f;
    private bool[] equipment = new bool[4];

    protected override void Awake()
    {
        base.Awake();
        enemyName = EnemyNames.HeavyInvader;
        structureTypes = new List<StructureType>()
        {
            StructureType.Storage,
            StructureType.Longhaus,
            StructureType.Defense
        };

        GameObject healthBarInst = Instantiate(StructureManager.HealthBarPrefab, StructureManager.GetInstance().canvas.transform.Find("HUD/BuildingHealthbars"));
        healthbar = healthBarInst.GetComponent<Healthbar>();
        healthbar.target = gameObject;
        healthbar.fillAmount = 1f;
        healthBarInst.SetActive(false);
    }

    protected override void LookAtPosition(Vector3 _position)
    {
        _position.y = transform.position.y;
        base.LookAtPosition(_position);
        // fixing animation problems
        transform.right = -transform.forward;
    }

    private void FixedUpdate()
    {
        if (stunned) return;
        walkHeight = 0.5f;
        if (signature.startTile)
        {
            Structure attached = signature.startTile.GetAttached();
            if (attached)
            {
                if (attached.GetStructureName() == StructureNames.MetalEnvironment)
                {
                    if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Structure")))
                    {
                        if (hit.transform.name.Contains("Hill"))
                        {
                            walkHeight = hit.point.y;
                        }
                    }
                }
            }
        }
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
                            UpdateEquipment();
                        }
                        else
                        {
                            if (needToMoveAway)
                            {
                                if ((target.transform.position - transform.position).magnitude < 0.5f)
                                {
                                    Vector3 newPosition = transform.position - (GetAvoidingMotionVector() * Time.fixedDeltaTime);
                                    newPosition.y = walkHeight;
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
                            newPosition.y = walkHeight;
                            LookAtPosition(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            hasPath = false;

                            // get the motion vector for this frame
                            Vector3 newPosition = transform.position + (GetAvoidingMotionVector() * Time.fixedDeltaTime);
                            newPosition.y = walkHeight;
                            LookAtPosition(newPosition);
                            transform.position = newPosition;

                            // if we are close enough to the target, attack the target
                            if ((target.transform.position - transform.position).magnitude <= 0.6f)
                            {
                                animator.SetBool("Attack", true);
                                LookAtPosition(target.transform.position);
                                enemyState = EnemyState.Action;
                                needToMoveAway = (target.transform.position - transform.position).magnitude < 0.5f;
                                if (needToMoveAway) { animator.SetBool("Attack", false); }
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

    public bool[] GetEquipment()
    {
        return equipment;
    }

    public void SetEquipment(bool[] _equipment)
    {
        equipment = new bool[4]
        {
            _equipment[0],
            _equipment[1],
            _equipment[2],
            _equipment[3]
        };
    }

    public void Randomize()
    {
        equipment[0] = Random.Range(0f, 1f) > 0.5f;
        equipment[1] = Random.Range(0f, 1f) > 0.5f;
        equipment[2] = Random.Range(0f, 1f) > 0.5f;
        equipment[3] = Random.Range(0f, 1f) > 0.5f;
    }

    private void UpdateEquipment()
    {
        Transform lowPoly = transform.GetChild(1);
        if (equipment[0]) // if sword
        {
            baseDamage = BaseDamage;
            animator.SetFloat("AttackSpeed", 1.2f);
            // disable axe
            lowPoly.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else // !sword means axe
        {
            baseDamage = BaseDamage * 1.2f;
            animator.SetFloat("AttackSpeed", 1.0f);
            // disable sword
            lowPoly.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        lowPoly.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = equipment[1];
        lowPoly.GetChild(3).GetComponent<SkinnedMeshRenderer>().enabled = equipment[2];
        lowPoly.GetChild(4).GetComponent<SkinnedMeshRenderer>().enabled = equipment[3];

        baseHealth = BaseHealth;
        finalSpeed = 0.35f;

        if (equipment[2]) { baseHealth += 20f; finalSpeed -= 0.035f; }
        if (equipment[3]) { baseHealth += 10f; finalSpeed -= 0.0175f; }

        currentSpeed = finalSpeed;
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(GameManager.GetPuffEffect());
        puff.transform.position = transform.position;
        puff.transform.localScale *= 3f;
    }

    public override void Action()
    {
        if (defending)
        {
            if (defenseTarget)
            {
                if (defenseTarget.GetHealth() > 0)
                {
                    LookAtPosition(defenseTarget.transform.position);
                    action = true;
                }
            }
            else
            {
                ForgetSoldier();
            }
        }
        else
        {
            if (target.GetHealth() > 0)
            {
                action = true;
                Vector3 lookPosition = target.transform.position;
                lookPosition.y = transform.position.y;
                LookAtPosition(lookPosition);
            }
            else
            {
                ForgetSoldier();
            }
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

    public void Initialize(int _level, bool[] equipment = null)
    {
        if (equipment == null)
        {
            Randomize();
        }
        else
        {
            SetEquipment(equipment);
        }
        UpdateEquipment();
        SetLevel(_level);
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
