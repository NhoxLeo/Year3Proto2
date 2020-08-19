using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected GameObject target;

    [Header("Properties")]
    [SerializeField] protected LayerMask layerMask;
    [SerializeField] protected float damage = 10.0f;
    [SerializeField] protected float speed = 1.0f;

    public abstract void OnProjectileHit(GameObject _target, Vector3 _contactPoint);

    public abstract void Launch();

    public IEnumerator DestroyLater(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        Destroy(gameObject);
    }

    public void SetTarget(GameObject _target)
    {
        target = _target;
    }
}
