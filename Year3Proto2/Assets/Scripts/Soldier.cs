using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private Enemy target = null;
    private Animator animator;
    [HideInInspector]
    public GameObject puffEffect;
    [HideInInspector]
    public float health = 18.0f;
    [HideInInspector]
    public float maxHealth = 18.0f;
    [HideInInspector]
    public float damage = 3.0f;
    [HideInInspector]
    public float movementSpeed = 0.5f;
    [HideInInspector]
    public bool canHeal = false;
    public float healRate = 0.5f;
    public Barracks home;
    public int state = 0; // 0 idle, 1 moving, 2 attacking
    private float searchTimer = 0f;
    private float searchDelay = 0.3f;
    private float avoidance = 0.05f;
    private List<Soldier> nearbySoldiers = new List<Soldier>();

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
        Idle();
        switch (state)
        {
            case 0:
                if (home)
                {
                    Vector3 toHome = home.transform.position - transform.position;
                    toHome.y = 0f;
                    if (toHome.magnitude > 0.7f)
                    {
                        LookAtPosition(home.transform.position);
                        transform.position += GetMotionToTarget(home.transform.position) * Time.fixedDeltaTime;
                        canHeal = false;
                        animator.SetInteger("State", 1);
                    }
                    else
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
                }
                break;
            case 1:
                animator.SetInteger("State", state);
                canHeal = false;
                if (!target)
                {
                    state = 0;
                    FindEnemy();
                }
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
                break;
            case 2:
                animator.SetInteger("State", state);
                canHeal = false;
                if (target)
                {
                    LookAtPosition(target.transform.position);
                }
                if (!target)
                {
                    state = 0;
                    FindEnemy();
                }
                break;
            default:
                break;
        }
        if (canHeal)
        {
            if (health != maxHealth)
            {
                health += healRate * Time.fixedDeltaTime;
                if (health > maxHealth) { health = maxHealth; }
            }
        }
    }

    private void FindEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length > 0)
        {
            float distance = Mathf.Infinity;
            foreach (Enemy enemy in enemies)
            {
                float thisDistance = (enemy.transform.position - transform.position).magnitude;
                if (thisDistance < distance)
                {
                    distance = thisDistance;
                    target = enemy;
                    state = 1;
                }
            }
        }
    }

    private void Update()
    {
        
    }

    private void Idle()
    {
        searchTimer += Time.fixedDeltaTime;
        if (searchTimer >= searchDelay)
        {
            searchTimer = 0f;
            FindEnemy();
        }
    }

    public bool Damage(float _damage)
    {
        health -= _damage;
        if (health <= 0f)
        {
            if (home)
            {
                if (home.soldiers.Contains(this))
                {
                    home.soldiers.Remove(this);
                }
            }
            GameObject puff = Instantiate(puffEffect);
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
            if (target.Damage(damage))
            {
                target = null;
            }
        }
    }

    protected Vector3 GetAvoidanceOnly()
    {
        Vector3 avoidanceForce = GetAvoidanceForce();
        if (avoidanceForce != Vector3.zero)
        {
            return avoidanceForce.normalized * movementSpeed;
        }
        return avoidanceForce;
    }

    protected Vector3 GetMotionToTarget(Vector3 _target)
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = _target - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
            finalMotionVector += GetAvoidanceForce();
        }
        return finalMotionVector.normalized * movementSpeed;
    }

    protected Vector3 GetAvoidanceForce()
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
}
