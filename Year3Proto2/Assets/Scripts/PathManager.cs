using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using TMPro;

public struct PathfindingTileData
{
    public TileBehaviour tile;
    public TileBehaviour fromTile;
    public float hCost;
    public float gCost;
    public float FCost()
    {
        return hCost + gCost;
    }
}

public struct JSTileNeighborContainer
{
    private int north;
    private int east;
    private int south;
    private int west;
    public int this[int _key]
    {
        get => GetValue(_key);
        set => SetValue(_key, value);
    }

    private int GetValue(int _key)
    {
        switch (_key)
        {
            case 0:
                return north;
            case 1:
                return east;
            case 2:
                return south;
            case 3:
                return west;
        }
        return -1;
    }

    private void SetValue(int _key, int _value)
    {
        switch (_key)
        {
            case 0:
                north = _value;
                break;
            case 1:
                east = _value;
                break;
            case 2:
                south = _value;
                break;
            case 3:
                west = _value;
                break;
        }
    }
}

public struct JSTileData
{
    public int ID;
    public JSTileNeighborContainer adjacentTiles;
    public JSTileNeighborContainer diagonalTiles;
    public Vector3 position;
}

public struct JSPathfindingTileData
{
    public JSTileData tile;
    public JSTileData fromTile;
    public float hCost;
    public float gCost;
    public float FCost()
    {
        return hCost + gCost;
    }
}

public struct EnemyPathSignature
{
    public TileBehaviour startTile;
    public List<StructureType> validStructureTypes;

    public bool IsValid
    {
        get
        {
            // if the list has been initialized...
            if (validStructureTypes != null)
            {
                // if the list is greater than 0...
                if (validStructureTypes.Count > 0)
                {
                    // if there is a startTile...
                    if (startTile)
                    {
                        // the signature appears to be valid.
                        return true;
                    }
                }
            }
            // the signature is invalid.
            return false;
        }
    }
}

public enum SoldierPathState
{
    Uninitialized = 0,
    Working,
    Done
}

public struct SoldierPathData
{
    public Soldier caller;
    public JobHandle jobHandle;
    public JSTileData startTile;
    public JSTileData endTile;
    public SoldierPath path;
    public NativeList<Vector3> nativePath;
    public Enemy target;
}

public struct EnemyPath
{
    public List<Vector3> pathPoints;
    public Structure target;

    public EnemyPath(EnemyPath _copyFrom)
    {
        pathPoints = new List<Vector3>(_copyFrom.pathPoints); // copy
        target = _copyFrom.target; // reference
    }
}

public struct SoldierPath
{
    public List<Vector3> pathPoints;
    public Enemy target;
}

public struct FindPath : IJob
{
    private JSTileData startingTile;
    private JSTileData destinationTile;
    private readonly NativeArray<int> allTileIDs;
    private readonly NativeArray<JSTileData> allTiles;
    public NativeList<Vector3> path;

    public FindPath(JSTileData _startingTile, JSTileData _destinationTile, NativeArray<int> _allTileIDs, NativeArray<JSTileData> _allTiles, NativeList<Vector3> _path)
    {
        startingTile = _startingTile;
        destinationTile = _destinationTile;
        allTileIDs = _allTileIDs;
        allTiles = _allTiles;
        path = _path;
    }

