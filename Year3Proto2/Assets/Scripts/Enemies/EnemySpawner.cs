using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabrication")]
    private List<Enemy> enemies = new List<Enemy>();
    public Enemy[] enemyPrefabs;
    public int enemyCount
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
    private int waveCounter = 0;
    private bool spawning = false;

    private MessageBox messageBox;
    List<TileBehaviour> availableTiles;
    List<TileBehaviour> waveValidTiles;
    List<TileBehaviour> waveSelectedTiles;

    private void Start()
    {
        messageBox = FindObjectOfType<MessageBox>();
        availableTiles = new List<TileBehaviour>();
        waveValidTiles = new List<TileBehaviour>();
        waveSelectedTiles = new List<TileBehaviour>();

        foreach (TileBehaviour tileBehaviour in FindObjectsOfType<TileBehaviour>())
        {
            if (tileBehaviour.GetSpawnTile()) availableTiles.Add(tileBehaviour);
        }
    }

    public TileBehaviour GetRandomSpawnTile()
    {
        return availableTiles[Random.Range(0, availableTiles.Count)];
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
                        enemySpawnPosition.x += (j % 2 == 0) ? -.25f : .25f;
                        enemySpawnPosition.y += .55f;
                        enemySpawnPosition.z += ((j + 1) % 2 == 0) ? -.25f : .25f;
                        enemies.Add(Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], enemySpawnPosition, Quaternion.identity));
                        enemiesLeftToSpawn--;
                    }
                }
                // The start tile plays the spawn effect
                GameManager.CreateAudioEffect("horn", startTile.transform.position);

                // Next wave is bigger
                enemiesPerWave += newEnemiesPerWave;
            }
        }
    }

    public void LoadEnemy(SuperManager.EnemySaveData _saveData)
    {
        if (_saveData.enemy == "Invader")
        {
            Enemy enemy = Instantiate(enemyPrefabs[0]);
            enemy.transform.position = _saveData.position;
            enemy.transform.rotation = _saveData.orientation;
            enemy.SetScale(_saveData.scale);
            enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.targetPosition));
            enemy.SetState(_saveData.state);
            enemies.Add(enemy);
        }
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

    public void RemoveEnemy(Enemy _enemy)
    {
        if (enemies.Contains(_enemy))
        {
            enemies.Remove(_enemy);
        }
    }
}
