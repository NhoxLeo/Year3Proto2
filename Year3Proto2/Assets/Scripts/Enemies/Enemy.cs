using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

[Serializable]
public enum EnemyState
{
    Deploy,
    Idle,
    Walk,
    Action
}

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : Enemy.cs
// Description  : Base class for enemies.
// Author       : Tjeu Vreeburg, Samuel Fortune
// Mail         : tjeu.vreeburg@gmail.com

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private Transform head;

    [HideInInspector]
    public GameObject puffEffect;
    [HideInInspector]
    public float health = 10.0f;
    [HideInInspector]
    public float damage = 2.0f;
    [HideInInspector]
    public bool nextReturnFalse = false;
    protected bool delayedDeathCalled = false;
    protected float delayedDeathTimer = 0f;
    protected float avoidForce = 0.05f;
    protected Structure target = null;
    [HideInInspector]
    public Soldier defenseTarget = null;
    protected EnemyState enemyState = EnemyState.Idle;
    protected List<GameObject> enemiesInArea = new List<GameObject>();
    protected bool needToMoveAway;
    protected float finalSpeed = 0.0f;
    protected Animator animator;
    protected bool action = false;

    // Stun
    protected bool stunned = false;
    protected float stunTime = 1.0f;
    protected float stunCurrentTime = 0.0f;


    protected Rigidbody body;
    protected List<StructureType> structureTypes;
    protected bool defending = false;
    protected int observers = 0;
    protected bool hasPath = false;
    protected EnemyPath path;
    private EnemyPathSignature signature;

    protected Vector3 velocity;
    protected Vector3 lastPosition;

    public abstract void Action();

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        finalSpeed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SwiftFootwork) ? 1.4f : 1.0f;
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        signature = new EnemyPathSignature()
        {
            startTile = null,
            validStructureTypes = structureTypes
        };
    }

    public void Stun(float _stunDuration)
    {
        stunned = true;
        stunCurrentTime = _stunDuration;
        animator.SetBool("Attack", false);
        animator.SetBool("Walk", false);
    }

    public virtual void OnKill()
    {
        EnemyManager.GetInstance().OnEnemyDeath(this);
    }

    public virtual void OnDamagedBySoldier(Soldier _soldier)
    {
        enemyState = EnemyState.Action;
        defenseTarget = _soldier;
        defending = true;
        animator.SetBool("Attack", true);
        action = true;
        LookAtPosition(_soldier.transform.position);
    }

    public void ForgetSoldier()
    {
        defenseTarget = null;
        defending = false;
        action = false;
        enemyState = EnemyState.Idle;
        animator.SetBool("Attack", false);
    }

    protected virtual void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.right = transform.forward;
    }

    protected virtual void Update()
    {
        velocity = (transform.position - lastPosition) * (1 / Time.deltaTime);
        lastPosition = transform.position;

        if (stunned)
        {
            stunCurrentTime -= Time.deltaTime;
            if(stunTime <= 0.0f)
            {
                stunned = false;
                animator.SetBool("Walk", true);
            }
        }

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

        // update signature
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            signature.startTile = hit.transform.GetComponent<TileBehaviour>();
        }
    }

    public void RequestNewPath()
    {
        // if the spawner returns true, a valid path was found...
        if (PathManager.GetInstance().RequestPath(signature, ref path))
        {
            hasPath = true;
            target = path.target;
            enemyState = EnemyState.Walk;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Enemy>())
        {
            if (!enemiesInArea.Contains(other.gameObject)) enemiesInArea.Add(other.gameObject);
        }
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

    protected Vector3 GetNextPositionPathFollow()
    {
        // follow the path
        // move towards the first element in the path, if you get within 0.25 units, delete the element from the path
        Vector3 nextPathPoint = path.pathPoints[0];
        nextPathPoint.y = transform.position.y;
        float distanceToNextPathPoint = (transform.position - nextPathPoint).magnitude;
        if (distanceToNextPathPoint < 0.25f)
        {
            // delete the first element in the path
            path.pathPoints.RemoveAt(0);
        }
        return transform.position + (GetPathVector() * Time.fixedDeltaTime);
    }

    protected Vector3 GetPathVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = path.pathPoints[0] - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
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
        }
        return finalMotionVector.normalized * finalSpeed;
    }

    protected Vector3 GetMotionVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
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

    public bool Damage(float _damage)
    {
        health -= _damage;
        if (health <= 0f)
        {
            OnKill();
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public void AddObserver()
    {
        observers++;
    }

    public void RemoveObserver()
    {
        observers--;
    }

    public bool IsBeingObserved()
    {
        return observers > 0;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public Transform GetHead()
    {
        return head;
    }
}
