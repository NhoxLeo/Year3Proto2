using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyInvader : Enemy
{
    private bool[] equipment = new bool[4];

    private void Start()
    {
        EnemyStart();
        UpdateEquipment();

        structureTypes = new List<StructureType>()
        {
            StructureType.attack,
            StructureType.storage,
            StructureType.longhaus,
            StructureType.defense
        };
    }

    protected override void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.right = -transform.forward;
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
                                if ((target.transform.position - transform.position).magnitude < 0.5f)
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
                            if ((target.transform.position - transform.position).magnitude <= 0.6f)
                            {
                                animator.SetBool("Attack", true);
                                LookAtPosition(target.transform.position);
                                enemyState = EnemyState.ACTION;
                                needToMoveAway = (target.transform.position - transform.position).magnitude < 0.5f;
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
            damage = 10f;
            animator.SetFloat("AttackSpeed", 1.2f);
            // disable axe
            lowPoly.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else // !sword means axe
        {
            damage = 12f;
            animator.SetFloat("AttackSpeed", 1.0f);
            // disable sword
            lowPoly.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        lowPoly.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = equipment[1];
        lowPoly.GetChild(3).GetComponent<SkinnedMeshRenderer>().enabled = equipment[2];
        lowPoly.GetChild(4).GetComponent<SkinnedMeshRenderer>().enabled = equipment[3];
        health = 65f;
        finalSpeed = 0.25f;
        if (equipment[2]) { health += 10f; finalSpeed -= 0.03f; }
        if (equipment[3]) { health += 5f; finalSpeed -= 0.015f; }
    }

    public override void OnKill()
    {
        base.OnKill();
        GameObject puff = Instantiate(puffEffect);
        puff.transform.position = transform.position;
        puff.transform.localScale *= 3f;
    }

    public override void Action()
    {
        if (defending)
        {
            if (defenseTarget)
            {
                if (defenseTarget.health > 0)
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
