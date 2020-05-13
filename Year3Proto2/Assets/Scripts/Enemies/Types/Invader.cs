using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Invader : Enemy
{
    private float cooldown = 0.0f;
    private void Start()
    {
        EnemyStart();

        structureTypes = new List<StructureType>(new[] { 
            StructureType.attack,
            StructureType.resource,
            StructureType.storage,
            StructureType.longhaus
        }); 
    }

    public override void Action(Structure structure, float damage)
    {
        if (structure.GetHealth() != 0)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0)
            {
                cooldown = finalSpeed / 0.6f;
                transform.LookAt(target.transform.position);
                if (structure.Damage(damage))
                {
                    action = false;
                    enemyState = EnemyState.IDLE;
                }
            }

            if (!action)
            {
                transform.DOKill(false);
                transform.DOMoveY(yPosition, 0.0f);

                transform.DOKill(false);
                transform.LookAt(target.transform.position);
                transform.DOMove(transform.position + (transform.forward * scale), finalSpeed / 3.0f).SetLoops(-1, LoopType.Yoyo);

                action = true;
            }
        }
        else
        {
            action = false;
            enemyState = EnemyState.IDLE;
        }
    }
}
