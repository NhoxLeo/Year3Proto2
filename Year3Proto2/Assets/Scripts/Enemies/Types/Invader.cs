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
            StructureType.longhaus
        }); 
    }

    public override void Action(Structure structure, float damage)
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = speed / 0.6f;
            structure.Damage(damage);
        }

        if(!action)
        {
            transform.DOKill(false);
            transform.DOMoveY(yPosition, 0.0f);

            transform.DOKill(false);
            transform.DOMove(transform.position + (transform.forward * 0.1f), speed / 2.0f).SetLoops(-1, LoopType.Yoyo);

            action = true;
        }
    }
}
