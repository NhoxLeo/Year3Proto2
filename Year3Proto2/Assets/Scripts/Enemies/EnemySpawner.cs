using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public struct PathSignature
    {
        public TileBehaviour startTile;
        public List<StructureType> validStructureTypes;

        public static bool operator ==(PathSignature _lhs, PathSignature _rhs)
        {
            return (_lhs.startTile == _rhs.startTile) && (_lhs.validStructureTypes == _rhs.validStructureTypes);
        }

        public static bool operator !=(PathSignature _lhs, PathSignature _rhs)
        {
            return !(_lhs == _rhs);
        }
    }

    public struct Path
    {
        public List<Vector3> pathPoints;
        public Structure target;
    }

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



    [Header("Prefabrication")]
    private List<Enemy> enemies = new List<Enemy>();
    public GameObject[] enemyPrefabs;
    public GameObject puffEffect;
    public int EnemyCount
    {
        get
        {
            return enemies.Count;
        }
    }

    [Header("Variables")]
    public int enemiesPerWave = 8;
    public int newEnemiesPerWave = 4;
    public float cooldown = 30.0f;
    public float timeBetweenWaves = 30.0f;
    private int enemiesKilled = 0;
    private int waveCounter = 0;
    private bool spawning = false;
    private Dictionary<PathSignature, Path> calculatedPaths;

    private MessageBox messageBox;
    List<TileBehaviour> allTiles;
    List<TileBehaviour> availableTiles;
    List<TileBehaviour> waveValidTiles;
    List<TileBehaviour> waveSelectedTiles;

    public int GetKillCount()
    {
        return enemiesKilled;
    }

    public Path GetPath(Vector3 startPoint, List<StructureType> _validStructureTypes)
    {
        PathSignature pathSignature = new PathSignature();
        if (Physics.Raycast(startPoint + Vector3.up, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            pathSignature.startTile = hit.transform.GetComponent<TileBehaviour>();
            pathSignature.validStructureTypes = _validStructureTypes;
        }
        Path path = FindPathWithSignature(pathSignature);
        return path;
    }

    private Path FindPathWithSignature(PathSignature _signature)
    {
        if (calculatedPaths.ContainsKey(_signature))
        {
            return calculatedPaths[_signature];
        }
        else if (_signature == new PathSignature())
        {
            return new Path();
        }
        else
        { return GenerateNewPath(_signature); }
    }

    private Path GenerateNewPath(PathSignature _signature)
    {
        Path path = new Path();
        path.pathPoints = new List<Vector3>();

        // FIRST VERSION
        // Find the closest target, path find to it
        // SECOND VERSION
        // Find the closest target, path find to it,
        // Test 4 points along the path to see if there are closer targets at each point - Path testing
        // Test paths when a new structure is placed
        // if the test fails (there's a closer structure) find a new path to that structure

        // A*

        // starting from _signature.startTile, find the closest valid structure
        List<Structure> validStructures = new List<Structure>();
        for (int i = 0; i < _signature.validStructureTypes.Count; i++)
        {
            Structure[] structures = { };
            switch (_signature.validStructureTypes[i])
            {
                case StructureType.attack:
                    structures = FindObjectsOfType<AttackStructure>();
                    break;
                case StructureType.defense:
                    structures = FindObjectsOfType<DefenseStructure>();
                    break;
                case StructureType.longhaus:
                    structures = FindObjectsOfType<Longhaus>();
                    break;
                case StructureType.storage:
                    structures = FindObjectsOfType<StorageStructure>();
                    break;
                case StructureType.resource:
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
        while (!pathFound && open.Count > 0)
        {
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
        }

        if (!calculatedPaths.ContainsKey(_signature))
        {
            calculatedPaths.Add(_signature, path);
        }
        return path;
    }

    public void OnStructurePlaced()
    {
        calculatedPaths.Clear();
    }

    private static PathfindingTileData GetNextOpenTile(List<PathfindingTileData> _open)
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

    private static bool ProcessTile(PathfindingTileData _tileData, List<PathfindingTileData> _open, List<PathfindingTileData> _closed, TileBehaviour _destination)
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
                // diagonals
                else
                {
                    if (_tileData.tile.GetDiagonalTiles().ContainsKey(i - 4))
                    {
                        TileBehaviour iTile = _tileData.tile.GetDiagonalTiles()[i - 4];
                        // check that both adjacents for this diagonal are in the open list
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

    private static PathfindingTileData FollowFromTile(List<PathfindingTileData> _closed, TileBehaviour _tile)
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

    private static float CalculateHCost(TileBehaviour _tile, TileBehaviour _destination)
    {
        // find how many tiles in x and z
        // use the shorter one as the number of diagonal steps
        // use the difference between the longer one and the shorter one
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
        messageBox = FindObjectOfType<MessageBox>();
        allTiles = new List<TileBehaviour>();
        availableTiles = new List<TileBehaviour>();
        waveValidTiles = new List<TileBehaviour>();
        waveSelectedTiles = new List<TileBehaviour>();
        allTiles.AddRange(FindObjectsOfType<TileBehaviour>());
        calculatedPaths = new Dictionary<PathSignature, Path>();
        foreach (GameObject enemy in enemyPrefabs)
        {
            enemy.GetComponent<Enemy>().puffEffect = puffEffect;
        }
        foreach (TileBehaviour tileBehaviour in allTiles)
        {
            if (tileBehaviour.GetSpawnTile()) availableTiles.Add(tileBehaviour);
        }
    }

    public TileBehaviour GetRandomSpawnTile()
    {
        return availableTiles[Random.Range(0, availableTiles.Count)];
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.RightBracket))
        {
            spawning = true;
            cooldown = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (spawning)
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
                    for (int i = 0; i < 4; i++)
                    {
                        TileBehaviour.TileCode tileCodeI = (TileBehaviour.TileCode)i;
                        Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToRandom = randomSelectedTile.GetAdjacentTiles();
                        if (adjacentsToRandom.ContainsKey(tileCodeI))
                        {
                            TileBehaviour tileI = adjacentsToRandom[tileCodeI];
                            if (tileI.GetSpawnTile() && !waveSelectedTiles.Contains(tileI))
                            {
                                waveValidTiles.Add(tileI);
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
                                newInvader.SetScale(Random.Range(1.5f, 2f));
                                newInvader.spawner = this;
                                enemies.Add(newInvader);
                                enemiesLeftToSpawn--;
                            }
                        }
                        else // we can't afford a heavy invader
                        {
                            Invader newInvader = Instantiate(enemyPrefabs[0], enemySpawnPosition, Quaternion.identity).GetComponent<Invader>();
                            lastEnemySpawnedPosition = newInvader.transform.position;
                            newInvader.SetScale(Random.Range(1.5f, 2f));
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
    }
    
    public void LoadInvader(SuperManager.InvaderSaveData _saveData)
    {
        Invader enemy = Instantiate(enemyPrefabs[0]).GetComponent<Invader>();
        enemy.transform.position = _saveData.enemyData.position;
        enemy.transform.rotation = _saveData.enemyData.orientation;
        enemy.SetScale(_saveData.scale);
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.enemyData.targetPosition));
        enemy.SetState(_saveData.enemyData.state);
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
