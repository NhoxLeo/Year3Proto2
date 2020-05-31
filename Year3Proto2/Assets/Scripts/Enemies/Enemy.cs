using UnityEngine;
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
    protected Structure target = null;
    protected EnemyState enemyState = EnemyState.IDLE;
    protected List<GameObject> enemiesInArea = new List<GameObject>();
    public GameObject puffEffect;

    protected bool action = false;

    public float health = 10.0f;
    public float avoidForce = 1.0f;
    public float speed = 0.6f;
    public float scale = 0.0f;
    public float damage = 2.0f;
    public bool nextReturnFalse = false;
    protected bool needToMoveAway;

    protected float yPosition;
    protected float finalSpeed = 0.0f;
    protected float jumpHeight = 0.0f;

    protected List<StructureType> structureTypes;

    public abstract void Action(Structure structure, float damage);

    private Rigidbody body;

    protected void EnemyStart()
    {
        body = GetComponent<Rigidbody>();
        SetScale(Random.Range(0.1f, 0.2f));
        speed *= SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.k_iSwiftFootwork) ? 1.4f : 1.0f;
    }

    public void SetScale(float _scale)
    {
        scale = _scale;
        jumpHeight = _scale;
        yPosition = transform.position.y;

        transform.localScale = new Vector3(_scale, _scale, _scale);
        damage = _scale * 20.0f;
        health = _scale * 75.0f;
        finalSpeed = speed + _scale / 4.0f;
    }

    public void OnKill()
    {
        FindObjectOfType<EnemySpawner>().OnEnemyDeath(GetComponent<Enemy>());
        Instantiate(puffEffect, transform.position, Quaternion.identity);
    }

    private void FixedUpdate()
    {

        switch (enemyState)
        {
            case EnemyState.ACTION:
                if (!target)
                {
                    action = false;
                    enemyState = EnemyState.IDLE;
                }
                else
                {
                    if (needToMoveAway)
                    {
                        if ((target.transform.position - transform.position).magnitude < (scale * 2f) + 0.5f)
                        {
                            Vector3 newPosition = transform.position - (GetMotionVector() * Time.fixedDeltaTime);
                            transform.LookAt(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            needToMoveAway = false;
                        }
                    }
                    else
                    {
                        Action(target, damage);
                    }
                }
                break;
            case EnemyState.WALK:
                if (target)
                {
                    if (target.attachedTile)
                    {
                        if (!Next()) { target = null; }
                    }

                    // get the motion vector for this frame
                    Vector3 newPosition = transform.position + (GetMotionVector() * Time.fixedDeltaTime);
                    transform.LookAt(newPosition);
                    transform.position = newPosition;

                    // if we are close enough to the target, attack the target
                    if ((target.transform.position - transform.position).magnitude <= (scale * 2f) + 0.5f)
                    {
                        enemyState = EnemyState.ACTION;
                        needToMoveAway = (target.transform.position - transform.position).magnitude < (scale * 2f) + 0.45f;
                        transform.DOKill(false);
                        transform.DOMoveY(yPosition, 0.25f);
                    }


                    //transform.position += transform.forward * finalSpeed * Time.deltaTime;

                    /*
                    enemiesInArea.RemoveAll(enemy => enemy == null);
                    foreach (GameObject enemy in enemiesInArea)
                    {
                        if (Vector3.Distance(enemy.transform.position, transform.position) < (scale + 0.2f))
                        {
                            transform.position = (transform.position - enemy.transform.position).normalized * (scale + 0.2f) + enemy.transform.position;
                        }
                    }
                    */

                }
                else
                {
                    enemyState = EnemyState.IDLE;
                }
                break;
            case EnemyState.IDLE:

                transform.DOKill(false);
                transform.DOMoveY(yPosition + jumpHeight, finalSpeed / 3.0f).SetLoops(-1, LoopType.Yoyo);

                if (!Next()) { target = null; }

                if (!target)
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
                if (structure.attachedTile)
                {
                    Vector3 directionToTarget = structure.transform.position - currentPosition;
                    float dSqrToTarget = directionToTarget.sqrMagnitude;
                    if (dSqrToTarget < closestDistanceSqr)
                    {
                        closestDistanceSqr = dSqrToTarget;
                        //transform.LookAt(structure.transform);

                        enemyState = EnemyState.WALK;
                        action = false;

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

    private Vector3 GetMotionVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = target.transform.position - transform.position;
        Vector3 finalMotionVector = toTarget;
        enemiesInArea.RemoveAll(enemy => !enemy);
        foreach (GameObject enemy in enemiesInArea)
        {
            // get a vector pointing from them to me, indicating a direction for this enemy to push 
            Vector3 enemyToThis = transform.position - enemy.transform.position;
            float inverseMag = 1f / enemyToThis.magnitude;
            finalMotionVector += enemyToThis.normalized * inverseMag * avoidForce;
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
