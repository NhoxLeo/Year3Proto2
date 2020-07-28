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
    [SerializeField] private float speed = 2.4f;
    [SerializeField] private List<Transform> transforms;

    private Transform target;
    private float distance = float.MaxValue;
    private float count = 0.0f;

    private AirshipState airshipState;

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update() 
    {
        if(target)
        {
            switch(airshipState)
            {
                case AirshipState.Depart:
                    Vector3 difference = initialLocation - transform.position;
                    if (difference.sqrMagnitude < 1.0f)
                    {
                        Destroy(gameObject);
                        return;
                    }

                    transform.position = difference.normalized * speed;
                    break;
                case AirshipState.Move:
                    if (count < 1.0f)
                    {
                        // Redo movement bezier. 

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
                    else
                    {
                        airshipState = AirshipState.Idle;
                        StartCoroutine(Deploy(1.0f));
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
        for (int i = 0; i < transforms.Count; i++)
        {
            Debug.Log("Airship Deploying " + i);
            Transform instantiatedTransform = Instantiate(transforms[i]);
            instantiatedTransform.DOLocalJump(spawnPoints[i], 0.1f, 1, seconds);
            yield return wait;
        }

        airshipState = AirshipState.Depart;
        target = null;
        yield return null;
    }

    /**************************************
    * Name of the Function: Embark
    * @Author: Tjeu Vreeburg
    * @Parameter: Transform Array, Transform
    * @Return: void
    ***************************************/
    public void Embark(List<Transform> transforms, Transform pointerParent)
    {
        TileBehaviour tileBehaviour = target.GetComponent<TileBehaviour>();
        // Check if target is a tile.
        if (tileBehaviour)
        {
            tileBehaviour.SetApproached(true);

            initialLocation = transform.position;

            // TODO: Determine Vector3.forward or Vector3.backward
            controlPoint = transform.position + (target.position - transform.position) / 2 + Vector3.forward * 20.0f;

            // Instantiate and Setup pointer.
            if (pointerPrefab) pointer = Instantiate(pointerPrefab, pointerParent);
            AirshipPointer airshipPointer = pointer.GetComponent<AirshipPointer>();
            if (airshipPointer) airshipPointer.SetTarget(transform);

            this.transforms = transforms;
            airshipState = AirshipState.Move;

            return;
        }

        Destroy(this);
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

        for (int i = 0; i < transforms.Count; i++)
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
        Gizmos.DrawSphere(initialLocation, 0.2f);
        Gizmos.DrawSphere(controlPoint, 0.2f);
        Gizmos.DrawSphere(target.position, 0.2f);

        if (target && transforms != null) {
            List<Vector3> spawnPoints = GenerateSpawnPoints();
            foreach (Vector3 spawnPoint in spawnPoints) Gizmos.DrawSphere(spawnPoint, 0.2f);
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
}