    public void Execute()
    {
        // we have our destination and our source, now use A* to find the path
        // generate initial open and closed lists
        List<JSPathfindingTileData> open = new List<JSPathfindingTileData>();
        List<JSPathfindingTileData> closed = new List<JSPathfindingTileData>();
        // add the tiles next to the start tile to the open list and calculate their costs
        JSPathfindingTileData startingData = new JSPathfindingTileData()
        {
            tile = startingTile,
            fromTile = startingTile,
            gCost = 0f,
            hCost = JSCalculateHCost(startingTile.position, destinationTile.position)
        };

        JSProcessTile(startingData, open, closed, destinationTile);

        // while a path hasn't been found
        while (open.Count > 0)
        {
            if (JSProcessTile(JSGetNextOpenTile(open), open, closed, destinationTile))
            {
                break;
            }
        }

        // generate a path from the tiles in the closed list
        // path from the source tile to the destination tile
        // find the destination tile in the closed list
        NativeList<Vector3> reversePath = new NativeList<Vector3>(Allocator.Temp);
        //List<Vector3> reversePath = new List<Vector3>();
        JSPathfindingTileData currentData = closed[closed.Count - 1];
        while (currentData.fromTile.ID != currentData.tile.ID)
        {
            Vector3 position = currentData.tile.position;
            reversePath.Add(position);
            if (TryGetTileInList(currentData.fromTile, closed, out JSPathfindingTileData outTile))
            {
                currentData = outTile;
            }
        }
        for (int i = reversePath.Length - 1; i >= 0; i--)
        {
            path.Add(reversePath[i]);
        }
        reversePath.Dispose();
    }

    private JSTileData GetTileDataWithID(int _ID)
    {
        for (int i = 0; i < allTileIDs.Length; i++)
        {
            if (allTileIDs[i] == _ID)
            {
                return allTiles[i];
            }
        }

        // if we couldn't find a tile with that ID, return an invalid tileData to communicate that.
        return new JSTileData() { ID = -1 };
    }

