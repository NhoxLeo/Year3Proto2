using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleDefenseStructure : DefenseStructure
{
    [Header("Particle")]
    [SerializeField] protected Transform particlePrefab;
    [SerializeField] protected float particleAmount;
    [SerializeField] private float particleTime;
    [SerializeField] private float particleDelay;
    [SerializeField] private float particleRate;
    [SerializeField] private float particleRateFactor;

    protected override void Update()
    {
        
    }

    public float GetFireRate()
    {
        return particleRate;
    }

}
