using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invader : Enemy
{
    public float scale = 0.0f;

    private void Start()
    {
        EnemyStart();

        structureTypes = new List<StructureType>()
        {
            StructureType.attack,
            StructureType.resource,
            StructureType.storage,
            StructureType.longhaus,
            StructureType.defense
        };
    }

    private void FixedUpdate()
    {
        if (!GlobalData.longhausDead)
        {
            switch (enemyState)
            {
                case EnemyState.ACTION:
                    if (defending)
                    {
                        Action();
                    }
                    else
                    {
                        if (!target)
                        {
                            animator.SetBool("Attack", false);
                            enemyState = EnemyState.IDLE;
                        }
                        else
                        {
                            if (needToMoveAway)
                            {
                                if ((target.transform.position - transform.position).magnitude < (scale * 0.04f) + 0.5f)
                                {
                                    Vector3 newPosition = transform.position - (GetMotionVector() * Time.fixedDeltaTime);
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
                                    Action();
                                }
                                else
                                {
                                    animator.SetBool("Attack", false);
                                    enemyState = EnemyState.IDLE;
                                }
                            }
                        }
                    }
                    break;
                case EnemyState.WALK:
                    if (target)
                    {
                        if (!target.attachedTile)
                        {
                            if (!Next()) { target = null; }
                        }

                        // get the motion vector for this frame
                        Vector3 newPosition = transform.position + (GetMotionVector() * Time.fixedDeltaTime);
                        //Debug.DrawLine(transform.position, transform.position + GetMotionVector(), Color.green);
                        LookAtPosition(newPosition);
                        transform.position = newPosition;

                        // if we are close enough to the target, attack the target
                        if ((target.transform.position - transform.position).magnitude <= (scale * 0.04f) + 0.5f)
                        {
                            animator.SetBool("Attack", true);
                            LookAtPosition(target.transform.position);
                            enemyState = EnemyState.ACTION;
                            needToMoveAway = (target.transform.position - transform.position).magnitude < (scale * 0.04f) + 0.45f;
                            if (needToMoveAway) { animator.SetBool("Attack", false); }
                        }

                    }
                    else
                    {
                        animator.SetBool("Attack", false);
                        enemyState = EnemyState.IDLE;
                    }
                    break;
                case EnemyState.IDLE:
                    if (!Next()) { target = null; }
                    if (!target) { Destroy(gameObject); }
                    break;
            }
        }
        else
        {
            action = false;
        }
    }

    public void SetScale(float _scale)
    {
        scale = _scale;
        transform.localScale *= _scale;
        damage = _scale * 2.0f;
        health = _scale * 7.5f;
        finalSpeed = 0.4f + (1f / _scale) / 10.0f;
        if (!animator) { animator = GetComponent<Animator>(); }
        animator.SetFloat("AttackSpeed", 1f / _scale);
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(puffEffect);
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
                if (defenseTarget.returnHome || defenseTarget.health <= 0)
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
                if (defenseTarget.health > 0)
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
                    if (defenseTarget.Damage(damage))
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
}
