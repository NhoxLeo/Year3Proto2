using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raider : Enemy
{
    private float cooldown = 0.0f;

    private void Start()
    {
        EnemyStart();

        structureTypes = new List<StructureType>(new[] {
            StructureType.resource,
        });
    }
    public override void Action(Structure structure, float damage)
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0)
        {
            cooldown = finalSpeed / 0.6f;
            structure.
        }

        if (!action)
        {
            transform.DOKill(false);
            transform.DOMoveY(yPosition, 0.0f);

            transform.DOKill(false);
            transform.DOMove(transform.position + (transform.forward * scale), finalSpeed / 3.0f).SetLoops(-1, LoopType.Yoyo);

            action = true;
        }
    }
}
