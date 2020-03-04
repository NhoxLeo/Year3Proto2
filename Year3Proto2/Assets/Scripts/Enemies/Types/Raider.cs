using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raider : Enemy
{

    private void Start()
    {
        Walk();
        Next();
        Debug.Log(GetTarget());
    }
    public override void Action(GameObject gameObject)
    {
        if (gameObject == null) Next();
    }
}
