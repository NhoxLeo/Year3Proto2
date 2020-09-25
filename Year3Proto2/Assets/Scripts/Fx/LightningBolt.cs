using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    public Transform target;
    [SerializeField] private ParticleSystem lightningBolt;
    [SerializeField] private ParticleSystem sparks;
    private float damage;
    private bool sparkDamage;

    private List<ParticleCollisionEvent> collisions;

    public void Initialize(Transform _target, float _damage, bool _sparkDamage)
    {
        target = _target;
        damage = _damage;
        sparkDamage = _sparkDamage;

        collisions = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject _other)
    {
        if (sparkDamage)
        {
            if ((sparks.gameObject.layer & (1 << _other.gameObject.layer)) != 0)
            {
                int numCollisionEvents = sparks.GetCollisionEvents(_other, collisions);

                Enemy enemy = _other.GetComponent<Enemy>();
                int i = 0;

                while (i < numCollisionEvents)
                {
                    if (enemy)
                    {
                        enemy.Damage(2.5f);
                    }
                    i++;
                }
            }
        }
    }

    public void Fire()
    {
        if (target != null)
        {
            transform.LookAt(target);

            Vector3 scale = transform.localScale;
            scale.z = Vector3.Distance(transform.position, target.transform.position);
            transform.localScale = scale;

            lightningBolt.Emit(1);
            sparks.Emit(10);

            Enemy enemy = target.GetComponent<Enemy>();
            if(enemy)
            {
                Petard pet = enemy.GetComponent<Petard>();
                if (pet)
                {
                    pet.SetOffBarrel();
                }
                else
                {
                    enemy.Damage(damage);
                }
            }
        }
    }
}
