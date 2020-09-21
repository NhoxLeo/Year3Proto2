using UnityEngine;

public class Arrow : PhysicalProjectile
{
    private readonly Vector3 midPointOffset = new Vector3(0.0f, 0.065f, 0.0f);
    public bool Pierce { get; set; }

    protected override void Start()
    {
        base.Start();

        Vector3 heading = target.position + midPointOffset - transform.position;
        if (Physics.Raycast(transform.position, heading.normalized, out RaycastHit _hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            endPosition = _hit.point;
        }
        else
        {
            endPosition = heading.normalized * 1000f;
        }
    }

    protected override void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance)
    {
        transform.position += _direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_heading + midPointOffset);
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
    }
}
