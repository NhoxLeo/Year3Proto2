using UnityEngine;

public class Boulder : PhysicalProjectile
{
    private readonly float height = 0.8f;
    private readonly float radius = 10.0f;
    [SerializeField] private float explosionRadius;

    private void Start() 
    {
        Physics.gravity = Vector3.up * -1.8f;
        StartCoroutine(DestroyLater(10));
    }

    public override void OnProjectileHit(Transform _target, Vector3 _contactPoint)
    {
        Collider[] hitColliders = Physics.OverlapSphere(_contactPoint, radius);

        for(int i = 0; i < hitColliders.Length; i++)
        {
            Enemy enemy = hitColliders[i].gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
                enemy.Damage((1.0f / Vector3.Distance(enemy.transform.position, _contactPoint)) * damage);
            }
        }

        Destroy(gameObject); 
    }

    public override Vector3 CalculateVelocity()
    {
        float displacementY = target.transform.position.y - transform.position.y;
        Vector3 displacementXZ = new Vector3(target.transform.position.x - transform.position.x, 0.0f, target.transform.position.z - transform.position.z);

        float time = Mathf.Sqrt(-2.0f * height / Physics.gravity.y) + Mathf.Sqrt(2.0f * (displacementY - height) / Physics.gravity.y);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2.0f * Physics.gravity.y * height);
        Vector3 velocityXZ = target.GetComponent<Enemy>().GetBody().velocity + displacementXZ / time;

        return velocityXZ + velocityY * -Mathf.Sign(Physics.gravity.y);
    }

    public void SetExplosionRadius(float _explosionRadius)
    {
        explosionRadius = _explosionRadius;
    }
    
    public float GetExplosionRadius()
    {
        return explosionRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
