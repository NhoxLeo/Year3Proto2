﻿using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public enum EnemyState
{
    IDLE,
    WALK,
    ACTION
}

public abstract class Enemy : MonoBehaviour
{
    private Structure target = null;
    private EnemyState enemyState = EnemyState.IDLE;
    private List<GameObject> enemiesInArea = new List<GameObject>();

    protected float health = 10.0f;
    protected float yPosition;
    protected float speed = 0.6f;
    protected float jumpHeight = 0.1f;
    protected bool action = false;

    protected List<StructureType> structureTypes;

    public abstract void Action(Structure structure);

    private void FixedUpdate()
    {
        switch (enemyState)
        {
            case EnemyState.ACTION:
                if (target == null)
                {
                    action = false;
                    enemyState = EnemyState.WALK;

                    transform.DOKill(false);
                    transform.DOMoveY(yPosition + jumpHeight, speed / 3.0f).SetLoops(-1, LoopType.Yoyo);
                }
                else
                {
                    Action(target);
                }
                break;
            case EnemyState.WALK:
                if (target)
                {
                    if (target.attachedTile == null)
                    {
                        if (!Next()) { target = null; }
                    }

                    transform.position += transform.forward * speed * Time.deltaTime;


                    enemiesInArea.RemoveAll(enemy => enemy == null);

                    foreach (GameObject @object in enemiesInArea)
                    {
                        if (Vector3.Distance(@object.transform.position, transform.position) < 0.2f)
                        {
                            transform.position = (transform.position - @object.transform.position).normalized * 0.2f + @object.transform.position;
                        }
                    }
       
                }

                Next();
                break;
            case EnemyState.IDLE:
                yPosition = transform.position.y;
                transform.DOMoveY(yPosition + jumpHeight, speed / 3.0f).SetLoops(-1, LoopType.Yoyo);

                if (!Next()) { target = null; }

                if (target == null)
                {
                    transform.DOKill(false);
                    Destroy(gameObject);
                }
                break;
        }
    }

    public bool Next()
    {
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            if (structureTypes.Contains(structure.GetStructureType()))
            {
                if (structure.attachedTile != null)
                {
                    Vector3 directionToTarget = structure.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        transform.LookAt(structure.transform);

                        enemyState = EnemyState.WALK;

                        target = structure;
                    }
                }
            }
        }
        return closestDistanceSqr != Mathf.Infinity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Enemy>() != null)
        {
            if (!enemiesInArea.Contains(other.gameObject)) enemiesInArea.Add(other.gameObject);
        }

        if (target)
        {
            if (other.gameObject == target.gameObject)
            {
                Debug.Log(target.name);
                enemyState = EnemyState.ACTION; 

                transform.DOKill(false);
                transform.DOMoveY(yPosition, 0.25f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Enemy>() != null)
        {
            if (enemiesInArea.Contains(other.gameObject)) enemiesInArea.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {

        if(target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }

    public Structure GetTarget()
    {
        return target;
    }
}
