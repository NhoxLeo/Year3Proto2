using UnityEngine;

public class Arrow : PhysicalProjectile
{
    private void Start()
    {
        StartCoroutine(DestroyLater(2));
    }

    public override Vector3 CalculateVelocity()
    {
        return (target.transform.position - transform.position) * speed + Vector3.forward;
    }


    public override void OnProjectileHit(GameObject _target, Vector3 _contactPoint)
    {
        Structure structure = _target.GetComponent<Structure>();
        if(structure) structure.Damage(damage);

        Enemy enemy = _target.GetComponent<Enemy>();
        if(enemy) enemy.Damage(damage);

        Destroy(gameObject);
    }
}
