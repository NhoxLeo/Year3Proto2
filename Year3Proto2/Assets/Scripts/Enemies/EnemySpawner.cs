using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
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

    private MessageBox messageBox;
    List<TileBehaviour> availableTiles;
    List<TileBehaviour> waveValidTiles;
    List<TileBehaviour> waveSelectedTiles;

    public int GetKillCount()
    {
        return enemiesKilled;
    }

    public void SetKillCount(int _killCount)
    {
        enemiesKilled = _killCount;
    }

    private void Start()
    {
        messageBox = FindObjectOfType<MessageBox>();
        availableTiles = new List<TileBehaviour>();
        waveValidTiles = new List<TileBehaviour>();
        waveSelectedTiles = new List<TileBehaviour>();
        foreach (GameObject enemy in enemyPrefabs)
        {
            enemy.GetComponent<Enemy>().puffEffect = puffEffect;
        }
        foreach (TileBehaviour tileBehaviour in FindObjectsOfType<TileBehaviour>())
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
                        if (randomSelectedTile.GetAdjacentTiles().ContainsKey(tileCodeI))
                        {
                            TileBehaviour tileI = randomSelectedTile.GetAdjacentTiles()[tileCodeI];
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
                                enemies.Add(newInvader);
                                enemiesLeftToSpawn -= 4;
                                j = 3;
                            }
                            else
                            {
                                Invader newInvader = Instantiate(enemyPrefabs[0], enemySpawnPosition, Quaternion.identity).GetComponent<Invader>();
                                lastEnemySpawnedPosition = newInvader.transform.position;
                                newInvader.SetScale(Random.Range(1.5f, 2f));
                                enemies.Add(newInvader);
                                enemiesLeftToSpawn--;
                            }
                        }
                        else // we can't afford a heavy invader
                        {
                            Invader newInvader = Instantiate(enemyPrefabs[0], enemySpawnPosition, Quaternion.identity).GetComponent<Invader>();
                            lastEnemySpawnedPosition = newInvader.transform.position;
                            newInvader.SetScale(Random.Range(1.5f, 2f));
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
        HeavyInvader enemy = Instantiate(enemyPrefabs[0]).GetComponent<HeavyInvader>();
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
