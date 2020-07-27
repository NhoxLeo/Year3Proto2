using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoints : MonoBehaviour
{
    [SerializeField] private float spawnPointOffset = 1.2f;
    [SerializeField] private int capacity = 9;
    [SerializeField] private int amount = 3;
    [SerializeField] private Transform target;
    private List<Vector3> GenerateSpawnPoints()
    {
        List<Vector3> vectors = new List<Vector3>();

        Vector3 halfScale = target.localScale / 2.0f;

        float xOffset = halfScale.x - spawnPointOffset;
        float zOffset = halfScale.z - spawnPointOffset;

        int columns = (int) Mathf.Sqrt(capacity);

        for (int i = 0; i < amount; i++)
        {
            float xPosition = i % columns / 2.0f * xOffset;
            float yPosition = halfScale.y;
            float zPosition = i / columns / 2.0f * zOffset;

            // Create position based on offset and index.
            Vector3 position = new Vector3(
                target.localPosition.x + xPosition - (xOffset / 2.0f),
                target.localPosition.y + yPosition,
                target.localPosition.z + zPosition - (zOffset / 2.0f) 
            );

            // Add position to Vector list.
            vectors.Add(position);
        }
        return vectors;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 spawnPoint in GenerateSpawnPoints())
        {
            Gizmos.DrawSphere(spawnPoint, 0.1f);
        }
    }
}