    private bool TryRemoveDataFromList(List<JSPathfindingTileData> _list, JSPathfindingTileData _tileData)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            // if we found the tile...
            if (_list[i].tile.ID == _tileData.tile.ID)
            {
                // remove the tile, report success.
                _list.RemoveAtSwapBack(i);
                return true;
            }
        }
        // report failure
        return false;
    }

    private bool TryGetTileInList(JSTileData _tile, List<JSPathfindingTileData> _list, out JSPathfindingTileData _pathingData)
    {
        // if the list is empty...
        if (_list.Count == 0)
        {
            // assign junk to _pathingData, report a fail.
            _pathingData = new JSPathfindingTileData();
            return false;
        }
        for (int i = 0; i < _list.Count; i++)
        {
            // if we found the tile...
            if (_list[i].tile.ID == _tile.ID)
            {
                // assign the tile to _pathingData, report success.
                _pathingData = _list[i];
                return true;
            }
        }
        // if we haven't found the tile...
        // assign junk to _pathingData, report a fail.
        _pathingData = _list[0];
        return false;
    }

    private float JSCalculateHCost(Vector3 _tilePosition, Vector3 _destination)
    {
        // find how many tiles in x and z
        // use the shorter one as the number of diagonal steps
        // use the difference between the longer one and the shorter one as the number of vertical/horizontal steps
        int XDistance = Mathf.RoundToInt(Mathf.Abs(_tilePosition.x - _destination.x));
        int ZDistance = Mathf.RoundToInt(Mathf.Abs(_tilePosition.z - _destination.z));
        if (ZDistance < XDistance)
        {
            return (14 * ZDistance) + (10 * (XDistance - ZDistance));
        }
        else
        {
            return (14 * XDistance) + (10 * (ZDistance - XDistance));
        }

    }

    private bool JSProcessTile(JSPathfindingTileData _pathingData, List<JSPathfindingTileData> _open, List<JSPathfindingTileData> _closed, JSTileData _destination)
    {
        // if the tile is the destination...
        if (_pathingData.tile.ID == _destination.ID)
        {
            // move the tile to the closed list
            TryRemoveDataFromList(_open, _pathingData);
            _closed.Add(_pathingData);
            // report success
            return true;
        }
        // otherwise...
        // calculate the costs for each neighbor of the tile and place them on the open list
        for (int i = 0; i < 8; i++)
        {
            // adjacents first, using 0 <= i < 4...
            if (i < 4)
            {
                // if the tile has an adjacent tile in that direction...
                if (_pathingData.tile.adjacentTiles[i] != -1)
                {
                    JSTileData tileI = GetTileDataWithID(_pathingData.tile.adjacentTiles[i]);
                    if (!TryGetTileInList(tileI, _closed, out _))
                    {
                        // add tile to open list
                        JSPathfindingTileData tileIPathingData = new JSPathfindingTileData()
                        {
                            tile = tileI,
                            fromTile = _pathingData.tile,
                            hCost = JSCalculateHCost(tileI.position, _destination.position),
                            gCost = _pathingData.gCost + 10f
                        };
                        if (TryGetTileInList(tileI, _open, out JSPathfindingTileData oldPathingData))
                        {
                            // if the new GCost is lower (we found a shorter route to that tile)...
                            if (tileIPathingData.gCost < oldPathingData.gCost)
                            {
                                // replace the pathing data with the new one
                                TryRemoveDataFromList(_open, oldPathingData);
                                _open.Add(tileIPathingData);
                            }
                        }
                        else // if there isn't any pathing data for that tile...
                        {
                            _open.Add(tileIPathingData);
                        }
                    }
                }
            }
            else // diagonals, using 4 <= i < 8
            {
                // if there is a diagonal tile for this index...
                if (_pathingData.tile.diagonalTiles[i - 4] != -1)
                {
                    // check that both adjacents for this diagonal are valid tiles
                    // CCW tile = i - 4, CW tile = (i - 3) % 4
                    bool CCWTile = _pathingData.tile.adjacentTiles[i - 4] != -1;
                    bool CWTile = _pathingData.tile.adjacentTiles[(i - 3) % 4] != -1;
                    // if they are, evaluate the diagonal for the open list
                    if (CCWTile && CWTile)
                    {
                        JSTileData tileI = GetTileDataWithID(_pathingData.tile.diagonalTiles[i - 4]);
                        if (!TryGetTileInList(tileI, _closed, out _))
                        {
                            // add tile to open list
                            JSPathfindingTileData tileIPathingData = new JSPathfindingTileData()
                            {
                                tile = tileI,
                                fromTile = _pathingData.tile,
                                hCost = JSCalculateHCost(tileI.position, _destination.position),
                                gCost = _pathingData.gCost + 14f
                            };
                            if (TryGetTileInList(tileI, _open, out JSPathfindingTileData oldPathingData))
                            {
                                // if the new GCost is lower (we found a shorter route to that tile)...
                                if (tileIPathingData.gCost < oldPathingData.gCost)
                                {
                                    // replace the pathing data with the new one
                                    TryRemoveDataFromList(_open, oldPathingData);
                                    _open.Add(tileIPathingData);
                                }
                            }
                            else // if there isn't any pathing data for that tile...
                            {
                                _open.Add(tileIPathingData);
                            }
                        }
                    }
                }
            }
        }
        TryRemoveDataFromList(_open, _pathingData);
        _closed.Add(_pathingData);
        return false;
    }

    private JSPathfindingTileData JSGetNextOpenTile(List<JSPathfindingTileData> _openList)
    {
        if (_openList.Count == 0)
        {
            return new JSPathfindingTileData();
        }
        JSPathfindingTileData result = _openList[0];
        if (_openList.Count > 1)
        {
            // find the open tile with the least FCost
            // if there are multiple with the same FCost, pick the one with the lowest HCost (the tile closest to the destination)
            float lowestFCost = result.FCost();
            float lowestHCost = result.hCost;
            foreach (JSPathfindingTileData pathingData in _openList)
            {
                float fCost = pathingData.FCost();
                if (fCost <= lowestFCost)
                {
                    if (fCost < lowestFCost)
                    {
                        lowestFCost = fCost;
                        result = pathingData;
                    }
                    else if (pathingData.hCost < lowestHCost)
                    {
                        lowestHCost = pathingData.hCost;
                        result = pathingData;
                    }
                }
            }
        }
        return result;
    }
}

