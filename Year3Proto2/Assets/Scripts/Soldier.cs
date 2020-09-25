using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Soldier : MonoBehaviour
{

    public static Enemy GetEnemy(Transform enemy)
    {
        return enemy.GetComponent<Enemy>();
    }

    private static Converter<Transform, Enemy> ToEnemyConverter = new Converter<Transform, Enemy>(GetEnemy);

    private static float MaxHealth = 18.0f;
    private static float Damage = 3.0f;
    private static float MovementSpeed = 0.5f;
    // 0 idle, 1 moving, 2 attacking, 3 returning home
    // animation states line up (0, 1, 2), with moving state being used for state 3
    private int state = 0;
    private Enemy target = null;
    private Animator animator;
    private static GameObject PuffEffect;
    private float health;
    private bool canHeal = false;
    private float healRate = 0.5f;
    private Barracks home;
    private bool returnHome;
    private int ID;
    private int barracksID;
    private float searchTimer = 0f;
    private float searchDelay = 0.3f;
    private float avoidance = 0.05f;
    private List<Soldier> nearbySoldiers = new List<Soldier>();
    private SoldierPath path;
    private bool waitingOnPath = false;
    private bool haveHomePath = false;

    public TileBehaviour GetCurrentTile()
    {
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            return hit.transform.GetComponent<TileBehaviour>();
        }
        return null;
    }

    public Enemy GetClosestEnemy()
    {
        Enemy closest = null;
        List<Enemy> barracksEnemies = home.GetEnemies().ConvertAll(ToEnemyConverter);
        if (barracksEnemies.Count > 0)
        {
            float distance = Mathf.Infinity;
            foreach (Enemy enemy in barracksEnemies)
            {
                if (enemy.IsBeingObserved())
                {
                    float thisDistance = (enemy.transform.position - transform.position).magnitude;
                    if (thisDistance < distance)
                    {
                        distance = thisDistance;
                        closest = enemy;
                    }
                }
            }
        }
        return closest;
    }

    public TileBehaviour GetHomeTile()
    {
        return home.attachedTile;
    }

    private void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.forward = transform.right;
    }

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // if the soldier has been recalled
        if (returnHome) { state = 3; }
        // if the soldier was responding to a recall but no longer needs to be recalled
        if (state == 3 && !returnHome) { state = 0; }
        // if the soldier is not responding to a recall
        if (state == 0 || state == 1) { SearchForEnemies(); }

        if (!home)
        {
            ApplyDamage(health);
            return;
        }

        switch (state)
        {
            case 0:
                UpdateWaitForTarget();
                break;
            case 1:
                UpdateMoveTowardsTarget();
                break;
            case 2:
                UpdateAttackTarget();
                break;
            case 3:
                UpdateRecall();
                break;
            default:
                break;
        }
        if (canHeal)
        {
            if (health != MaxHealth)
            {
                health += healRate * Time.fixedDeltaTime;
                if (health > MaxHealth) { health = MaxHealth; }
            }
        }
    }

    private void UpdateWaitForTarget()
    {
        Vector3 toHome = home.transform.position - transform.position;
        toHome.y = 0f;
        // if you are further than 1.0f from target
        if (toHome.magnitude > 0.5f)
        {
            animator.SetInteger("State", 1);
            if (!haveHomePath)
            {
                haveHomePath = PathManager.GetInstance().RequestPath(this, ref path, true);
            }
            else
            {
                if (path.pathPoints.Count > 0)
                {
                    // move towards the first element in the path, if you get within 0.25 units, delete the element from the path
                    Vector3 nextPathPoint = path.pathPoints[0];
                    nextPathPoint.y = transform.position.y;
                    float distanceToNextPathPoint = (transform.position - nextPathPoint).magnitude;
                    if (distanceToNextPathPoint < 0.25f)
                    {
                        // delete the first element in the path
                        path.pathPoints.RemoveAt(0);
                        if (path.pathPoints.Count < 1)
                        {
                            haveHomePath = false;
                        }
                    }
                    Vector3 newPosition = transform.position + (GetPathVector() * Time.fixedDeltaTime);
                    LookAtPosition(newPosition);
                    transform.position = newPosition;
                }
            }
        }
        // else go straight to target
        else
        {
            haveHomePath = false;
            if (toHome.magnitude < 0.25f)
            {
                Vector3 avoidance = GetAvoidanceOnly();
                if (avoidance == Vector3.zero)
                {
                    animator.SetInteger("State", 0);
                }
                else
                {
                    Vector3 futurePos = transform.position + (avoidance * Time.fixedDeltaTime);
                    LookAtPosition(home.transform.position);
                    transform.position = futurePos;
                    animator.SetInteger("State", 1);
                }
                canHeal = true;
            }
            else
            {
                LookAtPosition(home.transform.position);
                transform.position += GetMotionToTarget(home.transform.position) * Time.fixedDeltaTime;
                canHeal = false;
                animator.SetInteger("State", 1);
            }
        }
    }

    private void UpdateMoveTowardsTarget()
    {
        animator.SetInteger("State", state);
        canHeal = false;
        if (!target)
        {
            state = 0;
            return;
        }
        float distanceFromTarget = (target.transform.position - transform.position).magnitude;
        // if you are further than 1.0f from target
        if (distanceFromTarget > 1.0f && path.pathPoints.Count > 0)
        {
            // move towards the first element in the path, if you get within 0.25 units, delete the element from the path
            Vector3 nextPathPoint = path.pathPoints[0];
            nextPathPoint.y = transform.position.y;
            float distanceToNextPathPoint = (transform.position - nextPathPoint).magnitude;
            if (distanceToNextPathPoint < 0.25f)
            {
                // delete the first element in the path
                path.pathPoints.RemoveAt(0);
                if (path.pathPoints.Count < 1)
                {
                    state = 0;
                }
            }
            Vector3 newPosition = transform.position + (GetPathVector() * Time.fixedDeltaTime);
            LookAtPosition(newPosition);
            transform.position = newPosition;
        }
        // else go straight to target
        else
        {
            LookAtPosition(target.transform.position);
            transform.position += GetMotionToTarget(target.transform.position) * Time.fixedDeltaTime;
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.magnitude < 0.2f)
            {
                state = 2;
            }
        }
    }

    private void UpdateAttackTarget()
    {
        animator.SetInteger("State", state);
        canHeal = false;
        if (target)
        {
            LookAtPosition(target.transform.position);
        }
        else
        {
            state = 0;
        }
    }

    private void UpdateRecall()
    {
        animator.SetInteger("State", 1);
        canHeal = false;
        if (target)
        {
            target.ForgetSoldier();
        }
        Vector3 toHome = home.transform.position - transform.position;
        toHome.y = 0f;
        if (toHome.magnitude > 0.8f)
        {
            LookAtPosition(home.transform.position);
            transform.position += GetMotionToTarget(home.transform.position) * Time.fixedDeltaTime;
            animator.SetInteger("State", 1);
        }
        else
        {
            ApplyDamage(health);
        }
    }

    private void Awake()
    {
        health = MaxHealth;
        if (!PuffEffect)
        {
            PuffEffect = Resources.Load("EnemyPuffEffect") as GameObject;
        }
    }

    private void SearchForEnemies()
    {
        if (waitingOnPath)
        {
            if (PathManager.GetInstance().RequestPath(this, ref path))
            {
                target = path.target;
                state = 1;
                waitingOnPath = false;
                haveHomePath = false;
            }
        }
        else
        {
            searchTimer += Time.fixedDeltaTime;
            if (searchTimer >= searchDelay)
            {
                searchTimer = 0f;
                if (GetClosestEnemy())
                {
                    waitingOnPath = true;
                }
            }
        }


    }

    public bool ApplyDamage(float _damage)
    {
        health -= _damage;
        if (health <= 0f)
        {
            if (target)
            {
                target.ForgetSoldier();
            }
            if (home)
            {
                /*
                if (home.soldiers.Contains(this))
                {
                    home.soldiers.Remove(this);
                }
                */
            }
            GameObject puff = Instantiate(PuffEffect);
            puff.transform.position = transform.position;
            puff.transform.localScale *= 2f;
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public void SwingContact()
    {
        if (target && state == 2)
        {
            target.OnDamagedBySoldier(this);
            if (target.Damage(Damage))
            {
                target = null;
            }
            else
            {
                if ((target.transform.position - transform.position).magnitude > 0.2f)
                {
                    state = 1;
                }
            }
        }
    }

    private Vector3 GetAvoidanceOnly()
    {
        Vector3 avoidanceForce = GetAvoidanceForce();
        if (avoidanceForce != Vector3.zero)
        {
            return avoidanceForce.normalized * MovementSpeed;
        }
        return avoidanceForce;
    }

    private Vector3 GetMotionToTarget(Vector3 _target)
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = _target - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
            finalMotionVector += GetAvoidanceForce();
        }
        return finalMotionVector.normalized * MovementSpeed;
    }

    private Vector3 GetPathVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = path.pathPoints[0] - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
             finalMotionVector += GetAvoidanceOnly();
        }
        return finalMotionVector.normalized * MovementSpeed;
    }

    private Vector3 GetAvoidanceForce()
    {
        Vector3 finalMotionVector = Vector3.zero;
        bool soldierWasNull = false;
        foreach (Soldier soldier in nearbySoldiers)
        {
            if (!soldier)
            {
                soldierWasNull = true;
                continue;
            }
            // get a vector pointing from them to me, indicating a direction for this soldier to move
            Vector3 soldierToThis = transform.position - soldier.transform.position;
            soldierToThis.y = 0f;
            float inverseMag = 1f / soldierToThis.magnitude;
            if (inverseMag == Mathf.Infinity) { continue; }
            /*
            // move the soldier a bit away also
            Vector3 newSoldierPosition = soldier.transform.position + soldierToThis * avoidance * -0.05f;
            soldier.transform.position = newSoldierPosition;
            */
            finalMotionVector += soldierToThis.normalized * inverseMag * avoidance;
        }
        if (soldierWasNull)
        {
            nearbySoldiers.RemoveAll(soldier => !soldier);
        }
        if (finalMotionVector.magnitude < 0.25f)
        {
            return Vector3.zero;
        }
        return finalMotionVector;
    }

    private void OnTriggerEnter(Collider other)
    {
        Soldier otherSoldier = other.GetComponent<Soldier>();
        if (otherSoldier)
        {
            if (!nearbySoldiers.Contains(otherSoldier)) nearbySoldiers.Add(otherSoldier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Soldier otherSoldier = other.GetComponent<Soldier>();
        if (otherSoldier)
        {
            if (nearbySoldiers.Contains(otherSoldier)) nearbySoldiers.Remove(otherSoldier);
        }
    }

    public void SetBarracksID(int _ID)
    {
        barracksID = _ID;
    }

    public int GetBarracksID()
    {
        return barracksID;
    }

    public void SetReturnHome(bool _return)
    {
        returnHome = _return;
    }

    public bool GetReturnHome()
    {
        return returnHome;
    }

    public void SetID(int _ID)
    {
        ID = _ID;
    }

    public int GetID()
    {
        return ID;
    }

    public void SetState(int _state)
    {
        state = _state;
    }

    public int GetState()
    {
        return state;
    }

    public void SetHome(Barracks _home)
    {
        home = _home;
    }

    public void SetHealRate(float _rate)
    {
        healRate = _rate;
    }

    public void SetHealth(float _health)
    {
        health = _health;
    }

    public float GetHealth()
    {
        return health;
    }

    public static void SetMaxHealth(float _newMax)
    {
        MaxHealth = _newMax;
    }

    public static float GetMaxHealth()
    {
        return MaxHealth;
    }

    public static void SetDamage(float _damage)
    {
        Damage = _damage;
    }

    public static float GetDamage()
    {
        return Damage;
    }

    public static void SetMovementSpeed(float _speed)
    {
        MovementSpeed = _speed;
    }

    public static float GetMovementSpeed()
    {
        return MovementSpeed;
    }
}
