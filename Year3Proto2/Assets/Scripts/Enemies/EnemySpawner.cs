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



public class EnemySpawner : MonoBehaviour
{
    public struct EnemyPathSignature
    {
        public TileBehaviour startTile;
        public List<StructureType> validStructureTypes;

        public static bool operator ==(EnemyPathSignature _lhs, EnemyPathSignature _rhs)
        {
            // if the signatures are both empty
            if (_lhs.validStructureTypes == null && _rhs.validStructureTypes == null)
            {
                return true;
            }
            // if the signatures are both well defined
            else if (_lhs.validStructureTypes != null && _rhs.validStructureTypes != null)
            {
                foreach (StructureType type in _lhs.validStructureTypes)
                {
                    if (!_rhs.validStructureTypes.Contains(type))
                    {
                        return false;
                    }
                }
                return _lhs.startTile == _rhs.startTile;
            }
            // if one of them is defined and the other is empty
            else
            {
                return false;
            }
        }

        public bool isValid
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

        public static bool operator !=(EnemyPathSignature _lhs, EnemyPathSignature _rhs)
        {
            return !(_lhs == _rhs);
        }
    }

    public struct EnemyPath
    {
        public List<Vector3> pathPoints;
        public Structure target;
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

    public struct PathJobInfo
    {
        public EnemyPathSignature signature;
        public JobHandle jobHandle;
        public float startTime;
        public NativeList<Vector3> resultPath;
        public Structure target;
    }

    public struct CompletedPathInfo
    {
        public EnemyPathSignature signature;
        public JobHandle jobHandle;
        public EnemyPath path;
        public float runTime;
    }


    [Header("Prefabrication")]
    private List<Enemy> enemies = new List<Enemy>();
    public GameObject[] enemyPrefabs;
    public GameObject puffEffect;
    public int enemyCount
    {
        get
        {
            return enemies.Count;
        }
    }

    private int enemiesKilled = 0;
    private int waveCounter = 0;
    private float lastPathsClearTime = 0f;
    private Dictionary<EnemyPathSignature, EnemyPath> calculatedPaths;
    private Dictionary<EnemyPathSignature, PathJobInfo> activeJobDict;
    private List<CompletedPathInfo> completedJobs;
    private float totalTimeSync = 0f;
    private float totalTimeAsync = 0f;
    // to be deleted/moved, I think
    private bool spawning = false;
    public int enemiesPerWave = 8;
    public int newEnemiesPerWave = 4;
    public float cooldown = 30.0f;
    public float timeBetweenWaves = 30.0f;
    private NativeArray<int> allTileIDs;
    private NativeArray<JSTileData> allTiles;
    private TMP_Text debugText;

    public int GetKillCount()
    {
        return enemiesKilled;
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

    public EnemyPath GetPath(Vector3 _startPoint, List<StructureType> _validStructureTypes)
    {
        return FindPathWithSignature(GenerateSignature(_startPoint, _validStructureTypes));
    }
    
    public bool RequestPath(Vector3 _startPoint, List<StructureType> _validStructureTypes, ref EnemyPath _path)
    {
        return RequestPath(GenerateSignature(_startPoint, _validStructureTypes), ref _path);
    }

    public bool RequestPath(EnemyPathSignature _signature, ref EnemyPath _path)
    {
        // if the signature isn't valid...
        if (!_signature.isValid)
        {
            // don't return anything, report false
            return false;
        }

        // if there already exists a path for that signature...
        if (calculatedPaths.ContainsKey(_signature))
        {
            // return the path & report true
            _path = calculatedPaths[_signature];
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
                ScheduleNewPathJob(_signature);
                // the path isn't ready yet, so report false
                return false;
            }
            
        }
    }


