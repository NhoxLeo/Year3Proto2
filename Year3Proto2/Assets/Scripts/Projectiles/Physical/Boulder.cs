using UnityEngine;

public class Boulder : PhysicalProjectile
{
    public float ExplosionRadius { get; set;}

    private float arcFactor = 0.60f;
    private float distanceTravelled = 0.0f;

    private Vector3 current = Vector3.zero;
    private Vector3 origin = Vector3.zero;

    protected override void Start() 
    {
        base.Start();
        ExplosionRadius = 0.8f;
        current = origin = transform.position;
        endPosition = target.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
    }

    protected override void OnDisplacement(Vector3 _heading, Vector3 _direction, float _distance)
    { 
        current += _direction * speed * Time.deltaTime;
        distanceTravelled += speed * Time.deltaTime;

        float totalDistance = Vector3.Distance(origin, Destination);
        float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
        transform.position = current + new Vector3(0, heightOffset, 0);
    }

    protected override void OnDestination(Vector3 _location)
    {

    }

    protected override void OnGroundHit()
    {
        RaycastHit[] hitEnemies = Physics.SphereCastAll(transform.position, ExplosionRadius, Vector3.up, 0f, LayerMask.GetMask("EnemyStructureCollider"));
        GameObject explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
        explosion.transform.localScale *= 2f * ExplosionRadius;
        foreach (RaycastHit enemyHit in hitEnemies)
        {
            Enemy enemy = enemyHit.transform.GetComponent<Enemy>();
            if (enemy) enemy.Damage(damage);
        }

        Destroy(gameObject);
    }
}
