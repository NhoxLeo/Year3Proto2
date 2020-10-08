using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    [SerializeField] private ParticleSystem lightningBolt;
    [SerializeField] private ParticleSystem sparks;

    private const float DetectionRadius = 0.4f;
    private const float ChainDamageMultiplier = 0.35f;

    public Transform Fire(Vector3 _origin, Transform _target, ref List<Transform> _previousTargets, float _damage)
    {
        transform.position = _origin;
        if (_previousTargets != null)
        {
            if (_previousTargets.Count > 0)
            {
                transform.position = _previousTargets[_previousTargets.Count - 1].position;
            }
        }
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
                bool chain = false;
                if (_previousTargets != null)
                {
                    if (_previousTargets.Count > 0)
                    {
                        chain = true;
                    }
                }
                enemy.Damage(_damage * (chain ? ChainDamageMultiplier : 1.0f));
            }
        }

        if (_previousTargets == null)
        {
            return null;
        }
        else
        {
            _previousTargets.Add(_target);
        }

        RaycastHit[] hitEnemies = Physics.SphereCastAll(_target.position, DetectionRadius, Vector3.up, 0f, LayerMask.GetMask("EnemyStructureCollider"));
        for(int i = 0; i < hitEnemies.Length; i++)
        {
            RaycastHit raycastHit = hitEnemies[i];
                
            enemy = raycastHit.transform.GetComponent<Enemy>();
            if (enemy)
            {
                if(!_previousTargets.Contains(raycastHit.transform))
                {
                    return enemy.transform;
                }
            }
        }
        return null;
    }
}
