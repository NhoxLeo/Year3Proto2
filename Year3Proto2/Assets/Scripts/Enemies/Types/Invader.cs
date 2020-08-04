using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Invader : Enemy
{
    public float scale = 0.0f;

    private void Awake()
    {
        structureTypes = new List<StructureType>()
        {
            StructureType.Attack,
            StructureType.Resource,
            StructureType.Storage,
            StructureType.Longhaus,
            StructureType.Defense
        };
    }

    private void Start()
    {
        EnemyStart();
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
                        // if the distance from the enemy to the target is greater than 1 unit (one tile), the enemy should follow a path to the target. If they don't have one, they should request a path.
                        // if the distance is less than 1 unit, go ahead as normal
                        float distanceToTarget = (transform.position - target.transform.position).magnitude;

                        if (distanceToTarget > 1f)
                        {
                            // do we have a path?  If we don't have a path, get one.
                            if (!hasPath)
                            {
                                path = spawner.GetPath(transform.position, structureTypes);
                                if(path.target)
                                {
                                    hasPath = true;
                                }
                            }
                            // follow the path
                            // move towards the first element in the path, if you get within 0.25 units, delete the element from the path
                            Vector3 nextPathPoint = path.pathPoints[0];
                            nextPathPoint.y = transform.position.y;
                            float distanceToNextPathPoint = (transform.position - nextPathPoint).magnitude;
                            if (distanceToNextPathPoint < 0.25f)
                            {
                                // delete the first element in the path
                                path.pathPoints.RemoveAt(0);
                            }
                            Vector3 newPosition = transform.position + (GetPathVector() * Time.fixedDeltaTime);
                            LookAtPosition(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            hasPath = false;

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
