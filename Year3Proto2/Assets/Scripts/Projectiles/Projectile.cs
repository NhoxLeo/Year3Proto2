using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float destroyInSeconds = 3.0f;
    [SerializeField] protected float damage = 10.0f;
    [SerializeField] protected float speed = 1.0f;

    public Vector3 Destination { get; set; }

    protected Transform target;

    protected virtual void Start()
    {
        StartCoroutine(DestroyLater(destroyInSeconds));
    }

    private IEnumerator DestroyLater(float _seconds)
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
        Destination = target.position;
    }

    public float GetDamage()
    {
        return damage;
    }
}
