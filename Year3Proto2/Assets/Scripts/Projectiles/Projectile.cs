using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected GameObject target;

    [Header("Properties")]
    public Rigidbody body;
    public LayerMask layerMask;
    public float damage = 10.0f;
    public float speed = 1.0f;

    public abstract Vector3 CalculateVelocity();

    public abstract void OnProjectileHit(GameObject _target, Vector3 _contactPoint);

    public IEnumerator DestroyLater(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision _collision)
    {
        if ((layerMask.value & (1 << _collision.gameObject.layer)) != 0)
        {
            OnProjectileHit(_collision.gameObject, _collision.contacts[0].point);
        }
    }

    public void SetTarget(GameObject _target)
    {
        target = _target;
    }

    public void Ready()
    {
        body.velocity = CalculateVelocity();
        Vector3 oppositeVelocity = -body.velocity;
        body.AddRelativeForce(oppositeVelocity);
    }
}
