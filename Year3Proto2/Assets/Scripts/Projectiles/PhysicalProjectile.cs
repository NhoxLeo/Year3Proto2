using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalProjectile : Projectile
{
    private const float Ground = 0.51f;

    protected Vector3 endPosition;
    protected bool reachedDestination = false;

    [Header("Physical Attributes")]
    [SerializeField] private float distanceOffset = 0.5f;
    [SerializeField] private bool ground = false;
    [SerializeField] private LayerMask layerMask;

    private void Update()
    {
        Vector3 heading = (target ? (endPosition != null ? endPosition : target.position) : Destination) - transform.position; 

        Vector3 direction = heading.normalized;
        float distance = heading.magnitude;

        OnDisplacement(heading, direction, distance);

        if(transform.position.y <= Ground) OnGroundHit();
    }

    public override void SetTarget(Transform _target)
    {
        base.SetTarget(_target);
        endPosition = _target.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!reachedDestination)
        {
            if((layerMask.value & (1 << other.gameObject.layer)) != 0)
            {
                OnDestination(other.transform.position);
                Destroy(gameObject);
            }
        }
    }

    protected abstract void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance);
    protected abstract void OnDestination(Vector3 _location);
    protected abstract void OnGroundHit();
}
