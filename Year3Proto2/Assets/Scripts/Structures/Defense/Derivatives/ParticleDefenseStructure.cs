using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleDefenseStructure : DefenseStructure
{
    [SerializeField] private Transform particles;
    [SerializeField] private float damage;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float delay = 5;

    private ParticleSystem particle;
    private List<ParticleCollisionEvent> collisions;

    private float time;

    protected override void Start()
    {
        base.Start();
        time = delay;
        collisions = new List<ParticleCollisionEvent>();
    }

    protected override void Update()
    {
        base.Update();

        if (attachedTile != null && enemies.Count > 0)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                Transform particle = Instantiate(particles, transform.position, particles.rotation);
                ParticleSystem particleSystem = particle.GetComponent<ParticleSystem>();
                if(particleSystem) this.particle = particleSystem;

                time = delay;
            }
        }
    }

    private void OnParticleCollision(GameObject _other)
    {
        int numCollisionEvents = particle.GetCollisionEvents(_other, collisions);
        int i = 0;

        while (i < numCollisionEvents)
        {
            if ((layerMask.value & (1 << _other.gameObject.layer)) != 0)
            {
                OnParticleHit(_other.transform);
            }

            i++;
        }
    }

    public abstract void OnParticleHit(Transform _target);
}
