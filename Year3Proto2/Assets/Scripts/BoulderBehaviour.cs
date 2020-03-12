using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderBehaviour : MonoBehaviour
{
    public Vector3 origin = Vector3.zero;
    public Vector3 current = Vector3.zero;
    public Vector3 target = Vector3.zero;
    public float damage = 5f;
    public float speed = 0.8f;
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
            RaycastHit[] hitEnemies = Physics.SphereCastAll(transform.position, 0.5f, Vector3.up, 0f, 1 << LayerMask.NameToLayer("Enemy"));
            Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
            foreach (RaycastHit enemyHit in hitEnemies)
            {
                Enemy enemy = enemyHit.collider.GetComponent<Enemy>();
                if (enemy.health <= damage)
                {
                    Destroy(enemy.gameObject);
                }
                else
                {
                    enemy.health -= damage;
                }
            }
            Destroy(gameObject);
        }
    }
}
