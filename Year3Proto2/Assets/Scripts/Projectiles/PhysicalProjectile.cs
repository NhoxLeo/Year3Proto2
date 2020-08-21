using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalProjectile : Projectile
{
    [SerializeField] private Rigidbody body;
    public override void Launch()
    {
        body.velocity = CalculateVelocity();
        Vector3 oppositeVelocity = -body.velocity;
        body.AddRelativeForce(oppositeVelocity);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if ((layerMask.value & (1 << _other.gameObject.layer)) != 0)
        {
            OnProjectileHit(_other.transform, _other.bounds.center);
        }
    }

    public abstract Vector3 CalculateVelocity();
}