public struct EnemyPathJobInfo
{
    public EnemyPathSignature signature;
    public JobHandle jobHandle;
    public float startTime;
    public NativeList<Vector3> resultPath;
    public Structure target;
}

public struct CompletedEnemyPathInfo
{
    public EnemyPathSignature signature;
    public JobHandle jobHandle;
    public EnemyPath path;
    public float runTime;
}

public class PathManager : MonoBehaviour
{
    private static PathManager instance = null;

    private NativeArray<int> allTileIDs;
    private NativeArray<JSTileData> allTiles;

    // Enemy Pathfinding
    private Dictionary<EnemyPathSignature, EnemyPath> calculatedPaths;
    private Dictionary<EnemyPathSignature, EnemyPathJobInfo> activeJobDict;
    private List<CompletedEnemyPathInfo> completedEnemyPathJobs;

    // Soldier Pathfinding
    private Dictionary<Soldier, SoldierPathData> soldierPaths;
    private Dictionary<Soldier, SoldierPathData> soldierHomeTrips;

    private TMP_Text debugText;
    private float totalTimeSync = 0f;
    private float totalTimeAsync = 0f;
    private float lastPathsClearTime = 0f;

    public static PathManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    public static EnemyPathSignature GenerateSignature(Vector3 _startPoint, List<StructureType> _validStructureTypes)
    {
        if (Physics.Raycast(_startPoint + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            return new EnemyPathSignature
            {
                startTile = hit.transform.GetComponent<TileBehaviour>(),
                validStructureTypes = _validStructureTypes
            };
        }
        else return new EnemyPathSignature();
    }

    public bool RequestPath(EnemyPathSignature _signature, ref EnemyPath _path)
    {
        // if the signature isn't valid...
        if (!_signature.IsValid)
        {
            // don't return anything, report false
            return false;
        }

        // if there already exists a path for that signature...
        if (calculatedPaths.ContainsKey(_signature))
        {
            // return the path & report true
            _path = new EnemyPath(calculatedPaths[_signature]);
            return true;
        }
        else // there isn't a calculated path for that signature.
        {
            // if there is an active job working on that signature...
            if (activeJobDict.ContainsKey(_signature))
            {
                // path isn't ready yet, don't return anything, report false
                return false;
            }
            else
            {
                // there isn't a path already, and there isn't a job working on it right now.
                // That means we need to schedule a new job.
                ScheduleEnemyPathJob(_signature);
                // the path isn't ready yet, so report false
                return false;
            }
        }
    }

    public bool RequestPath(Soldier _soldier, ref SoldierPath _path, bool _home = false)
    {
        if (soldierPaths.ContainsKey(_soldier) && !_home)
        {
            if (soldierPaths[_soldier].jobHandle.IsCompleted)
            {
                soldierPaths[_soldier].jobHandle.Complete();
                _path = new SoldierPath
                {
                    target = soldierPaths[_soldier].target,
                    pathPoints = new List<Vector3>(soldierPaths[_soldier].nativePath.ToArray())
                };
                soldierPaths[_soldier].nativePath.Dispose();
                soldierPaths.Remove(_soldier);

                return true;
            }
            return false;
        }
        else if (soldierHomeTrips.ContainsKey(_soldier) && _home)
        {
            if (soldierHomeTrips[_soldier].jobHandle.IsCompleted)
            {
                soldierHomeTrips[_soldier].jobHandle.Complete();
                _path = new SoldierPath
                {
                    target = soldierHomeTrips[_soldier].target,
                    pathPoints = new List<Vector3>(soldierHomeTrips[_soldier].nativePath.ToArray())
                };
                soldierHomeTrips[_soldier].nativePath.Dispose();
                soldierHomeTrips.Remove(_soldier);

                return true;
            }
            return false;
        }
        else
        {
            SoldierPathData newData = new SoldierPathData();
            newData.caller = _soldier;
            newData.nativePath = new NativeList<Vector3>(Allocator.Persistent);
            newData.path = new SoldierPath();
            TileBehaviour soldierTile = _soldier.GetCurrentTile();
            TileBehaviour destination;
            if (!_home)
            {
                newData.target = _soldier.GetClosestEnemy();
                if (newData.target)
                {
                    destination = newData.target.GetCurrentTile();
                }
                else
                {
                    newData.nativePath.Dispose();
                    return false;
                }
            }
            else
            {
                destination = _soldier.GetHomeTile();
            }
            if (soldierTile && destination)
            {
                newData.startTile = soldierTile.GenerateJSTileData();
                newData.endTile = destination.GenerateJSTileData();

                FindPath jobData = new FindPath(newData.startTile, newData.endTile, allTileIDs, allTiles, newData.nativePath);

                newData.jobHandle = jobData.Schedule();
                if (_home)
                {
                    soldierHomeTrips[_soldier] = newData;
                }
                else
                {
                    soldierPaths[_soldier] = newData;
                }
            }
            return false;

        }
    }

    public void ClearPaths()
    {
        calculatedPaths.Clear();
        lastPathsClearTime = Time.realtimeSinceStartup;
    }

    private void Start()
    {
        debugText = FindObjectOfType<Canvas>().transform.Find("PathfindingDebug").GetComponent<TMP_Text>();
        calculatedPaths = new Dictionary<EnemyPathSignature, EnemyPath>();
        activeJobDict = new Dictionary<EnemyPathSignature, EnemyPathJobInfo>();
        completedEnemyPathJobs = new List<CompletedEnemyPathInfo>();
        soldierPaths = new Dictionary<Soldier, SoldierPathData>();
        soldierHomeTrips = new Dictionary<Soldier, SoldierPathData>();

        // get all the tiles and generate a NativeList for all of them
        TileBehaviour[] allTileBehaviours = FindObjectsOfType<TileBehaviour>();
        allTiles = new NativeArray<JSTileData>(allTileBehaviours.Length, Allocator.Persistent);
        allTileIDs = new NativeArray<int>(allTileBehaviours.Length, Allocator.Persistent);
        for (int i = 0; i < allTileBehaviours.Length; i++)
        {
            allTiles[i] = allTileBehaviours[i].GenerateJSTileData();
            allTileIDs[i] = allTiles[i].ID;
        }
    }

    private void OnDestroy()
    {
        // dispose of the memory being used by the tiles list.
        Shutdown();
        allTiles.Dispose();
        allTileIDs.Dispose();
    }

    private EnemyPathJobInfo ScheduleEnemyPathJob(EnemyPathSignature _signature)
    {

        // starting from _signature.startTile, find the closest valid structure
        List<Structure> validStructures = new List<Structure>();
        for (int i = 0; i < _signature.validStructureTypes.Count; i++)
        {
            Structure[] structures = { };
            switch (_signature.validStructureTypes[i])
            {
                case StructureType.Defense:
                    structures = FindObjectsOfType<DefenseStructure>();
                    break;
                case StructureType.Longhaus:
                    structures = FindObjectsOfType<Longhaus>();
                    break;
                case StructureType.Storage:
                    structures = FindObjectsOfType<StorageStructure>();
                    break;
                case StructureType.Resource:
                    structures = FindObjectsOfType<ResourceStructure>();
                    break;
                default:
                    break;
            }

            validStructures.AddRange(structures);
        }

        // now that we have all the structures that the enemy can attack, let's find the closest structure.
        if (validStructures.Count == 0)
        {
            Debug.LogError("An Enemy tried to pathfind, and found no structures.");
            return new EnemyPathJobInfo();
        }
        
        // stop enemies from pathfinding to a structure that hasn't been placed yet
        validStructures.RemoveAll(structure => !structure.isPlaced);

        Structure closest = validStructures[0];
        float closestDistance = (validStructures[0].transform.position - _signature.startTile.transform.position).magnitude;

        for (int i = 1; i < validStructures.Count; i++)
        {
            float distance = (validStructures[i].transform.position - _signature.startTile.transform.position).magnitude;
            if (distance < closestDistance)
            {
                closest = validStructures[i];
                closestDistance = distance;
            }
        }

        JSTileData startTile = _signature.startTile.GenerateJSTileData();
        JSTileData destinationTile = closest.attachedTile.GenerateJSTileData();
        EnemyPathJobInfo newJobInfo = new EnemyPathJobInfo
        {
            startTime = Time.realtimeSinceStartup,
            resultPath = new NativeList<Vector3>(Allocator.Persistent),
            target = closest,
            signature = _signature
        };
        FindPath jobData = new FindPath(startTile, destinationTile, allTileIDs, allTiles, newJobInfo.resultPath);



        newJobInfo.jobHandle = jobData.Schedule();
        activeJobDict.Add(_signature, newJobInfo);
        return newJobInfo;
    }

    private void Update()
    {
        // toggle debugger
        if (SuperManager.DevMode)
        {
            if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.P))
            {
                debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
            }
        }

