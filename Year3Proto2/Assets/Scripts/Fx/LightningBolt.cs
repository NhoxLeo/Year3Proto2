using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    public Transform target;
    [SerializeField] private ParticleSystem lightningBolt;
    [SerializeField] private ParticleSystem sparks;
    private float damage;
    
    public void Initialize(Transform _target, float _damage)
    {
        target = _target;
        damage = _damage;
    }

    public void Fire(Transform _target)
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
