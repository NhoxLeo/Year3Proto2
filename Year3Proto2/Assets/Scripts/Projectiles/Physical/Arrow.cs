using UnityEngine;

public class Arrow : PhysicalProjectile
{
    public bool Pierce { get; set; }

    protected override void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance)
    {
        transform.position += _direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_heading);
    }

    protected override void OnDestination(Vector3 _location)
    {
        if (target)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy) enemy.Damage(damage);

            // Checks whether the target is a structure.
            Structure structure = target.GetComponent<Structure>();
            if (structure) structure.Damage(damage);
        }

        Destroy(gameObject);
    }
}