    private EnemyPath FindPathWithSignature(EnemyPathSignature _signature)
    {
        // if a path already exists for that signature...
        if (calculatedPaths.ContainsKey(_signature))
        {
            // return that path.
            return calculatedPaths[_signature];
        }
        // otherwise, if the signature is a dud...
        else if (!_signature.isValid)
        {
            // return an empty path
            return new EnemyPath();
        }
        else
        {
            return GenerateNewPath(_signature);
        }
    }

    private EnemyPath GenerateNewPath(EnemyPathSignature _signature)
    {
        EnemyPath path = new EnemyPath();
        path.pathPoints = new List<Vector3>();

        float startTime = Time.realtimeSinceStartup;

        // FIRST VERSION
        // Find the closest target, path find to it | DONE
        // SECOND VERSION
        // Find the closest target, path find to it,
        // Test 4 points along the path to see if there are closer targets at each point - Path testing
        // Test paths when a new structure is placed
        // if the test fails (there's a closer structure) find a new path to that structure

        // starting from _signature.startTile, find the closest valid structure
        List<Structure> validStructures = new List<Structure>();
        for (int i = 0; i < _signature.validStructureTypes.Count; i++)
        {
            Structure[] structures = { };
            switch (_signature.validStructureTypes[i])
            {
                case StructureType.Attack:
                    structures = FindObjectsOfType<AttackStructure>();
                    break;
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
            return path;
        }

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

        // we have our destination and our source, now use A* to find the path
        // generate initial open and closed lists
        List<PathfindingTileData> open = new List<PathfindingTileData>();
        List<PathfindingTileData> closed = new List<PathfindingTileData>();
        TileBehaviour destination = closest.attachedTile;
        // add the tiles next to the start tile to the open list and calculate their costs
        PathfindingTileData startingData = new PathfindingTileData()
        {
            tile = _signature.startTile,
            fromTile = _signature.startTile,
            gCost = 0f,
            hCost = CalculateHCost(_signature.startTile, destination)
        };

        ProcessTile(startingData, open, closed, destination);

        // while a path hasn't been found
        bool pathFound = false;
        int lapCount = 0;
        while (!pathFound && open.Count > 0 && lapCount < 200000)
        {
            lapCount++;
            if (ProcessTile(GetNextOpenTile(open), open, closed, destination))
            {
                // generate a path from the tiles in the closed list
                // path from the source tile to the destination tile
                // find the destination tile in the closed list
                List<Vector3> reversePath = new List<Vector3>();
                PathfindingTileData currentData = closed[closed.Count - 1];
                while (currentData.fromTile != currentData.tile)
                {
                    reversePath.Add(currentData.tile.transform.position);
                    currentData = FollowFromTile(closed, currentData.fromTile);
                }
                reversePath.Reverse();
                path.pathPoints = reversePath;
                path.target = closest;
                pathFound = true;
            }
            if (lapCount % 10000 == 0)
            {
                Debug.LogWarning("lapCount = " + lapCount);
            }
        }

        if (!calculatedPaths.ContainsKey(_signature))
        {
            calculatedPaths.Add(_signature, path);
        }

        float finishTime = Time.realtimeSinceStartup;
        Debug.Log("Pathfinding complete, took " + (finishTime - startTime).ToString() + " seconds");
        return path;
    }

    public void OnStructurePlaced()
    {
        calculatedPaths.Clear();
        lastPathsClearTime = Time.realtimeSinceStartup;
    }

    public void RecordNewEnemy(Enemy _enemy)
    {
        enemies.Add(_enemy);
    }

    public static PathfindingTileData GetNextOpenTile(List<PathfindingTileData> _open)
    {
        PathfindingTileData next = new PathfindingTileData();
        if (_open.Count >= 1)
        {
            next = _open[0];
            if (_open.Count == 1)
            {
                return next;
            }
            // find the open tile with the least FCost
            // if there are multiple with the same FCost, pick the one with the lowest HCost (the tile closest to the destination)
            float lowestFCost = next.FCost();
            float lowestHCost = next.hCost;
            foreach (PathfindingTileData tileData in _open)
            {
                if (tileData.FCost() <= lowestFCost)
                {
                    if (tileData.FCost() < lowestFCost)
                    {
                        lowestFCost = tileData.FCost();
                        next = tileData;
                    }
                    else if (tileData.hCost < lowestHCost)
                    {
                        lowestHCost = tileData.hCost;
                        next = tileData;
                    }
                }
            }
        }
        return next;
    }

    public static bool ProcessTile(PathfindingTileData _tileData, List<PathfindingTileData> _open, List<PathfindingTileData> _closed, TileBehaviour _destination)
    {
        // evaluate if the tile is the destination tile
        if (_tileData.tile == _destination)
        {
            // generate pathfinding data and put on the closed list
            _open.Remove(_tileData);
            _closed.Add(_tileData);
            return true;
        }
        // if not
        else
        {
            // calculate the costs for each neighbor of the tile and place them on the open list
            for (int i = 0; i < 8; i++)
            {
                // adjacents
                if (i < 4)
                {
                    if (_tileData.tile.GetAdjacentTiles().ContainsKey((TileBehaviour.TileCode)i))
                    {
                        TileBehaviour iTile = _tileData.tile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];

                        if (!TileInClosedList(iTile, _closed))
                        {
                            // add tile to open list
                            // calculate the pathfinding data
                            PathfindingTileData iTileData = new PathfindingTileData()
                            {
                                tile = iTile,
                                fromTile = _tileData.tile,
                                hCost = CalculateHCost(iTile, _destination),
                                gCost = _tileData.gCost + 10f
                            };

                            // if the tile is not in the closed list
                            PathfindingTileData oldITileData = new PathfindingTileData();
                            // if there already is pathfinding data for that tile...
                            if (TileInOpenList(iTile, _open, ref oldITileData))
                            {
                                // compare the GCosts and only replace it if the new GCost is lower (we found a shorter route to that tile)
                                if (iTileData.gCost < oldITileData.gCost)
                                {
                                    // remove the tile from the open list and replace it with the shorter gCost path
                                    _open.Remove(oldITileData);
                                    _open.Add(iTileData);
                                }
                            }
                            // if there isn't any pathfinding data for that tile
                            else
                            {
                                // add the tile to the open list
                                _open.Add(iTileData);
                            }
                        }
                    }
                }
                // diagonals
                else
                {
                    if (_tileData.tile.GetDiagonalTiles().ContainsKey(i - 4))
                    {
                        TileBehaviour iTile = _tileData.tile.GetDiagonalTiles()[i - 4];
                        // check that both adjacents for this diagonal are valid
                        // CCW tile = i - 4, CW tile = (i - 3) % 4
                        bool CCWTile = _tileData.tile.GetAdjacentTiles().ContainsKey((TileBehaviour.TileCode)(i - 4));
                        bool CWTile = _tileData.tile.GetAdjacentTiles().ContainsKey((TileBehaviour.TileCode)((i - 3) % 4));
                        // if they are, evaluate the diagonal for the open list
                        if (CCWTile && CWTile)
                        {
                            // add tile to open list
                            // calculate the pathfinding data
                            PathfindingTileData iTileData = new PathfindingTileData()
                            {
                                tile = iTile,
                                fromTile = _tileData.tile,
                                hCost = CalculateHCost(iTile, _destination),
                                gCost = _tileData.gCost + 14f
                            };
                            PathfindingTileData oldITileData = new PathfindingTileData();
                            // if there already is pathfinding data for that tile...
                            if (TileInOpenList(iTile, _open, ref oldITileData))
                            {
                                // compare the GCosts and only replace it if the new GCost is lower (we found a shorter route to that tile)
                                if (iTileData.gCost < oldITileData.gCost)
                                {
                                    // remove the tile from the open list and place the tile on the closed list
                                    _open.Remove(oldITileData);
                                    _open.Add(iTileData);
                                }
                            }
                            // if there isn't any pathfinding data for that tile
                            else
                            {
                                // add the tile to the open list
                                _open.Add(iTileData);
                            }
                        }
                    }
                }
            }
            _open.Remove(_tileData);
            _closed.Add(_tileData);
            return false;
        }
    }

