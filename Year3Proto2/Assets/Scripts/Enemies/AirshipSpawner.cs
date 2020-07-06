using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirshipSpawner : MonoBehaviour
{
    [SerializeField] private Transform airshipPrefab;
    [SerializeField] private float radiusOffset;

    private float distance = 0.0f;

    private void Start()
    {
        TileBehaviour[] tileBehaviours = FindObjectsOfType<TileBehaviour>();
        for(int i = 0; i < tileBehaviours.Length; i++)
        {
            float distance = (tileBehaviours[i].transform.position - transform.position).sqrMagnitude;
            if (distance > this.distance) this.distance = distance;
        }

        distance = Mathf.Sqrt(distance) + radiusOffset;
    } 
    
    private void Spawn()
    {
        float angle = Random.Range(0.0f, 360.0f);
        Vector3 location = new Vector3(Mathf.Sin(angle) * distance, 0.0f, Mathf.Cos(angle) * distance);
        location.y = 0.0f;
     
        Instantiate(airshipPrefab, location, Quaternion.identity, transform);
    }
}
