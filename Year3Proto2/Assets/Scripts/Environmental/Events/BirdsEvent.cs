using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdsEvent : EnvironmentEvent
{
    [Header("Attributes")]
    [SerializeField] private int points = 10;
    [SerializeField] private Transform birdsPrefab;
    private Transform birds;

    [Header("Positions")]
    [SerializeField] private Vector3 originPoint;
    [SerializeField] private Vector3 originPointTwo;

    [SerializeField] private Vector3[] destinations;
    [SerializeField] private Vector3[] origins;
    private Vector3 destination, origin;

    private void Update()
    {
        if(birds && !completed)
        {
            Vector3 heading = (destination - origin);
            Quaternion rotation = Quaternion.LookRotation(heading.normalized);
            if (rotation != birds.rotation) birds.rotation = rotation;

            birds.position += heading.normalized * Time.deltaTime;
            if (heading.sqrMagnitude < 0.5f * 0.05) completed = true;
        }
    }

    public void Populate(ref Vector3[] _vectors, bool _inverse, Vector3 _offset)
    {
        Vector3 offset = _offset;
        offset.x -= points / 2;
        offset.z -= points / 2;

        Vector3[] vectors = new Vector3[points];
        float inversePoint = _inverse ? -(points - 1) : (points - 1);

        for (int i = 0; i < points; i++)
        {
            vectors[i].x = _inverse ? (offset.x - i) : (offset.x + i);
            vectors[i].z = (_inverse ? (offset.z + i) : (offset.z - i)) + inversePoint;
        }

        _vectors = vectors;
    }

    public override void Invoke()
    {
        Populate(ref destinations, false, originPoint);
        Populate(ref origins, true, originPointTwo);

        bool result = new System.Random().Next(0, 2) != 0;

        destination = result ? RandomFromArray(destinations) : RandomFromArray(origins);
        origin = result ? RandomFromArray(origins) : RandomFromArray(destinations);

        birds = Instantiate(birdsPrefab, origin, Quaternion.identity, transform);
    }

    private Vector3 RandomFromArray(Vector3[] _array)
    {
        return _array[Random.Range(0, _array.Length)];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for(int i = 0; i < destinations.Length; i++) Gizmos.DrawSphere(destinations[i], 0.2f);
        for(int i = 0; i < origins.Length; i++) Gizmos.DrawSphere(origins[i], 0.2f);
    }
}