    private static bool TileInOpenList(TileBehaviour _tile, List<PathfindingTileData> _open, ref PathfindingTileData _tileData)
    {
        foreach (PathfindingTileData tileData in _open)
        {
            if (tileData.tile == _tile)
            {
                _tileData = tileData;
                return true;
            }
        }
        return false;
    }

    private static bool TileInClosedList(TileBehaviour _tile, List<PathfindingTileData> _closed)
    {
        foreach (PathfindingTileData tileData in _closed)
        {
            if (tileData.tile == _tile)
            {
                return true;
            }
        }
        return false;
    }

    public static PathfindingTileData FollowFromTile(List<PathfindingTileData> _closed, TileBehaviour _tile)
    {
        //find the tiledata in the closed list that corresponds to _tile
        foreach (PathfindingTileData tileData in _closed)
        {
            if (tileData.tile == _tile)
            {
                return tileData;
            }
        }
        return new PathfindingTileData();
    }

    public static float CalculateHCost(TileBehaviour _tile, TileBehaviour _destination)
    {
        // find how many tiles in x and z
        // use the shorter one as the number of diagonal steps
        // use the difference between the longer one and the shorter one as the number of vertical/horizontal steps
        int xDist = Mathf.RoundToInt(Mathf.Abs(_tile.transform.position.x - _destination.transform.position.x));
        int zDist = Mathf.RoundToInt(Mathf.Abs(_tile.transform.position.z - _destination.transform.position.z));
        if (zDist < xDist)
        {
            return (14 * zDist) + (10 * (xDist - zDist));
        }
        else
        {
            return (14 * xDist) + (10 * (zDist - xDist));
        }
    }
    
