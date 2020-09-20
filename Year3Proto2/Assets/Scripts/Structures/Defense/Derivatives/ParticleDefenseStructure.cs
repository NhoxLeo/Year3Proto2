using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleDefenseStructure : DefenseStructure
{
    [SerializeField] private float damage;
    [SerializeField] private Transform particles;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float delay = 5;

    private float time;
    protected override void Update()
    {
        base.Update();

        time -= Time.deltaTime;
        if(time <= 0)
        {
            Instantiate(particles, transform);
            time = delay;
        }
    }

    private void OnParticleCollision(GameObject _other)
    {
        ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
        if (particleSystem)
        {
            for (int i = 0; i < particleSystem.GetCollisionEvents(_other, new List<ParticleCollisionEvent>()); i++)
            {
                if ((layerMask.value & (1 << _other.gameObject.layer)) != 0)
                {
                    OnParticleHit(_other.transform);
                }
            }
        }
    }

    public abstract void OnParticleHit(Transform _target);
}
