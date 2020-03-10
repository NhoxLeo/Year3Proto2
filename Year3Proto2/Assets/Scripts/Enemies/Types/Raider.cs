using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raider : Enemy
{
    private void Start()
    {
        EnemyStart();
    }
    public override void Action(Structure structure, float damage)
    {
        throw new System.NotImplementedException();
    }
}