    public void SetKillCount(int _killCount)
    {
        enemiesKilled = _killCount;
    }

    private void Start()
    {
        debugText = FindObjectOfType<Canvas>().transform.Find("PathfindingDebug").GetComponent<TMP_Text>();
        calculatedPaths = new Dictionary<EnemyPathSignature, EnemyPath>();
        activeJobDict = new Dictionary<EnemyPathSignature, PathJobInfo>();
        completedJobs = new List<CompletedPathInfo>();

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
        allTiles.Dispose();
        allTileIDs.Dispose();
    }

    private PathJobInfo ScheduleNewPathJob(EnemyPathSignature _signature)
    {

        // starting from _signature.startTile, find the closest valid structure
        List<Structure> validStructures = new List<Structure>();
        for (int i = 0; i < _signature.validStructureTypes.Count; i++)
        {
            Structure[] structures = { };
            switch (_signature.validStructureTypes[i])
            {
                case StructureType.Attack:
                    structures = FindObjectsOfType<AttackStructure>();
                    break;
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
            return new PathJobInfo();
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
        PathJobInfo newJobInfo = new PathJobInfo
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
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.P))
        {
            debugText.gameObject.SetActive(!debugText.gameObject.activeSelf);
        }

        if (activeJobDict.Count > 0) // there is one pathfinding job active
        {
            totalTimeSync += Time.deltaTime;
        }

        List<PathJobInfo> toBeRemoved = new List<PathJobInfo>();
        foreach (PathJobInfo info in activeJobDict.Values)
        {
            // If the path has been found
            if (info.jobHandle.IsCompleted)
            {
                // create a completedJob instance from the info.
                CompletedPathInfo completedJob = new CompletedPathInfo
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
                completedJobs.Add(completedJob);
                totalTimeAsync += completedJob.runTime;
                toBeRemoved.Add(info);
            }
        }
        foreach (PathJobInfo info in toBeRemoved)
        {
            // release memory
            info.resultPath.Dispose();
            activeJobDict.Remove(info.signature);
        }
        if (debugText.gameObject.activeSelf)
        {
            string heading = "Pathfinding Debugger Readout:";
            string activeJobs = "\nActive Jobs: " + activeJobDict.Count.ToString();
            string jobsCompleted = "\nJobs Completed: " + completedJobs.Count.ToString();
            string totalTimeSyncString = "\nJob Active Time: " + totalTimeSync.ToString();
            string totalTimeAsyncString = "\nCumulative Total Runtime: " + totalTimeAsync.ToString();
            debugText.text = heading + activeJobs + jobsCompleted + totalTimeSyncString + totalTimeAsyncString;
        }
    }