        if (activeJobDict.Count > 0) // there is one pathfinding job active
        {
            totalTimeSync += Time.deltaTime;
        }

        List<EnemyPathJobInfo> toBeRemoved = new List<EnemyPathJobInfo>();
        foreach (EnemyPathJobInfo info in activeJobDict.Values)
        {
            // If the path has been found
            if (info.jobHandle.IsCompleted)
            {
                // create a completedJob instance from the info.
                CompletedEnemyPathInfo completedJob = new CompletedEnemyPathInfo
                {
                    signature = info.signature,
                    jobHandle = info.jobHandle
                };
                info.jobHandle.Complete();
                // store the path
                // if the job started AFTER the last clear time (jobs from before need to be ignored.)
                if (info.startTime >= lastPathsClearTime)
                {
                    completedJob.path = new EnemyPath
                    {
                        target = info.target,
                        pathPoints = new List<Vector3>(info.resultPath.ToArray())
                    };
                    // runTime truly represents the time from when the task was scheduled to when the task was found to be complete
                    completedJob.runTime = Time.realtimeSinceStartup - info.startTime;
                    calculatedPaths.Add(completedJob.signature, completedJob.path);
                }
                
                // move the info into the other containers
                completedEnemyPathJobs.Add(completedJob);
                totalTimeAsync += completedJob.runTime;
                toBeRemoved.Add(info);
            }
        }
        foreach (EnemyPathJobInfo info in toBeRemoved)
        {
            // release memory
            info.resultPath.Dispose();
            activeJobDict.Remove(info.signature);
        }

        if (debugText.gameObject.activeSelf)
        {
            string heading = "Pathfinding Debugger Readout:";
            string activeJobs = "\nActive Jobs: " + activeJobDict.Count.ToString();
            string jobsCompleted = "\nJobs Completed: " + completedEnemyPathJobs.Count.ToString();
            string totalTimeSyncString = "\nJob Active Time: " + totalTimeSync.ToString();
            string totalTimeAsyncString = "\nCumulative Total Runtime: " + totalTimeAsync.ToString();

            string soldierJobs = "\n\nSoldier Jobs: " + soldierPaths.Count.ToString();
            debugText.text = heading + activeJobs + jobsCompleted + totalTimeSyncString + totalTimeAsyncString + soldierJobs;
        }
    }

    public void Shutdown()
    {
        foreach (EnemyPathJobInfo jobInfo in activeJobDict.Values)
        {
            jobInfo.jobHandle.Complete();
            jobInfo.resultPath.Dispose();
        }
    }
}
