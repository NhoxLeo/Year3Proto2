using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
public enum EnemyState
{
    IDLE,
    WALK,
    ACTION
}

public abstract class Enemy : MonoBehaviour
{
    private bool delayedDeathCalled = false;
    private float delayedDeathTimer = 0f;
    private float avoidForce = 0.05f;
    protected Structure target = null;
    protected Soldier defenseTarget = null;
    protected EnemyState enemyState = EnemyState.IDLE;
    protected List<GameObject> enemiesInArea = new List<GameObject>();
    protected bool needToMoveAway;
    protected float finalSpeed = 0.0f;
    protected Animator animator;
    protected bool action = false;
    [HideInInspector]
    public GameObject puffEffect;
    [HideInInspector]
    public float health = 10.0f;
    [HideInInspector]
    public float damage = 2.0f;
    [HideInInspector]
    public bool nextReturnFalse = false;

    protected List<StructureType> structureTypes;

    private Rigidbody body;

    public abstract void Action();

    protected void EnemyStart()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.k_iSwiftFootwork) ? 1.4f : 1.0f;
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }

    public virtual void OnKill()
    {
        FindObjectOfType<EnemySpawner>().OnEnemyDeath(this);
    }

    protected virtual void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.right = transform.forward;
    }

    protected virtual void Update()
    {
        if (GlobalData.longhausDead)
        {
            if (!delayedDeathCalled)
            {
                delayedDeathCalled = true;
                delayedDeathTimer = UnityEngine.Random.Range(0.5f, 3.5f);
            }
            delayedDeathTimer -= Time.deltaTime;
            if (delayedDeathTimer <= 0f)
            {
                Damage(health);
            }
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
                if (structure.attachedTile)
                {
                    Vector3 directionToTarget = structure.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        //transform.LookAt(structure.transform);

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
        if(other.GetComponent<Enemy>())
        {
            if (!enemiesInArea.Contains(other.gameObject)) enemiesInArea.Add(other.gameObject);
        }
        /*
        if (target)
        {
            if (other.gameObject == target.gameObject)
            {
                //Debug.Log(target.name);
                enemyState = EnemyState.ACTION; 

                transform.DOKill(false);
                transform.DOMoveY(yPosition, 0.25f);
            }
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Enemy>())
        {
            if (enemiesInArea.Contains(other.gameObject)) enemiesInArea.Remove(other.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.transform.position);
        }
    }

    protected Vector3 GetMotionVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        bool enemyWasNull = false;
        foreach (GameObject enemy in enemiesInArea)
        {
            if (!enemy)
            {
                enemyWasNull = true;
                continue;
            }
            // get a vector pointing from them to me, indicating a direction for this enemy to push 
            Vector3 enemyToThis = transform.position - enemy.transform.position;
            enemyToThis.y = 0f;
            float inverseMag = 1f / enemyToThis.magnitude;
            if (inverseMag == Mathf.Infinity) { continue; }
            finalMotionVector += enemyToThis.normalized * inverseMag * avoidForce;
        }
        if (enemyWasNull)
        {
            enemiesInArea.RemoveAll(enemy => !enemy);
        }
        if (Vector3.Angle(finalMotionVector.normalized, toTarget.normalized) > 90f)
        {
            return Vector3.zero;
        }
        return finalMotionVector.normalized * finalSpeed;
    }

    public Structure GetTarget()
    {
        return target;
    }

    public EnemyState GetState()
    {
        return enemyState;
    }

    public void SetState(EnemyState _newState)
    {
        enemyState = _newState;
    }

    public void SetTarget(Structure _target)
    {
        target = _target;
    }

    public void SetTargetNull()
    {
        target = null;
    }

    public Rigidbody GetBody()
    {
        return body;
    }

    public void Damage(float _damage)
    {
        health -= _damage;
        if (health <= 0f)
        {
            OnKill();
            Destroy(gameObject);
        }
    }
}