    private void FixedUpdate()
    {
        /*
        if (spawning && false)
        {
            cooldown -= Time.fixedDeltaTime;
            if (cooldown <= 0.0f)
            {
                waveCounter++;
                if (waveCounter == 1)
                {
                    messageBox.ShowMessage("Invaders incoming!", 3.5f);
                }
                cooldown = timeBetweenWaves;

                enemiesPerWave = Mathf.Clamp(enemiesPerWave, 0, 300);
                int enemiesLeftToSpawn = enemiesPerWave;
                // SPAWN ENEMY WAVE
                // up to 4 enemies per tile, spread out at 0.25 points

                // calculate how many tiles are needed
                int tilesRequired = (int)Mathf.Ceil(enemiesPerWave / 4f);

                // randomly select that number of tiles, store them as a seperate collection of tiles
                // randomly select one tile...
                TileBehaviour startTile = GetRandomSpawnTile();
                waveSelectedTiles.Clear();
                waveSelectedTiles.Add(startTile);
                int tilesSelected = 1;

                // then grow from that point until enough tiles are selected...

                while (tilesSelected < tilesRequired)
                {
                    Debug.Log("Tiles selected: " + tilesSelected);
                    int tilesToFind = tilesRequired - tilesSelected;
                    TileBehaviour randomSelectedTile = waveSelectedTiles[Random.Range(0, waveSelectedTiles.Count)];
                    waveValidTiles.Clear();
                    for (int i = 0; i < 8; i++)
                    {
                        if (i < 4)
                        {
                            if (randomSelectedTile.GetAdjacentTiles().ContainsKey((TileBehaviour.TileCode)i))
                            {
                                TileBehaviour tileI = randomSelectedTile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];
                                if (tileI.GetSpawnTile() && !waveSelectedTiles.Contains(tileI))
                                {
                                    waveValidTiles.Add(tileI);
                                }
                            }
                        }
                        else
                        {
                            if (randomSelectedTile.GetDiagonalTiles().ContainsKey(i - 4))
                            {
                                TileBehaviour tileI = randomSelectedTile.GetDiagonalTiles()[i - 4];
                                if (tileI.GetSpawnTile() && !waveSelectedTiles.Contains(tileI))
                                {
                                    waveValidTiles.Add(tileI);
                                }
                            }
                        }
                    }
                    while (tilesToFind > 0 && waveValidTiles.Count > 0)
                    {
                        tilesToFind--;
                        TileBehaviour movingTile = waveValidTiles[Random.Range(0, waveValidTiles.Count)];
                        waveSelectedTiles.Add(movingTile);
                        waveValidTiles.Remove(movingTile);
                        tilesSelected++;
                    }
                }

                Vector3 lastEnemySpawnedPosition = Vector3.zero;
                // for each tile
                for (int i = 0; i < waveSelectedTiles.Count; i++)
                {
                    TileBehaviour spawnTile = waveSelectedTiles[i];
                    //   enemies to spawn on this tile = clamp total number to spawn left between 0 and 4
                    int enemiesToSpawnHere = Mathf.Clamp(enemiesLeftToSpawn, 0, 4);
                    //   if there are enemies to spawn, spawn them
                    for (int j = 0; j < enemiesToSpawnHere; j++)
                    {
                        // Calculate position to spawn enemy
                        Vector3 startingPosition = spawnTile.transform.position;
                        Vector3 enemySpawnPosition = startingPosition;
                        // y position is handled by enemy start function
                        enemySpawnPosition.x += (j % 2 == 0) ? -.25f : .25f;
                        enemySpawnPosition.z += ((j + 1) % 2 == 0) ? -.25f : .25f;
                        if (j == 0 && enemiesToSpawnHere == 4) // we can afford a heavy invader
                        {
                            // 25% chance to spawn a heavy invader
                            float random = Random.Range(0f, 1f);
                            if (random < 0.20f)
                            {
                                enemySpawnPosition = spawnTile.transform.position;
                                HeavyInvader newInvader = Instantiate(enemyPrefabs[1], enemySpawnPosition, Quaternion.identity).GetComponent<HeavyInvader>();
                                lastEnemySpawnedPosition = newInvader.transform.position;
                                newInvader.Randomize();
                                newInvader.spawner = this;
                                enemies.Add(newInvader);
                                enemiesLeftToSpawn -= 4;
                                j = 3;
                            }
                            else
                            {
                                Invader newInvader = Instantiate(enemyPrefabs[0], enemySpawnPosition, Quaternion.identity).GetComponent<Invader>();
                                lastEnemySpawnedPosition = newInvader.transform.position;
                                newInvader.SetScale(Random.Range(0.8f, 1.5f));
                                newInvader.spawner = this;
                                enemies.Add(newInvader);
                                enemiesLeftToSpawn--;
                            }
                        }
                        else // we can't afford a heavy invader
                        {
                            Invader newInvader = Instantiate(enemyPrefabs[0], enemySpawnPosition, Quaternion.identity).GetComponent<Invader>();
                            lastEnemySpawnedPosition = newInvader.transform.position;
                            newInvader.SetScale(Random.Range(0.8f, 1.5f));
                            newInvader.spawner = this;
                            enemies.Add(newInvader);
                            enemiesLeftToSpawn--;
                        }
                    }
                }
                // The start tile plays the spawn effect
                GameManager.CreateAudioEffect("horn", lastEnemySpawnedPosition);

                // Next wave is bigger
                enemiesPerWave += newEnemiesPerWave;
            }
        }
        */
    }
    
