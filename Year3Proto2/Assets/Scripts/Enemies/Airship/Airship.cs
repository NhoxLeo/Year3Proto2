﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airship : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float maxSpeed = 1.0f;
    [SerializeField] private float steeringForce = 0.1f;
    [SerializeField] private float time = 2.0f;

    [Header("Pointer")]
    [SerializeField] private Transform pointerPrefab;
    [SerializeField] private Transform pointerTarget;
    private Transform pointer;
    private GameObject pointerParent;

    private Transform target;
    private Vector3 velocity;
    private float distance = float.MaxValue;
    private bool docked = false;

    private void OnBecameInvisible()
    {
        if (docked) Destroy(this);
    }

    private void Start()
    {
        pointerParent = GameObject.Find("Airship Pointers");
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

        Embark();
    }

    private void Update() 
    {
        if(target)
        {
            Vector3 displacement = target.position - transform.position;
            Vector3 direction = displacement.normalized;

            float distance = displacement.sqrMagnitude;
            Quaternion rotation = Quaternion.LookRotation(direction);

            if (distance < 0.25f)
            {
                Deploy();
                //StartCoroutine(Depart(2));
                return;
            }

            velocity += steeringForce * Time.deltaTime * (target.position - (transform.position + velocity * time));
            if (velocity.sqrMagnitude > (maxSpeed * maxSpeed)) velocity = velocity.normalized * maxSpeed;

            transform.position += velocity * Time.deltaTime;
            transform.rotation = rotation;


        }
    }

    public void Embark(/*List<Transform> enemies*/)
    {
        if (target)
        {
            if(pointerPrefab && pointerParent) pointer = Instantiate(pointerPrefab, pointerParent.transform);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();
            if (airshipPointer) airshipPointer.SetTarget(pointerTarget);

            Vector3 displacement = target.position - transform.position;
            float angle = Random.Range(60.0f, 80.0f) * (Random.Range(0, 1) * 2 - 1);

            velocity = Quaternion.Euler(0.0f, angle, 0.0f) * displacement.normalized * 2.0f;
            distance = Mathf.Sqrt(distance);

            TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
            if (tileBehaviour) tileBehaviour.SetApproached(true);

            //this.enemies = enemies;
            return;
        }

        Destroy(this);
    }

    private void Deploy()
    {
        if(pointer) Destroy(pointer.gameObject);

        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(false);

            //if(enemies != null) enemies.ForEach(enemy=> { }); Place enemies nicely on a tile.   

            //Bens Idea

            // Each enemy jumps out one after another
            // The scale up once they physically jump out
            // Once all the enemies are out of the airship:
            // They will start moving to their targets location.
        }
    }

    

    IEnumerator Depart(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        //Potentially depart airship...
        yield return null;
    }
}
