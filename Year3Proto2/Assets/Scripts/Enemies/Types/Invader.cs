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

    public override void Action(Structure _structure, float _damage)
    {
        if (_structure.GetHealth() > 0)
        {
            animator.SetBool("Attack", true);
            cooldown -= Time.deltaTime;
            if (cooldown <= 0)
            {
                LookAtPosition(_structure.transform.position);
                cooldown = finalSpeed / 0.6f;
                if (_structure.Damage(_damage))
                {
                    enemyState = EnemyState.IDLE;
                }
            }
        }
        else
        {
            animator.SetBool("Attack", false);
            enemyState = EnemyState.IDLE;
        }
    }
}
