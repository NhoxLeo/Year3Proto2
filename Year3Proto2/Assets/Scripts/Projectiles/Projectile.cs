using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{ 
    [Header("Properties")]
    [SerializeField] protected LayerMask layerMask;
    [SerializeField] protected float damage = 10.0f;
    [SerializeField] protected float speed = 1.0f;

    protected Transform target;

    public abstract void OnProjectileHit(Transform _target, Vector3 _contactPoint);

    public abstract void Launch();

    public IEnumerator DestroyLater(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        Destroy(gameObject);
    }

    public void SetDamage(float  _damage)
    {
        damage = _damage;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    public float GetDamage()
    {
        return damage;
    }
}
