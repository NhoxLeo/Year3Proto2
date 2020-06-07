using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{

    [Header("Prefabrication")]
    private List<Unit> units = new List<Unit>();
    public Unit[] unitPrefabs;
    public GameObject puffEffect;

    public int unitCount
    {
        get
        {
            return units.Count;
        }
    }

    [Header("Properties")]
    public int unitsPerWave = 8;
    public int newUnitsPerWave = 4;
    public float cooldown = 30.0f;
    public float timeBetweenWaves = 30.0f;

    private int waveCounter = 0;
    private int unitsKilled = 0;
    private bool spawning = false;

    private MessageBox messageBox;
    private List<TileBehaviour> availableTiles;
    private List<TileBehaviour> waveValidTiles;
    private List<TileBehaviour> waveSelectedTiles;

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

        foreach (Unit unit in unitPrefabs)
        {
            unit.GetComponent<Unit>().puffEffect = puffEffect;
        }

    }

    private void FixedUpdate()
    {
        if (!spawning) return;

        cooldown -= Time.fixedDeltaTime;
        if (cooldown <= 0.0f)
        {
            waveCounter++;
            if (waveCounter == 1) messageBox.ShowMessage("Invaders incoming!", 3.5f);

            cooldown = timeBetweenWaves;
            unitsPerWave = Mathf.Clamp(unitsPerWave, 0, 300);
            int unitsLeftToSpawn = unitsPerWave;
            // SPAWN ENEMY WAVE
            // up to 4 enemies per tile, spread out at 0.25 points

            // calculate how many tiles are needed
            int tilesRequired = (int)Mathf.Ceil(unitsPerWave / 4f);

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
                int enemiesToSpawnHere = Mathf.Clamp(unitsLeftToSpawn, 0, 4);
                //   if there are enemies to spawn, spawn them
                for (int j = 0; j < enemiesToSpawnHere; j++)
                {
                    // Calculate position to spawn enemy
                    Vector3 startingPosition = spawnTile.transform.position;
                    Vector3 enemySpawnPosition = startingPosition;
                    // y position is handled by enemy start function
                    enemySpawnPosition.x += (j % 2 == 0) ? -.25f : .25f;
                    enemySpawnPosition.z += ((j + 1) % 2 == 0) ? -.25f : .25f;

                    Unit newUnit;
                    if (j == 0 && enemiesToSpawnHere == 4) // we can afford a heavy invader
                    {
                        float random = Random.Range(0f, 1f);
                        if (random < 0.20f)
                        {
                            enemySpawnPosition = spawnTile.transform.position;
                            newUnit = Instantiate(unitPrefabs[1], enemySpawnPosition, Quaternion.identity);
                            unitsLeftToSpawn -= 4;
                            j = 3;
                        }
                        else
                        {
                            newUnit = Instantiate(unitPrefabs[0], enemySpawnPosition, Quaternion.identity);
                            newUnit.SetScale(Random.Range(1.5f, 2f));
                            unitsLeftToSpawn--;
                        }
                    }
                    else // we can't afford a heavy invader
                    {
                        newUnit = Instantiate(unitPrefabs[0], enemySpawnPosition, Quaternion.identity);
                        newUnit.SetScale(Random.Range(1.5f, 2f));
                        unitsLeftToSpawn--;
                    }
                    lastEnemySpawnedPosition = newUnit.transform.position;
                    units.Add(newUnit);
                }
            }

            // The start tile plays the spawn effect
            GameManager.CreateAudioEffect("horn", lastEnemySpawnedPosition);

            // Next wave is bigger
            unitsPerWave += newUnitsPerWave;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.RightBracket))
        {
            spawning = true;
            cooldown = 0f;
        }
    }

    public int GetKillCount()
    {
        return unitsKilled;
    }

    public void SetKillCount(int _killCount)
    {
        unitsKilled = _killCount;
    }



    public TileBehaviour GetRandomSpawnTile()
    {
        return availableTiles[Random.Range(0, availableTiles.Count)];
    }

    public void LoadUnit(SuperManager.UnitSaveData unitSaveData)
    {
        Unit unit = Instantiate(unitPrefabs[0]); // TODO Set prefab id
        unit.transform.position = unitSaveData.position;
        unit.transform.rotation = unitSaveData.orientation;
        unit.SetScale(unitSaveData.scale);
        unit.SetUnitType(unitSaveData.unitType);

        IDamageable damageable = null;
        if (Physics.Raycast(unitSaveData.targetPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
        {
            if (hit.collider.transform.GetComponent<IDamageable>() != null) damageable = hit.collider.gameObject.GetComponent<IDamageable>();
        }

        if (damageable != null) unit.SetTarget(damageable);

        unit.SetState(UnitState.IDLE);
        units.Add(unit);
    }

    public void OnDeath(Unit _unit)
    {
        unitsKilled++;
        if (units.Contains(_unit)) units.Remove(_unit);
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
}
