using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalProjectile : Projectile
{
    private const float Ground = 0.51f;

    [SerializeField] private float distanceOffset = 0.5f;
    [SerializeField] private bool ground = false;
    protected abstract void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance);
    protected abstract void OnDestination(Vector3 _location);

    private void Update()
    {
        Destination = target ? target.position : Destination;
        Vector3 position = Destination;

        Vector3 heading = position - transform.position; 

        Vector3 direction = heading.normalized;
        float distance = heading.magnitude;

        OnDisplacement(heading, direction, distance);

        if (transform.position.y <= Ground && ground) OnDestination(transform.position);
        if (heading.sqrMagnitude < distanceOffset * distanceOffset && !ground) OnDestination(transform.position);
    }
}
