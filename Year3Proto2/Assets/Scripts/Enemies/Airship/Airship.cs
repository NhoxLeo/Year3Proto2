using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : Airship.cs
// Description  : Airsips carry enemy troops
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public enum AirshipState
{
    Idle,
    Move,
    Deploy,
    Depart
}

public class Airship : MonoBehaviour
{
    [Header("Pointer")]
    [SerializeField] private Transform pointerPrefab;
    private Transform pointer;

    [Header("Spawn Location")]
    [SerializeField] private float spawnPointOffset = 1.2f;
    [SerializeField] private int capacity = 9;
    private Vector3 initialLocation;
    private Vector3 controlPoint = Vector3.zero;

    [Header("Attributes")]
    [SerializeField] private float height = 0.75f;
    [SerializeField] private float speed = 2.4f;
    [SerializeField] private float distanceOffset = 0.75f;
    [SerializeField] private Transform[] transforms;

    private Transform target;
    private float distance = float.MaxValue;

    private AirshipState airshipState;
    private EnemySpawner enemySpawner;

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        if (target)
        {
            Vector3 heading;
            Vector3 direction;

            switch (airshipState)
            {
                case AirshipState.Depart:
                    Vector3 newLocation = initialLocation;
                    newLocation.y = height;

                    heading = newLocation - transform.position;
                    direction = heading.normalized;

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), speed * Time.deltaTime * 2.0f);
                    transform.position += heading * Time.deltaTime * speed / 4.0f;

                    Vector3 position = transform.position;
                    position.y = height;
                    transform.position = position;

                    if (heading.sqrMagnitude < distanceOffset * 4.0f)
                    {
                        Destroy(gameObject);
                    }

                    break;
                case AirshipState.Move:
                    Vector3 targetPosition = target.position;
                    targetPosition.y = height;

                    heading = targetPosition - transform.position;
                    direction = heading.normalized;


                    // Update Airship Values
                    transform.rotation = Quaternion.LookRotation(direction);
                    transform.position += heading * Time.deltaTime * speed;

                    Vector3 newPosition = transform.position;
                    newPosition.y = height;

                    transform.position = newPosition;

                    if (heading.sqrMagnitude < Mathf.Pow(distanceOffset, 2))
                    {
                        airshipState = AirshipState.Deploy;
                        Destroy(pointer.gameObject);
                        StartCoroutine(Deploy(1.5f));
                    }
                    break;
            }
        }
    }

    /**************************************
    * Name of the Function: Deploy
    * @Author: Tjeu Vreeburg
    * @Parameter: Float
    * @Return: IEnumerator
    ***************************************/
    
    private IEnumerator Deploy(float seconds)
    {
        WaitForSeconds wait = new WaitForSeconds(seconds);
        List<Vector3> spawnPoints = GenerateSpawnPoints();
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] == null) break;

            Transform instantiatedTransform = Instantiate(transforms[i], transform.position, Quaternion.identity);
            instantiatedTransform.position = spawnPoints[i];

            Invader invader = instantiatedTransform.GetComponent<Invader>();
            if(invader)
            {
                invader.SetScale(Random.Range(0.8f, 1.5f));
                invader.spawner = enemySpawner;
                enemySpawner.RecordNewEnemy(invader);
            }

            HeavyInvader heavyInvader = instantiatedTransform.GetComponent<HeavyInvader>();
            if (heavyInvader)
            {
                heavyInvader.Randomize();
                heavyInvader.spawner = enemySpawner;
                enemySpawner.RecordNewEnemy(heavyInvader);
            }


            yield return wait;

        }

        yield return wait;


        target.GetComponent<TileBehaviour>().SetApproached(false);
        airshipState = AirshipState.Depart;
        yield return null;
    }

    /**************************************
    * Name of the Function: Embark
    * @Author: Tjeu Vreeburg
    * @Parameter: Transform Array, Transform
    * @Return: void
    ***************************************/
    public void Embark(Transform[] transforms, Transform pointerParent)
    {
        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        // Check if target is a tile.
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(true);
            initialLocation = transform.position;

            // Instantiate and Setup pointer.
            if (pointerPrefab) pointer = Instantiate(pointerPrefab, pointerParent);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();
            if (airshipPointer) airshipPointer.SetTarget(transform);

            this.transforms = transforms;
            airshipState = AirshipState.Move;
            return;
        }

        Destroy(gameObject);
    }

    /**************************************
    * Name of the Function: GenerateSpawnPoints
    * @Author: Tjeu Vreeburg
    * @Parameter: Transform, Integer
    * @Return: Vector3 List
    ***************************************/
    private List<Vector3> GenerateSpawnPoints()
    {
        List<Vector3> vectors = new List<Vector3>();

        Vector3 halfScale = target.localScale / 2.0f;

        float xOffset = halfScale.x - spawnPointOffset;
        float zOffset = halfScale.z - spawnPointOffset;

        int columns = (int)Mathf.Sqrt(capacity);

        for (int i = 0; i < transforms.Length; i++)
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

    /**************************************
    * Name of the Function: OnDrawGizmosSelected
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(initialLocation, 0.1f);
        Gizmos.DrawSphere(controlPoint, 0.1f);
        Gizmos.DrawSphere(target.position, 0.1f);

        if (target && transforms != null) {
            List<Vector3> spawnPoints = GenerateSpawnPoints();
            foreach (Vector3 spawnPoint in spawnPoints) Gizmos.DrawSphere(spawnPoint, 0.1f);
        }
    }

    /**************************************
    * Name of the Function: HasTarget
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: boolean
    ***************************************/
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

    public void SetSpawner(EnemySpawner _enemySpawner)
    {
        enemySpawner = _enemySpawner;
    }
}
