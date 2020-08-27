using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleProjectile : Projectile
{
    [SerializeField] private Transform particlePrefab;
    [SerializeField] private Transform particleParent;

    public abstract void OnParticleHit();
}
