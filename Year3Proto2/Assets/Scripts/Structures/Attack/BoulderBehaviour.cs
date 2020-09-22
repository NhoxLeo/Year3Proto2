using UnityEngine;

public class BoulderBehaviour : MonoBehaviour
{
    public Vector3 origin = Vector3.zero;
    public Vector3 current = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public float damage = 5f;
    public float speed = 0.8f;
    public float explosionRadius = 0.5f;
    private float arcFactor = 0.60f;
    private float distanceTravelled = 0f;

    // Start is called before the first frame update
    void Start()
    {
        origin = current = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 direction = target - current;
        current += direction.normalized * speed * Time.deltaTime;
        distanceTravelled += speed * Time.deltaTime;

        float totalDistance = Vector3.Distance(origin, target);
        float heightOffset = arcFactor * totalDistance * Mathf.Sin(distanceTravelled * Mathf.PI / totalDistance);
        transform.position = current + new Vector3(0, heightOffset, 0);

        if (transform.position.y <= 0.51f)
        {
            RaycastHit[] hitEnemies = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("EnemyStructureCollider"));
            GameObject explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
            explosion.transform.localScale *= 3f * explosionRadius;
            foreach (RaycastHit enemyHit in hitEnemies)
            {
                Enemy enemy = enemyHit.transform.GetComponent<Enemy>();
                if (enemy.enemyName == EnemyNames.Petard)
                {
                    enemy.GetComponent<Petard>().SetOffBarrel();
                    continue;
                }
                if (enemy)
                {
                    enemy.Damage(damage);
                }
            }
            Destroy(gameObject);
            GameManager.CreateAudioEffect("Explosion", transform.position, 0.6f);
        }
    }
}
