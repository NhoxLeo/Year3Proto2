using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float steeringForce = 0.1f;
    [SerializeField] private float time = 2.0f;
    
    [Header("Spawn Location")]
    [SerializeField] private float spawnPointOffset = 1.2f;
    [SerializeField] private int spawnPointColumns = 3;
    [SerializeField] private int capacity = 9;

    [Header("Pointer")]
    [SerializeField] private Transform pointerPrefab;
    private Transform pointer;

    private Transform target;
    private Vector3 velocity, initialLocation;
    private float distance = float.MaxValue;

    private List<Transform> enemies;

    private void Update() 
    {
        if(target)
        {
            float distance = (target.transform.position - transform.position).sqrMagnitude;

            if (distance < 0.25f)
            {
                Deploy();
                return;
            }

            velocity += steeringForce * Time.deltaTime * (target.transform.position - (transform.position + velocity * time));

            if (velocity.sqrMagnitude > (maxSpeed * maxSpeed))
            {
                velocity = velocity.normalized * maxSpeed;
            }

            transform.position += velocity * Time.deltaTime;
        }
    }

    public bool HasTarget()
    {
        List<TileBehaviour> list = new List<TileBehaviour>(FindObjectsOfType<TileBehaviour>());
        list.RemoveAll(element => element.GetAttached() != null && element.GetApproached());

        list.ForEach(tile =>
        {
            float distance = (tile.transform.position - transform.position).sqrMagnitude;
            if (distance < this.distance)
            {
                this.distance = distance;
                target = tile.transform;
            }
        });

        return target;
    }

    public void Embark(List<Transform> enemies)
    {

        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(true);

            initialLocation = target.transform.position;

            pointer = Instantiate(pointerPrefab, transform);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();

            if (airshipPointer) airshipPointer.SetTargetPosition(transform);


            float angle = Random.Range(60.0f, 80.0f) * (Random.Range(0, 1) * 2 - 1);
            velocity = Quaternion.Euler(0.0f, angle, 0.0f) * (initialLocation - transform.position).normalized * 2.0f;
            distance = Mathf.Sqrt(distance);

            this.enemies = enemies;
        }
    }

    private void Deploy()
    {
        if(pointer) Destroy(pointer.gameObject);

        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(false);

            if (enemies != null)
            {

                StartCoroutine(Disembark());
                return;
            }

            Destroy(gameObject);
        }
    }

    IEnumerator Disembark()
    {
        List<Vector3> spawnPoints = GenerateSpawnPoints(target.transform, enemies.Count);

        float time = 0.0f;

        for (int i = 0; i < enemies.Count; i++)
        {
            time += 1.0f;
            Transform enemyPrefab = enemies[i];
            Vector3 enemySpawnPoint = spawnPoints[i];

            if (enemySpawnPoint != null)
            {
                Transform enemy = Instantiate(enemyPrefab, null);
                enemy.position = transform.position;
                enemy.DOJump(enemySpawnPoint, 0.2f, 1, 1.0f);
            }
        }

        yield return new WaitForSeconds(time);

        Depart();

        yield return 0;
    }

    private List<Vector3> GenerateSpawnPoints(Transform _transform, int amount)
    {
        List<Vector3> vectors = new List<Vector3>();

        Vector3 halfScale = _transform.localScale / 2.0f;

        float xOffset = halfScale.x - spawnPointOffset;
        float zOffset = halfScale.z - spawnPointOffset;

        for (int i = 0; i < amount; i++)
        {
            Vector3 position = new Vector3(
                _transform.position.x + (i % (Mathf.Sqrt(capacity) / 2.0f * xOffset)) - (xOffset / 2.0f),
                _transform.position.y + halfScale.y,
                _transform.position.z + (i / (Mathf.Sqrt(capacity) / 2.0f * zOffset)) - (zOffset / 2.0f)
            );

            vectors.Add(position);
        }
        return vectors;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if(target) GenerateSpawnPoints(target.transform, capacity).ForEach(point => Gizmos.DrawSphere(point, 0.1f));
    }

    private void Depart()
    {
        target = null;
    }
}
