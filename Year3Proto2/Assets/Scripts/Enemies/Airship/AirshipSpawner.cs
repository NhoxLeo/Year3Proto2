﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirshipSpawner : MonoBehaviour
{
    [SerializeField] private Transform airshipPrefab;
    [SerializeField] private Transform pointerParent;
    [SerializeField] private float radiusOffset;

    private float distance = 0.0f;

    private void Start()
    {
        // Find all tilebehaviours in scene and calculate furthest distance tile from center/longhaus
        TileBehaviour[] tileBehaviours = FindObjectsOfType<TileBehaviour>();
        for (int i = 0; i < tileBehaviours.Length; i++)
        {
            float distance = (tileBehaviours[i].transform.position - transform.position).sqrMagnitude;
            if (distance > this.distance) this.distance = distance;
        }
        distance = Mathf.Sqrt(distance) + radiusOffset;
    } 
    
    public void Spawn(Transform[] transforms)
    {
        Debug.Log("Spawning " + transforms.Length + " Enemies");

        float angle = Random.Range(0.0f, 360.0f);
        Vector3 location = new Vector3(Mathf.Sin(angle) * distance, 0.0f, Mathf.Cos(angle) * distance)
        {
            y = 0.0f
        };

        Transform instantiatedAirship = Instantiate(airshipPrefab, location, Quaternion.identity, transform);

        Airship airship = instantiatedAirship.GetComponent<Airship>();
        if (airship.HasTarget()) airship.Embark(transforms, pointerParent);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, distance);
    }
}