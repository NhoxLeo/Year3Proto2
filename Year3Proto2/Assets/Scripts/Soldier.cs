using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    public struct SoldierPath
    {
        public List<Vector3> pathPoints;
        public Enemy target;
    }

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
    public bool returnHome;
    public int state = 0; // 0 idle, 1 moving, 2 attacking
    public int barracksID;
    private float searchTimer = 0f;
    private float searchDelay = 0.3f;
    private float avoidance = 0.05f;
    private List<Soldier> nearbySoldiers = new List<Soldier>();
    public bool hasPath = false;
    Soldier followTarget = null;
    protected SoldierPath path;

    public static SoldierPath GetPath(Vector3 _startPoint, ref bool _enemyFound)
    {
        TileBehaviour startTile = null;
        if (Physics.Raycast(_startPoint + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            TileBehaviour tile = hit.transform.GetComponent<TileBehaviour>();
            if (tile)
            {
                startTile = tile;
            }
        }
        SoldierPath path = new SoldierPath();

        // get the closest observed enemy to the _startPoint
        _enemyFound = false;

        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length > 0)
        {
            float distance = Mathf.Infinity;
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsBeingObserved())
                {
                    _enemyFound = true;
                    float thisDistance = (enemy.transform.position - _startPoint).magnitude;
                    if (thisDistance < distance)
                    {
                        distance = thisDistance;
                        path.target = enemy;
                    }
                }
            }
        }

        if (_enemyFound && startTile)
        {
            float startTime = Time.realtimeSinceStartup;

            // find a path to the enemy

            // we have our destination and our source, now use A* to find the path
            // generate initial open and closed lists
            List<PathfindingTileData> open = new List<PathfindingTileData>();
            List<PathfindingTileData> closed = new List<PathfindingTileData>();
            TileBehaviour destination = null;
            if (Physics.Raycast(path.target.transform.position + Vector3.up, Vector3.down, out RaycastHit hitGround, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                destination = hitGround.transform.GetComponent<TileBehaviour>();
            }
            // add the tiles next to the start tile to the open list and calculate their costs
            PathfindingTileData startingData = new PathfindingTileData()
            {
                tile = startTile,
                fromTile = startTile,
                gCost = 0f,
                hCost = EnemySpawner.CalculateHCost(startTile, destination)
            };

            EnemySpawner.ProcessTile(startingData, open, closed, destination);

            // while a path hasn't been found
            bool pathFound = false;
            int lapCount = 0;
            while (!pathFound && open.Count > 0 && lapCount < 20000)
            {
                lapCount++;
                if (EnemySpawner.ProcessTile(EnemySpawner.GetNextOpenTile(open), open, closed, destination))
                {
                    // generate a path from the tiles in the closed list
                    // path from the source tile to the destination tile
                    // find the destination tile in the closed list
                    List<Vector3> reversePath = new List<Vector3>();
                    PathfindingTileData currentData = closed[closed.Count - 1];
                    while (currentData.fromTile != currentData.tile)
                    {
                        reversePath.Add(currentData.tile.transform.position);
                        currentData = EnemySpawner.FollowFromTile(closed, currentData.fromTile);
                    }
                    reversePath.Reverse();
                    path.pathPoints = reversePath;
                    pathFound = true;
                }
            }

            float finishTime = Time.realtimeSinceStartup;
            Debug.Log("Soldier Pathfinding complete, took " + (finishTime - startTime).ToString() + " seconds");
        }
        return path;
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
        if (state != 3) { Idle(); }

        switch (state)
        {
            case 0:
                if (home)
                {
                    Vector3 toHome = home.transform.position - transform.position;
                    toHome.y = 0f;
                    if (toHome.magnitude > 0.25f)
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
                else
                {
                    // stop walking animation
                    Vector3 avoidance = GetAvoidanceOnly();
                    if (avoidance == Vector3.zero)
                    {
                        animator.SetInteger("State", 0);
                    }
                    else
                    {
                        Vector3 futurePos = transform.position + (avoidance * Time.fixedDeltaTime);
                        transform.position = futurePos;
                        animator.SetInteger("State", 1);
                    }
                }
                break;
            case 1:
                animator.SetInteger("State", state);
                canHeal = false;
                if (!target)
                {
                    state = 0;
                    Idle();
                }
                else
                {
                    float distanceFromTarget = (target.transform.position - transform.position).magnitude;
                    // if you are further than 1.0f from target
                    if (distanceFromTarget > 1.0f)
                    {
                        // if you have a path, follow the path, otherwise follow the follow target
                        if (hasPath)
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
                                if (path.pathPoints.Count < 1)
                                {
                                    FindEnemy();
                                }
                            }
                            Vector3 newPosition = transform.position + (GetPathVector() * Time.fixedDeltaTime);
                            LookAtPosition(newPosition);
                            transform.position = newPosition;
                        }
                        else
                        {
                            Vector3 toFollowTarget = target.transform.position - transform.position;
                            toFollowTarget.y = 0f;
                            if (toFollowTarget.magnitude > 0.2f)
                            {
                                LookAtPosition(followTarget.transform.position);
                                transform.position += GetMotionToFollowTarget() * Time.fixedDeltaTime;
                            }
                        }
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
                            hasPath = false;
                            followTarget = null;
                        }
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
                    Idle();
                }
                break;
            case 3:
                animator.SetInteger("State", 1);
                canHeal = false;
                if (!home)
                {
                    state = 0;
                    break;
                }
                else
                {
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
                        canHeal = false;
                        animator.SetInteger("State", 1);
                    }
                    else
                    {
                        Damage(health);
                    }
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
        bool enemyFound = false;
        SoldierPath newPath = GetPath(transform.position, ref enemyFound);
        if (enemyFound)
        {
            path = newPath;
            hasPath = true;
        }
    }

    private void Idle()
    {
        searchTimer += Time.fixedDeltaTime;
        if (searchTimer >= searchDelay)
        {
            searchTimer = 0f;

            bool enemyFound = false;
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            if (enemies.Length > 0)
            {
                foreach (Enemy enemy in enemies)
                {
                    if (enemy.IsBeingObserved())
                    {
                        enemyFound = true;
                        break;
                    }
                }
            }

            if (enemyFound)
            {
                state = 1;
                // try to find a soldier around who has a path
                foreach (Soldier soldier in FindObjectsOfType<Soldier>())
                {
                    if ((soldier.transform.position - transform.position).magnitude < 1.5f)
                    {
                        if (soldier.hasPath)
                        {
                            followTarget = soldier;
                            target = soldier.path.target;
                            break;
                        }
                    }
                }
                if (followTarget == null)
                {
                    FindEnemy();
                }
            }

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

    protected Vector3 GetMotionToFollowTarget()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = followTarget.transform.position - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
            finalMotionVector += GetAvoidanceForce();
        }
        return finalMotionVector.normalized * movementSpeed;
    }


    protected Vector3 GetPathVector()
    {
        // Get the vector between this enemy and the target
        Vector3 toTarget = path.pathPoints[0] - transform.position;
        toTarget.y = 0f;
        Vector3 finalMotionVector = toTarget;
        if (toTarget.magnitude > 1.5f)
        {
             finalMotionVector += GetAvoidanceOnly();
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
