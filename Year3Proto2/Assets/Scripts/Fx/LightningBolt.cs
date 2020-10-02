using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    [SerializeField] private ParticleSystem lightningBolt;
    [SerializeField] private ParticleSystem sparks;

    private const float DetectionRadius = 2.4f;

    public Transform Fire(Vector3 _origin, Transform _target, Transform _previousTarget, float damage)
    {
        transform.position = _origin;
        transform.LookAt(_target);

        Vector3 scale = transform.localScale;
        scale.z = (_target.transform.position - transform.position).magnitude;
        transform.localScale = scale;

        lightningBolt.Emit(1);
        sparks.Emit(10);

        Enemy enemy = _target.GetComponent<Enemy>();
        if (enemy)
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

        if (_previousTarget == null) return _target;

        RaycastHit[] hitEnemies = Physics.SphereCastAll(_target.position, DetectionRadius, Vector3.up, 0f, LayerMask.GetMask("EnemyStructureCollider"));
        for(int i = 0; i < hitEnemies.Length; i++)
        {
            RaycastHit raycastHit = hitEnemies[i];
            Transform transform = raycastHit.transform;
                
            enemy = transform.GetComponent<Enemy>();
            if(enemy)
            {
                if(transform != _previousTarget)
                {
                    return enemy.transform;
                }
            }            
        }
        return null;
    }
}
