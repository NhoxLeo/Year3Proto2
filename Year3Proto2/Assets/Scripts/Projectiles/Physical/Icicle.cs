using UnityEngine;

public class Icicle : PhysicalProjectile
{
    [SerializeField] private float stunDuration;

    public override Vector3 CalculateVelocity()
    {
        Enemy enemy = target.GetComponent<Enemy>();

        Vector3 velocity = Vector3.zero;
        if (enemy) velocity = enemy.GetVelocity();

        Vector3 heading = target.transform.position - transform.position;
        Vector3 direction = heading.normalized;
        transform.rotation = Quaternion.LookRotation(heading);

        // Velocity Vector for the arrow.
        return direction * speed + velocity.normalized;
    }


    public override void OnProjectileHit(Transform _target, Vector3 _contactPoint)
    {
        // Makes sure that the target hit is actually the correct target
        if (_target == target)
        { 
            // Checks whether the target is an enemy.
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy) enemy.Stun(stunDuration);
        }

        Destroy(gameObject);
    }
     
    public void SetStunDuration(float _stunDuration)
    {
        stunDuration = _stunDuration;
    }
}
