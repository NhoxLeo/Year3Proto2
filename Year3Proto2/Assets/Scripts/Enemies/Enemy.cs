using UnityEngine;
using System.Collections.Generic;
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
    [HideInInspector]
    protected static GameObject PuffEffect;
    [HideInInspector]
    protected float baseHealth = 10.0f;
    [HideInInspector]
    protected float baseDamage = 2.0f;
    [HideInInspector]
    protected float health;
    [HideInInspector]
    protected float damage;
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
    protected float currentSpeed = 0.0f;
    protected float finalAttackSpeed = 0.0f;
    protected float currentAttackSpeed = 0.0f;
    protected Animator animator;
    protected bool action = false;
    [HideInInspector]
    public string enemyName;
    private int spawnWave;
    protected Rigidbody body;
    protected List<StructureType> structureTypes;
    protected bool defending = false;
    protected int observers = 0;
    protected bool hasPath = false;
    protected EnemyPath path;
    protected float updatePathTimer = 0f;
    protected float updatePathDelay = 1.5f;
    private EnemyPathSignature signature;
    protected int level;
    protected Healthbar healthbar;
    private bool onKillCalled = false;

    // Stun
    protected bool stunned = false;
    protected float stunDuration = 1.2f;
    protected float stunTime = 0.0f;

    // Slow
    private int slowCount = 0;

    public abstract void Action();

    protected virtual void Awake()
    {
        if (!PuffEffect)
        {
            PuffEffect = Resources.Load("EnemyPuffEffect") as GameObject;
        }
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        signature = new EnemyPathSignature()
        {
            startTile = null,
            validStructureTypes = structureTypes
        };
    }

    public void Stun(float _damage)
    {
        animator.enabled = false;
        stunned = true;
        stunTime = stunDuration;

        Damage(_damage);
    }

    public void Slow(bool _newSlow, float _slowAmount)
    {
        if (_newSlow)
        {
            if (slowCount == 0)
            {
                float slowMult = 0.6f * (1f / _slowAmount);
                currentSpeed = finalSpeed * slowMult;
                if (enemyName == EnemyNames.Invader || enemyName == EnemyNames.HeavyInvader)
                {
                    finalAttackSpeed = animator.GetFloat("AttackSpeed");
                    currentAttackSpeed = finalAttackSpeed * slowMult;
                    animator.SetFloat("AttackSpeed", currentAttackSpeed);
                }
            }
            slowCount++;
        }
        else
        {
            if (slowCount == 1)
            {
                currentSpeed = finalSpeed;
                if (enemyName == EnemyNames.Invader || enemyName == EnemyNames.HeavyInvader)
                {
                    animator.SetFloat("AttackSpeed", finalAttackSpeed);
                }
            }
            slowCount--;
        }

    }

    public virtual void OnKill()
    {
        EnemyManager.GetInstance().OnEnemyDeath(this);
    }

    public virtual void OnDamagedBySoldier(Soldier _soldier)
    {
        if (enemyName == EnemyNames.Invader || enemyName == EnemyNames.HeavyInvader)
        {
            enemyState = EnemyState.Action;
            defenseTarget = _soldier;
            defending = true;
            animator.SetBool("Attack", true);
            action = true;
            LookAtPosition(_soldier.transform.position);
        }
    }

    public void ForgetSoldier()
    {
        if (enemyName == EnemyNames.Invader || enemyName == EnemyNames.HeavyInvader)
        {
            defenseTarget = null;
            defending = false;
            action = false;
            enemyState = EnemyState.Idle;
            animator.SetBool("Attack", false);
        }
    }

    protected virtual void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
    }

    protected virtual void Update()
    {
        if(stunned)
        {
            stunTime -= Time.deltaTime;
            if (stunTime <= 0.0f)
            {
                stunned = false;
                animator.enabled = true;
            }
        }

        if (healthbar)
        {
            if (!GameManager.ShowEnemyHealthbars)
            {
                if (healthbar.gameObject.activeSelf)
                {
                    healthbar.gameObject.SetActive(false);
                }
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

    public bool RequestNewPath()
    {
        // if the spawner returns true, a valid path was found...
        if (PathManager.GetInstance().RequestPath(signature, ref path))
        {
            hasPath = true;
            target = path.target;
            enemyState = EnemyState.Walk;
            return true;
        }
        return false;
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
        if (toTarget.magnitude > 0.5f)
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
        return finalMotionVector.normalized * currentSpeed;
    }

    protected Vector3 GetAvoidingMotionVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = target.transform.position - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 0.85f)
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
        return finalMotionVector.normalized * currentSpeed;
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
        if (healthbar && GameManager.ShowEnemyHealthbars)
        {
            if (health < GetTrueMaxHealth())
            {
                healthbar.gameObject.SetActive(true);
                healthbar.fillAmount = health / GetTrueMaxHealth();
            }
        }

        if (health <= 0f && !onKillCalled)
        {
            if (!onKillCalled)
            {
                OnKill();
                onKillCalled = true;
                Destroy(gameObject);
            }
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

    public TileBehaviour GetCurrentTile()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            return hit.transform.GetComponent<TileBehaviour>();
        }
        return null;
    }

    public void SetSpawnWave(int _wave)
    {
        spawnWave = _wave;
    }

    public int GetSpawnWave()
    {
        return spawnWave;
    }

    public int GetLevel()
    {
        return level;
    }

    public virtual void SetLevel(int _level)
    {
        level = _level;
        damage = baseDamage * Mathf.Pow(SuperManager.ScalingFactor, _level - 1);
        health = baseHealth * Mathf.Pow(SuperManager.ScalingFactor, _level - 1);
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float _health)
    {
        health = _health;
    }

    public float GetDamage()
    {
        return damage;
    }

    public float GetTrueMaxHealth()
    {
        return baseHealth * Mathf.Pow(SuperManager.ScalingFactor, level - 1);
    }

    private void OnDestroy()
    {
        if (healthbar)
        {
            Destroy(healthbar.gameObject);
        }
    }
}
