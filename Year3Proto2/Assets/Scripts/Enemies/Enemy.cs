using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum EnemyState
{
    IDLE,
    WALK,
    ACTION
}

public abstract class Enemy : MonoBehaviour
{
    private Transform target = null;
    private Tween tween;
    private EnemyState enemyState;
    private float speed = 2.0f;

    public abstract void Action(GameObject gameObject);

    public void Walk()
    {
        if (enemyState != EnemyState.WALK)
        {
            tween = transform.DOMoveY(0.75f, 0.25f).SetLoops(-1, LoopType.Yoyo);
            enemyState = EnemyState.WALK;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target == null) return;
        if(other.transform == target) Action(target.gameObject);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void Next()
    {
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Structure potentialTarget in FindObjectsOfType<Structure>())
        {
            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                target = potentialTarget.transform;
            }
        }
    }

    public Transform GetTarget()
    {
        return target;
    }

    private void OnDrawGizmosSelected()
    {
        if(target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
