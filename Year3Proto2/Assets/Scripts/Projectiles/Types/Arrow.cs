using UnityEngine;

public class Arrow : Projectile
{
    private void Start()
    {
        Ready();
        StartCoroutine(DestroyLater(2));
    }

    public override Vector3 CalculateVelocity()
    {
        return (target.transform.position - transform.position) * speed + Vector3.forward;
    }


    public override void OnProjectileHit(GameObject _target, Vector3 _contactPoint)
    {
        if(_target.GetComponent<Structure>() != null) _target.GetComponent<Structure>().Damage(damage);
        if(_target.GetComponent<Enemy>() != null) _target.GetComponent<Enemy>().Damage(damage);

        Destroy(gameObject);
    }
}
