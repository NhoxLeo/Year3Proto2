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
                        if ((target.transform.position - transform.position).magnitude <= 0.6f)
                        {
                            animator.SetBool("Attack", true);
                            LookAtPosition(target.transform.position);
                            enemyState = EnemyState.ACTION;
                            needToMoveAway = (target.transform.position - transform.position).magnitude < 0.5f;
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
                defending = false;
                animator.SetBool("Attack", false);
                action = false;
                enemyState = EnemyState.IDLE;
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
                animator.SetBool("Attack", false);
                action = false;
                enemyState = EnemyState.IDLE;
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
                        defenseTarget = null;
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
