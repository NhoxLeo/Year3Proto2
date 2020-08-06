using System;
using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : BirdsEvent.cs
// Description  : Inherits ambient event to create nice ambient behaviour in scenes.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class BirdsEvent : EnvironmentAmbientEvent
{
    [Header("Attributes")]
    [SerializeField] private int points = 10;
    [SerializeField] private int height = 2;

    [Header("Positions")]
    [SerializeField] private Vector3 originPoint;
    [SerializeField] private Vector3 originPointTwo;

    [SerializeField] private Vector3[] destinations;
    [SerializeField] private Vector3[] origins;
    private Vector3 destination, origin;

    /**************************************
     * Name of the Function: Update
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: void
     ***************************************/
    private void Update()
    {
        if(ambient && !completed)
        {
            Vector3 heading = destination - ambient.position;
            Vector3 direction = heading.normalized;

            ambient.position += direction * Time.deltaTime;
            ambient.rotation = Quaternion.LookRotation(direction);

            if (heading.sqrMagnitude < 2.0f * 2.0f) completed = true;
        }
    }

    /**************************************
     * Name of the Function: Populate
     * @Author: Tjeu Vreeburg
     * @Parameter: ref Vector3, boolean, Vector3
     * @Return: void
     ***************************************/
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
            vectors[i].y = height;
            vectors[i].z = (_inverse ? (offset.z + i) : (offset.z - i)) + inversePoint;
        }

        _vectors = vectors;
    }

    /**************************************
     * Name of the Function: Invoke
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: override void
     ***************************************/
    public override void Invoke()
    {
        Populate(ref destinations, false, originPoint);
        Populate(ref origins, true, originPointTwo);

        bool result = new System.Random().Next(0, 2) != 0;

        destination = result ? RandomFromArray(destinations) : RandomFromArray(origins);
        origin = result ? RandomFromArray(origins) : RandomFromArray(destinations);

        destinations = null;
        origins = null;

        ambient = Instantiate(ambientPrefab, origin, Quaternion.identity, transform);
    } // End of scope will free array from memory.

    /**************************************
     * Name of the Function: RandomFromArray
     * @Author: Tjeu Vreeburg
     * @Parameter: Vector3 Array
     * @Return: Vector3
     ***************************************/
    private Vector3 RandomFromArray(Vector3[] _array)
    {
        return _array[UnityEngine.Random.Range(0, _array.Length)];
    }
}
