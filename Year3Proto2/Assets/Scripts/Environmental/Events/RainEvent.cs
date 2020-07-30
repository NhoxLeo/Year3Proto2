using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainEvent : EnvironmentEvent
{
    [SerializeField] private Transform rainPrefab;
    private Transform rain;

    public override void Invoke()
    {
        throw new System.NotImplementedException();
    }
}
