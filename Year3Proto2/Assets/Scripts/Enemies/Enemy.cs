using UnityEngine;
using DG.Tweening;

public enum EnemyState
{
    IDLE,
    WALK,
    ACTION
}

public abstract class Enemy<T>: MonoBehaviour
{
    private Transform target = null;
    private EnemyState enemyState = EnemyState.IDLE;

    private float yPosition;
    private float speed = 0.25f;

    public abstract void Action(T t);

    private void Update()
    {
        switch (enemyState)
        {
            case EnemyState.ACTION:
                Action(target.GetComponent<T>());
                break;
            case EnemyState.WALK:
                if (target.GetComponent<Structure>().attachedTile == null) Next();
                transform.position += transform.forward * speed * Time.deltaTime;
                break;
            case EnemyState.IDLE:
                yPosition = transform.position.y;
                transform.DOMoveY(yPosition + (speed / 2.0f), 0.25f).SetLoops(-1, LoopType.Yoyo);

                Next();

                if (target == null)
                {
                    transform.DOKill(false);
                    Destroy(gameObject);
                }
                break;
        }
    }

    public void Next()
    {
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject @object in FindObjectsOfType<GameObject>())
        {
            if (@object.GetComponent<T>() != null)
            {
                if (@object.GetComponent<Structure>().attachedTile != null)
                {
                    Vector3 directionToTarget = @object.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        transform.LookAt(@object.transform);
                        enemyState = EnemyState.WALK;

                        target = @object.transform;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == target && target != null)
        {
            enemyState = EnemyState.ACTION; 

            transform.DOKill(false);
            transform.DOMoveY(yPosition, 0.25f);
        }
    }
}
