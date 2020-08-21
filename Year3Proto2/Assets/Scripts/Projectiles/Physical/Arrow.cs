using UnityEngine;

public class Arrow : PhysicalProjectile
{
    private bool pierce;

    private void Start()
    {
        StartCoroutine(DestroyLater(3));
        Launch();
    }

    public override Vector3 CalculateVelocity()
    {
        Enemy enemy = target.GetComponent<Enemy>();

        Vector3 velocity = Vector3.zero;
        if(enemy) velocity = enemy.GetBody().velocity;

        Vector3 heading = target.transform.position - transform.position;
        Vector3 direction = heading.normalized;
        transform.rotation = Quaternion.LookRotation(heading);

        // Velocity Vector for the arrow.
        return direction * speed + velocity.normalized;
    }


    public override void OnProjectileHit(Transform _target, Vector3 _contactPoint)
    {
        // Makes sure that the target hit is actually the correct target
        if(_target == target)
        {
            // Checks whether the target is an enemy.
            Enemy enemy = target.GetComponent<Enemy>();
            if(enemy) enemy.Damage(damage);

            // Checks whether the target is a structure.
            Structure structure = target.GetComponent<Structure>();
            if (structure) structure.Damage(damage);

        }

        Destroy(gameObject);
    }

    public void SetPierce(bool _pierce)
    {
        pierce = _pierce;
    }

    public bool IsPierce()
    {
        return pierce;
    }
}
