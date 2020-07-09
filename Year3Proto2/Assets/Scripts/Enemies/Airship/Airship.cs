using System.Collections;
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
    private Transform pointer;

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
        List<TileBehaviour> list = new List<TileBehaviour>(FindObjectsOfType<TileBehaviour>());
        list.RemoveAll(element => element.GetAttached() != null);

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
            float distance = (target.position - transform.position).sqrMagnitude;

            if (distance < 0.25f)
            {
                Deploy();
                //StartCoroutine(Depart(2));
                return;
            }

            velocity += steeringForce * Time.deltaTime * (target.position - (transform.position + velocity * time));
            if (velocity.sqrMagnitude > (maxSpeed * maxSpeed)) velocity = velocity.normalized * maxSpeed;

            transform.position += velocity * Time.deltaTime;
        }
    }

    public void Embark(/*List<Transform> enemies*/)
    {
        if (target)
        {
            pointer = Instantiate(pointerPrefab, transform);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();
            if (airshipPointer) airshipPointer.SetTargetPosition(transform);
            

            float angle = Random.Range(60.0f, 80.0f) * (Random.Range(0, 1) * 2 - 1);
            velocity = Quaternion.Euler(0.0f, angle, 0.0f) * (target.position - transform.position).normalized * 2.0f;
            distance = Mathf.Sqrt(distance);

            //this.enemies = enemies;
            return;
        }

        Destroy(this);
    }

    private void Deploy()
    {
        if(pointer) Destroy(pointer.gameObject);

        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        if(tileBehaviour)
        {
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
