using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalProjectile : Projectile
{
    protected Rigidbody body;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void Launch()
    {
        body.velocity = CalculateVelocity();
        Vector3 oppositeVelocity = -body.velocity;
        body.AddRelativeForce(oppositeVelocity);
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if ((layerMask.value & (1 << _collision.gameObject.layer)) != 0)
        {
            OnProjectileHit(_collision.gameObject, _collision.contacts[0].point);
        }
    }

    public abstract Vector3 CalculateVelocity();
}
