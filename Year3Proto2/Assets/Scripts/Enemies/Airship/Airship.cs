using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship : MonoBehaviour
{
    [Header("Pointer")]
    [SerializeField] private Transform pointerPrefab;
    [SerializeField] private Transform pointerTarget;
    private Transform pointer;
    private GameObject pointerParent;

    [Header("Spawn Location")]
    [SerializeField] private float spawnPointOffset = 1.2f;
    [SerializeField] private int capacity = 9;
    private Vector3 initialLocation;
    private Vector3 controlPoint = Vector3.zero;

    private Transform target;
    private float distance = float.MaxValue;

    private List<Transform> enemies;
    private float count = 0.0f;

    private void Update() 
    {
        if(target)
        {
            if(count < 1.0f)
            {
                count += 0.02f * Time.deltaTime;
                Vector3 oneTwo = Vector3.Lerp(initialLocation, controlPoint, count);
                Vector3 twoThree = Vector3.Lerp(controlPoint, target.position, count);

                Vector3 position = Vector3.Lerp(oneTwo, twoThree, count);
                position.y = 0.0f;

                Vector3 direction = position - transform.position;


                // Update Airship Values
                transform.rotation = Quaternion.LookRotation(direction.normalized);
                transform.position = position;
            }
        }
    }
    
    public void Embark(List<Transform> enemies)
    {
        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        // Check if target is a tile.
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(true);

            initialLocation = transform.position;
            controlPoint = transform.position + (target.position - transform.position) / 2 + Vector3.forward * 20.0f;
            
            // Instantiate Pointer
            if (pointerPrefab && pointerParent) pointer = Instantiate(pointerPrefab, pointerParent.transform);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();
            if (airshipPointer) airshipPointer.SetTarget(pointerTarget);

            this.enemies = enemies;
            return;
        }

        Destroy(this);
    }


    IEnumerator Disembark()
    {
        // Generate spawnpoints based on enemy count.
        List<Vector3> spawnPoints = GenerateSpawnPoints(target.transform, enemies.Count);

        float time = 0.0f;

        // Iterate through all the enemy prefabs.
        for (int i = 0; i < enemies.Count; i++)
        {
            time += 1.0f;
            Transform enemyPrefab = enemies[i];
            Vector3 enemySpawnPoint = spawnPoints[i];

            // Check if spawnpoint is available
            if (enemySpawnPoint != null)
            {
                // Instantiate enemy prefabs
                Transform enemy = Instantiate(enemyPrefab, null);
                enemy.position = transform.position;
                enemy.DOJump(enemySpawnPoint, 0.2f, 1, 1.0f);
            }
        }

        yield return new WaitForSeconds(time);

        Depart();

        yield return 0;
    }

    private void Depart()
    {
        //Make airship return to original location.
        target = null;
    }

    private List<Vector3> GenerateSpawnPoints(Transform _transform, int amount)
    {
        List<Vector3> vectors = new List<Vector3>();

        Vector3 halfScale = _transform.localScale / 2.0f;

        // Generate coordinate offsets.
        float xOffset = halfScale.x - spawnPointOffset;
        float zOffset = halfScale.z - spawnPointOffset;

        for (int i = 0; i < amount; i++)
        {
            // Create position based on offset and index.
            Vector3 position = new Vector3(
                _transform.localPosition.x + (i % (Mathf.Sqrt(capacity) / 2.0f * xOffset)) - (xOffset / 2.0f),
                _transform.localPosition.y + halfScale.y,
                _transform.localPosition.z + (i / (Mathf.Sqrt(capacity) / 2.0f * zOffset)) - (zOffset / 2.0f)
            );

            // Add position to Vector list.
            vectors.Add(position);
        }
        return vectors;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(initialLocation, 0.2f);
        Gizmos.DrawSphere(controlPoint, 0.2f);
        Gizmos.DrawSphere(target.position, 0.2f);

        Gizmos.color = Color.red;
        Vector3 oneTwo = Vector3.Lerp(initialLocation, controlPoint, count);
        Vector3 twoThree = Vector3.Lerp(controlPoint, target.position, count);

        Gizmos.DrawLine(transform.position, oneTwo);
        Gizmos.DrawLine(transform.position, twoThree);
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
}
