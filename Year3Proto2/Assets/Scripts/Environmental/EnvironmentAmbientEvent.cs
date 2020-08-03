using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentAmbientEvent : EnvironmentEvent
{
    [SerializeField] protected Transform ambientPrefab;
    protected Transform ambient;

    public Transform GetAmbient()
    {
        return ambient;
    }
}