    public void LoadInvader(SuperManager.InvaderSaveData _saveData)
    {
        Invader enemy = Instantiate(enemyPrefabs[0]).GetComponent<Invader>();

        enemy.transform.position = _saveData.enemyData.position;
        enemy.transform.rotation = _saveData.enemyData.orientation;
        enemy.SetScale(_saveData.scale);
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.enemyData.targetPosition));
        enemy.SetState(_saveData.enemyData.state);
        enemy.spawner = this;

        enemies.Add(enemy);
    }

    public void LoadHeavyInvader(SuperManager.HeavyInvaderSaveData _saveData)
    {
        HeavyInvader enemy = Instantiate(enemyPrefabs[1]).GetComponent<HeavyInvader>();

        enemy.transform.position = _saveData.enemyData.position;
        enemy.transform.rotation = _saveData.enemyData.orientation;
        enemy.SetEquipment(_saveData.equipment);
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.enemyData.targetPosition));
        enemy.SetState(_saveData.enemyData.state);
        enemy.spawner = this;

        enemies.Add(enemy);
    }

    public int GetWaveCurrent()
    {
        return waveCounter;
    }

    public void SetWaveCurrent(int _wave)
    {
        waveCounter = _wave;
    }

    public void ToggleSpawning()
    {
        spawning = !spawning;
    }

    public bool IsSpawning()
    {
        return spawning;
    }

    public void OnEnemyDeath(Enemy _enemy)
    {
        enemiesKilled++;
        if (enemies.Contains(_enemy))
        {
            enemies.Remove(_enemy);
        }
    }
}
