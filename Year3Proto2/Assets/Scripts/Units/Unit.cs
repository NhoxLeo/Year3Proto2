using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum UnitState { IDLE,  WALK, ACTION /*COMMAND (Alpha Stage)*/ }
public enum UnitTarget { STRUCTURE, UNIT, BOTH }
public enum UnitType { PLAYER, ENEMY, NEUTRAL }

public interface IDamageable
{
    float GetHealth();
    bool Damage(float _amount);
    Transform GetTransform();
}

[Serializable]
public struct UnitProperties
{
    public float health;
    public float speed;
    public float scale;
    public float damage;
    public float jumpHeight;
    public float avoidForce;
}

public class Unit : MonoBehaviour, IDamageable
{
    [HideInInspector]
    public GameObject puffEffect;

    public Transform targetTransform;
    public IDamageable target;

    private bool delayedDeathCalled = false;
    private bool needToMoveAway;
    private float delayedDeathTimer = 0f;

    protected bool action;

    protected Animator animator;
    protected List<Transform> surroundings = new List<Transform>();
    protected StructureType[] structureTypes = new StructureType[] { };
    protected UnitTarget unitTarget = UnitTarget.BOTH;
    protected UnitType unitType = UnitType.NEUTRAL;
    protected UnitState unitState = UnitState.IDLE;
    public UnitProperties unitProperties;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        unitProperties.speed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.k_iSwiftFootwork) ? 1.4f : 1.0f;
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }

    private void FixedUpdate()
    {
        if (GlobalData.longhausDead)
        {
            animator.SetBool("Attack", false);
            return;
        }

        switch(unitState)
        {
            case UnitState.ACTION:
                if (targetTransform)
                {
                    if (needToMoveAway)
                    {
                        if (IsClose(0.5f))
                        {
                            Vector3 newPosition = transform.position - (GetMotionVector() * Time.fixedDeltaTime);
                            LookAtPosition(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            needToMoveAway = false;
                        }
                    }
                    else
                    {
                        action = true;
                    }
                }
                else
                {
                    animator.SetBool("Attack", false);
                    unitState = UnitState.IDLE;
                }
                break;
            case UnitState.WALK:
                if (targetTransform)
                {
                    Vector3 newPosition = transform.position + (GetMotionVector() * Time.fixedDeltaTime);
                    LookAtPosition(newPosition);
                    transform.position = newPosition;

                    if (IsClose(0.7f))
                    {
                        animator.SetBool("Attack", true);
                        transform.LookAt(targetTransform);
                        // fixing animation problems
                        transform.right = transform.forward;
                        needToMoveAway = IsClose(0.5f);
                        unitState = UnitState.ACTION;
                        if (needToMoveAway) animator.SetBool("Attack", false);
                    }
                }
                else
                {
                    animator.SetBool("Attack", false);
                    unitState = UnitState.IDLE;
                }
                break;
        }
    }


    private bool IsClose(float _scalar)
    {
        return (targetTransform.position - transform.position).magnitude < _scalar;
    }


    private void Update()
    {
        if (GlobalData.longhausDead)
        {
            if (!delayedDeathCalled)
            {
                delayedDeathCalled = true;
                delayedDeathTimer = UnityEngine.Random.Range(0.5f, 3.5f);
            }
            delayedDeathTimer -= Time.deltaTime;
            if (delayedDeathTimer <= 0f) Damage(unitProperties.health);
        }

        if (unitState == UnitState.IDLE)
        {
            IDamageable[] damageables = SearchForTarget(unitTarget);
            if (damageables.Length > 0)
            {
                IDamageable damageable = GetClosest(damageables);
                if (damageable != null)
                {
                    target = damageable;
                    targetTransform = damageable.GetTransform();
                    unitState = UnitState.WALK;
                }
            }

        }
    }

    protected virtual void OnKill()
    {
        FindObjectOfType<UnitSpawner>().OnDeath(this);
        GameObject puff = Instantiate(puffEffect, transform.position, Quaternion.identity);
        puff.transform.localScale *= unitProperties.scale * 1.5f;
    }

    public void SwingContact()
    {
        if (targetTransform && action)
        {
            if (target.Damage(unitProperties.damage))
            {
                Destroy(targetTransform.gameObject);
                animator.SetBool("Attack", false);
                unitState = UnitState.IDLE;

                action = false;
                target = null;
            }
        }
    }

    protected virtual void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.right = transform.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Alpha improvements (Change to layer mask.)
        if (other.GetComponent<Unit>() != null)
        {
            if (!surroundings.Contains(other.transform)) surroundings.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Alpha improvements (Change to layer mask.)
        if (other.GetComponent<Unit>() != null)
        {
            if (surroundings.Contains(other.transform)) surroundings.Remove(other.transform);
        }
    }

    protected Vector3 GetMotionVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = targetTransform.position - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        bool enemyWasNull = false;

        foreach (Transform transform in surroundings)
        {
            if (!transform)
            {
                enemyWasNull = true;
                continue;
            }

            // get a vector pointing from them to me, indicating a direction for this enemy to push 
            Vector3 enemyToThis = this.transform.position - transform.transform.position;
            enemyToThis.y = 0f;
            float inverseMag = 1f / enemyToThis.magnitude;
            if (inverseMag == Mathf.Infinity) { continue; }
            finalMotionVector += enemyToThis.normalized * inverseMag * unitProperties.avoidForce;
        }

        if (enemyWasNull) surroundings.RemoveAll(enemy => !enemy);
        return finalMotionVector.normalized * unitProperties.speed;
    }

    private IDamageable[] SearchForTarget(UnitTarget _unitTarget)
    {
        List<IDamageable> damageables = new List<IDamageable>();

        if (_unitTarget == UnitTarget.STRUCTURE || unitTarget == UnitTarget.BOTH)
        {
            foreach (Structure structure in FindObjectsOfType<Structure>())
            {
                if (structureTypes.Contains(structure.GetStructureType()))
                {
                    if (structure.attachedTile != null) damageables.Add(structure);
                }
            }
        }

        if (_unitTarget == UnitTarget.UNIT || unitTarget == UnitTarget.BOTH)
        {
            foreach (Unit unit in FindObjectsOfType<Unit>())
            { 
                if (unit.GetType() != unitType) damageables.Add(unit);
            }
        }

        return damageables.ToArray();
    }

    private IDamageable GetClosest(IDamageable[] damageables)
    {
        IDamageable bestTarget = null;

        float closestDistanceSquared = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (IDamageable damageable in damageables)
        {
            Vector3 direction = damageable.GetTransform().position - position;
            float distanceSquared = direction.sqrMagnitude;
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                bestTarget = damageable;
            }
        }

        return bestTarget;
    }

    public virtual void SetScale(float _scale)
    {
        unitProperties.scale = _scale;
        unitProperties.jumpHeight = _scale;

        transform.localScale = new Vector3(_scale, _scale, _scale);
        unitProperties.damage = _scale * 20.0f;
        unitProperties.health = _scale * 75.0f;
        unitProperties.speed = unitProperties.speed + _scale / 4.0f;
    }

    public void SetTarget(IDamageable _damageable)
    {
        target = _damageable;
    }

    public void SetState(UnitState _unitState)
    {
        unitState = _unitState;
    }

    public void SetUnitType(UnitType _unitType)
    {
        unitType = _unitType;
    }

    public IDamageable GetTarget()
    {
        return target;
    }

    public UnitTarget GetUnitTarget()
    {
        return unitTarget;
    }

    public StructureType[] GetStructureTypes()
    {
        return structureTypes;
    }

    public UnitState GetUnitState()
    {
        return unitState;
    }

    public new UnitType GetType()
    {
        return unitType;
    }

    public float GetHealth()
    {
        return unitProperties.health;
    }

    public bool Damage(float _amount)
    {
        unitProperties.health -= _amount;
        if (unitProperties.health <= 0.0f)
        {
            FindObjectOfType<UnitSpawner>().OnDeath(this);
            Instantiate(puffEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
