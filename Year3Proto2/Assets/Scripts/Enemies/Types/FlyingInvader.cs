using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInvader : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
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
                        Vector3 newPosition = transform.position + (toTarget * Time.fixedDeltaTime);
                        //Debug.DrawLine(transform.position, transform.position + GetMotionVector(), Color.green);
                        LookAtPosition(newPosition);
                        transform.position = newPosition;

                        // if we are close enough to the target, attack the target
                        if ((target.transform.position - transform.position).magnitude <= 2.5f)
                        {
                            LookAtPosition(target.transform.position);
                            enemyState = EnemyState.Action;
                        }
                    }
                    else
                    {
                        enemyState = EnemyState.Idle;
                    }
                    break;
                case EnemyState.Idle:
                    break;
            }
        }
    }
    public override void Action()
    {
        if (target.GetHealth() > 0)
        {
            action = true;
        }
    }
}
