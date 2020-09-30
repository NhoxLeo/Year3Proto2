using UnityEngine;
using System.Linq;
using System.Collections.Generic;

//
// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : EnemyManager.cs
// Description  : Dedicates waves of enemies to airships, amongst other responsibilites.
// Author       : Tjeu Vreeburg, Samuel Fortune
// Mail         : tjeu.vreeburg@gmail.com
//

public struct LevelSetting
{
    public string enemy;
    public int enemyLevel;
    public int level;
    public int wave;
    public LevelSetting(int _level, int _wave, string _enemy, int _enemyLevel)
    {
        enemy = _enemy;
        enemyLevel = _enemyLevel;
        level = _level;
        wave = _wave;
    }
}

public static class EnemyNames
{
    public const string Invader = "Invader";
    public const string HeavyInvader = "Heavy Invader";
    public const string FlyingInvader = "Flying Invader";
    public const string Petard = "Petard";
    public const string BatteringRam = "Battering Ram";
}

public static class EnemyMaterials
{
    private static Dictionary<(string, int), Material> materials = new Dictionary<(string, int), Material>();

    private static string GetPathFromKey((string, int) _key)
    {
        switch (_key.Item1)
        {
            case EnemyNames.Invader:
                return "Materials/mInvader_Lvl" + _key.Item2.ToString();
            case EnemyNames.HeavyInvader:
                return "Materials/mHeavyInvader_Lvl" + _key.Item2.ToString();
            case EnemyNames.FlyingInvader:
                return "Materials/mFlyingInvaderSails_Lvl" + _key.Item2.ToString();
            case EnemyNames.Petard:
                return "Materials/mExplosiveInvader_Lvl" + _key.Item2.ToString();
            case EnemyNames.BatteringRam:
                return "Materials/mBatteringRam_Lvl" + _key.Item2.ToString();
            default:
                break;
        }
        return "";
    }

    public static Material Fetch(string _name, int _level)
    {
        (string, int) key = (_name, _level);
        if (!materials.ContainsKey(key))
        {
            materials.Add(key, Resources.Load(GetPathFromKey(key)) as Material);
        }
        return materials[key];
    }
}

public struct WaveData
{
    public int enemiesRemaining;

    public WaveData(int _enemies)
    {
        enemiesRemaining = _enemies;
    }

    public void ReportEnemyDead()
    {
        enemiesRemaining--;
    }

    public bool WaveSurvived()
    {
        return enemiesRemaining == 0;
    }
}

public struct EnemyDefinition
{
    public float spawnChance;
    public int tokenCost;
    private GameObject prefab;

    public EnemyDefinition(float _spawnChance, int _tokenCost)
    {
        spawnChance = _spawnChance;
        tokenCost = _tokenCost;
        prefab = null;
    }

    public void SetPrefab(GameObject _prefab)
    {
        prefab = _prefab;
    }

    public GameObject GetPrefab()
    {
        return prefab;
    }

    public bool AffordedBy(int _remainingTokens)
    {
        return tokenCost <= _remainingTokens;
    }
}

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager instance = null;

    [Header("Properties")]
    [SerializeField] private float weightageScalar = 0.01f; // 1% boost to tokens for each structure/research element
    [SerializeField] private float tokenIncrement = 0.05f; // 20 seconds to earn an Invader, 80 to earn a heavy, at base.
    [SerializeField] private float tokensScalar = 0.0001f; // 0.05f every 500 seconds
    [SerializeField] private float time = 90.0f;
    [SerializeField] private float tokens = 0.0f;
    [SerializeField] private Vector2 timeVariance = new Vector2(45, 90);
    [SerializeField] private bool spawning = false;

    [Header("Enemies")]
    [SerializeField] private int maxEnemies = 300;
    [SerializeField] private int minEnemies = 3;

    [Header("Airships")]
    [SerializeField] private Transform airshipPrefab;
    [SerializeField] private Transform pointerParent;
    [SerializeField] private int enemiesPerAirship = 9;
    [SerializeField] private float radiusOffset;
    [SerializeField] private float distance = 0.0f;

    private MessageBox messageBox;

    private readonly List<Enemy> enemies = new List<Enemy>();
    private int enemiesKilled = 0;
    private int wave = 0;

    public static Dictionary<string, EnemyDefinition> Enemies = new Dictionary<string, EnemyDefinition>()
    {
        { EnemyNames.Invader, new EnemyDefinition(1.0f, 1) },
        { EnemyNames.HeavyInvader, new EnemyDefinition(0.25f, 4) },
        { EnemyNames.FlyingInvader, new EnemyDefinition(0.25f, 4) },
        { EnemyNames.Petard, new EnemyDefinition(0.2f, 6) },
        { EnemyNames.BatteringRam, new EnemyDefinition(0.1f, 8) },
    };

    private readonly List<LevelSetting> levelSettings = new List<LevelSetting>
    {
        // Level 1 --------------------------------
        // wave 1
        new LevelSetting(0, 1, EnemyNames.Invader,         1),
        
        // wave 3
        new LevelSetting(0, 3, EnemyNames.HeavyInvader,    1),
        
        // wave 5
        new LevelSetting(0, 5, EnemyNames.Invader,         2),
        new LevelSetting(0, 5, EnemyNames.HeavyInvader,    0),
        new LevelSetting(0, 5, EnemyNames.Petard,          1),

        // wave 7
        new LevelSetting(0, 7, EnemyNames.HeavyInvader,    2),
        new LevelSetting(0, 7, EnemyNames.Petard,          0),
        
        // wave 11
        new LevelSetting(0, 11, EnemyNames.Petard,         1),

        // wave 15
        new LevelSetting(0, 15, EnemyNames.BatteringRam,   1),

        // Level 2 --------------------------------
        // wave 1
        new LevelSetting(1, 1, EnemyNames.Invader,         1),
        new LevelSetting(1, 1, EnemyNames.HeavyInvader,    1),
        
        // wave 3
        new LevelSetting(1, 3, EnemyNames.Invader,         2),
        new LevelSetting(1, 3, EnemyNames.HeavyInvader,    0),
        new LevelSetting(1, 3, EnemyNames.Petard,          1),
        
        // wave 5
        new LevelSetting(1, 5, EnemyNames.Invader,         0),
        new LevelSetting(1, 5, EnemyNames.HeavyInvader,    2),
        new LevelSetting(1, 5, EnemyNames.Petard,          0),
        new LevelSetting(1, 5, EnemyNames.FlyingInvader,   1),
        
        // wave 7
        new LevelSetting(1, 7, EnemyNames.Invader,         3),
        new LevelSetting(1, 7, EnemyNames.Petard,          2),

        // wave 9
        new LevelSetting(1, 9, EnemyNames.FlyingInvader,   2),
        new LevelSetting(1, 9, EnemyNames.BatteringRam,    1),
        
        // wave 11
        new LevelSetting(1, 11, EnemyNames.HeavyInvader,   3),
        
        // wave 13
        new LevelSetting(1, 13, EnemyNames.BatteringRam,   2),

        // Level 3 --------------------------------
        // wave 1
        new LevelSetting(2, 1, EnemyNames.Invader,         2),
        new LevelSetting(2, 1, EnemyNames.HeavyInvader,    1),
        new LevelSetting(2, 1, EnemyNames.Petard,          1),
        
        // wave 3
        new LevelSetting(2, 3, EnemyNames.HeavyInvader,    0),
        new LevelSetting(2, 3, EnemyNames.FlyingInvader,   1),
        
        // wave 5
        new LevelSetting(2, 5, EnemyNames.Invader,         3),
        new LevelSetting(2, 5, EnemyNames.Petard,          2),
        new LevelSetting(2, 5, EnemyNames.BatteringRam,    1),
        
        // wave 7
        new LevelSetting(2, 7, EnemyNames.HeavyInvader,    3),
        new LevelSetting(2, 7, EnemyNames.Petard,          0),
        new LevelSetting(2, 7, EnemyNames.FlyingInvader,   2),
        
        // wave 9
        new LevelSetting(2, 9, EnemyNames.Invader,         0),
        new LevelSetting(2, 9, EnemyNames.Petard,          3),
        new LevelSetting(2, 9, EnemyNames.FlyingInvader,   0),
        new LevelSetting(2, 9, EnemyNames.BatteringRam,    2),
        
        // wave 11
        new LevelSetting(2, 11, EnemyNames.Invader,        3),
        new LevelSetting(2, 11, EnemyNames.FlyingInvader,  3),

        // Level 4 --------------------------------
        // wave 1
        new LevelSetting(3, 1, EnemyNames.Invader,         2),
        new LevelSetting(3, 1, EnemyNames.HeavyInvader,    2),
        new LevelSetting(3, 1, EnemyNames.Petard,          1),
        new LevelSetting(3, 1, EnemyNames.FlyingInvader,   1),
        
        // wave 3
        new LevelSetting(3, 3, EnemyNames.Petard,          2),
        new LevelSetting(3, 3, EnemyNames.FlyingInvader,   2),
        new LevelSetting(3, 3, EnemyNames.BatteringRam,    1),
        
        // wave 5
        new LevelSetting(3, 5, EnemyNames.Invader,         3),
        new LevelSetting(3, 5, EnemyNames.HeavyInvader,    3),
        new LevelSetting(3, 5, EnemyNames.BatteringRam,    2),
        
        // wave 7
        new LevelSetting(3, 7, EnemyNames.Petard,          3),
        new LevelSetting(3, 7, EnemyNames.FlyingInvader,   3),

        // wave 9
        new LevelSetting(3, 9, EnemyNames.BatteringRam,    3),
    };
    private readonly Dictionary<int, List<LevelSetting>> sortedLevelSettings = new Dictionary<int, List<LevelSetting>>()
    {
        {0, new List<LevelSetting>() },
        {1, new List<LevelSetting>() },
        {2, new List<LevelSetting>() },
        {3, new List<LevelSetting>() }
    };

    private readonly Dictionary<string, (bool, int)> currentSettings = new Dictionary<string, (bool, int)>();
    private readonly Dictionary<int, WaveData> waveEnemyCounts = new Dictionary<int, WaveData>();

    public static EnemyManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
        List<string> keys = new List<string>();

        foreach (string key in Enemies.Keys)
        {
            keys.Add(key);
        }

        foreach (string key in keys)
        {
            EnemyDefinition temp = Enemies[key];
            temp.SetPrefab(Resources.Load("Enemies/" + key) as GameObject);
            Enemies[key] = temp;
            currentSettings.Add(key, (false, 0));
        }

        int index = 0;
        // sort level settings
        for (int i = 0; i < 4; i++)
        {
            List<LevelSetting> levelI = sortedLevelSettings[i];
            bool loopCondition = true;
            while (loopCondition)
            {
                if (index < levelSettings.Count)
                {
                    if (levelSettings[index].level == i)
                    {
                        levelI.Add(levelSettings[index++]);
                    }
                    else
                    {
                        loopCondition = false;
                    }
                }
                else
                {
                    loopCondition = false;
                }
            }
            sortedLevelSettings[i] = levelI;
        }
    }

    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Start()
    {
        messageBox = FindObjectOfType<MessageBox>();
        TileBehaviour[] tiles = FindObjectsOfType<TileBehaviour>();
        for (int i = 0; i < tiles.Length; i++)
        {
            float newDistance = (tiles[i].transform.position - transform.position).magnitude;
            if (newDistance > distance)
            {
                distance = newDistance;
            }
        }
        distance += radiusOffset;

    }

    /**************************************
    * Name of the Function: GetSpawning
    * @Author: Samuel Fortune
    * @Parameter: n/a
    * @Return: bool, value of spawning
    * @Description: Getter method for the spawning member.
    ***************************************/
    public bool GetSpawning()
    {
        return spawning;
    }

    /**************************************
    * Name of the Function: SetSpawning
    * @Author: Samuel Fortune
    * @Parameter: bool _spawning, the value to set spawning to.
    * @Return: void
    * @Description: Setter method for the spawning member.
    ***************************************/
    public void SetSpawning(bool _spawning)
    {
        spawning = _spawning;
    }

    /**************************************
    * Name of the Function: SpawnAirship
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SpawnAirship(Transform[] transforms)
    {
        float random = Random.Range(-180.0f, 180f);
        Vector3 location = new Vector3(Mathf.Sin(random) * distance, 0.0f, Mathf.Cos(random) * distance);

        Transform instantiatedAirship = Instantiate(airshipPrefab, location, Quaternion.identity, transform);

        Airship airship = instantiatedAirship.GetComponent<Airship>();
        if (airship)
        {
            airship.spawnWave = wave;
            if (airship.GetTarget())
            {
                airship.Embark(transforms, pointerParent);
            }
        }
    }


    public Transform[] SpawnFlyingInvaders(Transform[] transforms)
    {
        List<Transform> flyingInvaders = new List<Transform>();
        List<Transform> remainingEnemies = new List<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].gameObject.GetComponent<FlyingInvader>())
            {
                flyingInvaders.Add(transforms[i]);
            }
            else
            {
                remainingEnemies.Add(transforms[i]);
            }
        }
        foreach (Transform transform in flyingInvaders)
        {
            float random = Random.Range(-180.0f, 180f);
            Vector3 location = new Vector3(Mathf.Sin(random) * distance * 0.75f, 0.0f, Mathf.Cos(random) * distance * 0.75f);

            FlyingInvader enemy = Instantiate(transform.gameObject, location, Quaternion.identity).GetComponent<FlyingInvader>();
            enemy.SetSpawnWave(wave);
            enemy.Initialize(GetEnemyCurrentLevel(EnemyNames.FlyingInvader));
            RecordNewEnemy(enemy);
        }
        return remainingEnemies.ToArray();
    }

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg, Samuel Fortune
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        if (SuperManager.DevMode)
        {
            if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.RightBracket))
            {
                SpawnNextWave();
            }
        }
        if (spawning)
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                wave++;

                UpdateSpawnSettings();

                time = Random.Range(timeVariance.x, timeVariance.y);

                float enemiesToSpawn = tokens * (1f + GetWeightage());
                enemiesToSpawn = Mathf.Clamp(enemiesToSpawn, minEnemies, maxEnemies);

                Transform[] dedicatedEnemies = DedicateEnemies((int)enemiesToSpawn);
                waveEnemyCounts.Add(wave, new WaveData(dedicatedEnemies.Length));

                // first spawn flying invaders
                Transform[] remaining = SpawnFlyingInvaders(dedicatedEnemies);

                if (remaining.Length > 0)
                {
                    //Dedicate enemies to airships
                    List<Transform[]> dedicatedAirships = DedicateAirships(remaining);
                    for (int i = 0; i < dedicatedAirships.Count; i++)
                    {
                        SpawnAirship(dedicatedAirships[i]);
                    }

                    messageBox.ShowMessage("Invaders incoming!", 3.5f);
                }

                tokens = 0.0f;
            }

            tokenIncrement += tokensScalar * Time.deltaTime;
            tokens += tokenIncrement * Time.deltaTime;
        }
    }

    /**************************************
    * Name of the Function: DedicateAirships
    * @Author: Tjeu Vreeburg
    * @Parameter: Transform Array
    * @Return: List of Transform Arrays
    ***************************************/
    private List<Transform[]> DedicateAirships(Transform[] enemies)
    {
        List<Transform[]> enemiesInAirships = new List<Transform[]>();
        Transform[] currentBatch = new Transform[enemiesPerAirship];

        // for every enemy
        for (int i = 0; i < enemies.Length; i++)
        {
            // add this enemy to the batch/ship
            currentBatch[i % enemiesPerAirship] = enemies[i];
            // if the batch/ship is full
            if (((i + 1) % enemiesPerAirship) == 0)
            {
                // add this batch/ship to the list 
                enemiesInAirships.Add(currentBatch);
                // define the next batch/ship 
                currentBatch = new Transform[enemiesPerAirship];
            }
        }
        // if the currentBatch has any enemies in it
        if (currentBatch[0])
        {
            // add that batch as an airship
            enemiesInAirships.Add(currentBatch);
        }
        return enemiesInAirships;
    }

    /**************************************
    * Name of the Function: DedicateEnemies
    * @Author: Tjeu Vreeburg, Samuel Fortune
    * @Parameter: Integer
    * @Return: Transform Array
    ***************************************/
    private Transform[] DedicateEnemies(int _enemiesLeftTokens)
    {
        int tokensLeft = _enemiesLeftTokens;
        List<Transform> enemies = new List<Transform>();

        List<EnemyDefinition> enemiesThisWave = new List<EnemyDefinition>();

        foreach (string key in Enemies.Keys)
        {
            bool canSpawnEnemy = currentSettings[key].Item1;
            if (canSpawnEnemy)
            {
                enemiesThisWave.Add(Enemies[key]);
            }
        }
        EnemyDefinition cheapestEnemy = enemiesThisWave[0];
        if (enemiesThisWave.Count > 1)
        {
            foreach (EnemyDefinition definition in enemiesThisWave)
            {
                if (definition.tokenCost < cheapestEnemy.tokenCost)
                {
                    cheapestEnemy = definition;
                }
            }
        }

        while (tokensLeft >= cheapestEnemy.tokenCost) // while the system can still afford an enemy
        {
            // define randomMax
            float randomMax = cheapestEnemy.spawnChance;

            foreach (EnemyDefinition enemy in enemiesThisWave)
            {
                bool canAffordEnemy = enemy.AffordedBy(tokensLeft);
                if (canAffordEnemy)
                {
                    randomMax += enemy.spawnChance;
                }
            }

            // define spawnNumber
            float spawnNumber = Random.Range(0f, randomMax);
            bool enemySpawned = false;
            foreach (EnemyDefinition enemy in enemiesThisWave)
            {
                bool canAffordEnemy = enemy.AffordedBy(tokensLeft);
                if (canAffordEnemy)
                {
                    spawnNumber -= enemy.spawnChance;
                    if (spawnNumber <= 0f)
                    {
                        enemies.Add(enemy.GetPrefab().transform);
                        tokensLeft -= enemy.tokenCost;
                        enemySpawned = true;
                        break;
                    }
                }
            }
            if (enemySpawned)
            {
                continue;
            }

            spawnNumber -= cheapestEnemy.spawnChance;
            if (spawnNumber <= 0f)
            {
                enemies.Add(cheapestEnemy.GetPrefab().transform);
                tokensLeft -= cheapestEnemy.tokenCost;
            }
        }
        return enemies.ToArray();
    }

    /**************************************
    * Name of the Function: GetWeightage
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: float
    ***************************************/
    private float GetWeightage()
    {
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan)
        {
            // Data is serialized correctly
            if (superMan.CheckData())
            {
                int researchCompleted = superMan.GetResearch().ToList().RemoveAll(entry => !entry.Value);
                int currentStructures = StructureManager.GetInstance().GetPlayerStructureCount();
                return (researchCompleted + currentStructures) * weightageScalar;
            }
        }
        return 0;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, distance);
    }

    /**************************************
    * Name of the Function: LoadSystemFromData
    * @Author: Samuel Fortune
    * @Parameter: SuperManager.MatchSaveData _data, the data to load information from
    * @Return: void
    * @Description: Certain parameters of the EnemyManager, token data in particular, need to be loaded and saved.
    ***************************************/
    public void LoadData(SuperManager.MatchSaveData _data)
    {
        spawning = _data.spawning;
        wave = _data.wave;
        weightageScalar = _data.weightageScalar;
        tokenIncrement = _data.tokenIncrement;
        tokensScalar = _data.tokenScalar;
        time = _data.time;
        timeVariance = _data.timeVariance;
        tokens = _data.tokens;
        enemiesKilled = _data.enemiesKilled;
    }

    /**************************************
    * Name of the Function: SaveSystemToData
    * @Author: Samuel Fortune
    * @Parameter: ref SuperManager.MatchSaveData _data, the data to save information to
    * @Return: void
    * @Description: Allows the SuperManager to load the state of the EnemyManager when it loads a match from file.
    ***************************************/
    public void SaveSystemToData(ref SuperManager.MatchSaveData _data)
    {
        _data.spawning = spawning;
        _data.enemiesKilled = enemiesKilled;
        _data.spawnTime = time;
        _data.wave = wave;
        _data.weightageScalar = weightageScalar;
        _data.tokenIncrement = tokenIncrement;
        _data.tokenScalar = tokensScalar;
        _data.time = time;
        _data.timeVariance = new SuperManager.SaveVector3(timeVariance);
        _data.tokens = tokens;
    }

    public int GetEnemiesAlive()
    {
        return enemies.Count;
    }

    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }

    public void SetEnemiesKilled(int _killed)
    {
        enemiesKilled = _killed;
    }

    public void RecordNewEnemy(Enemy _enemy)
    {
        enemies.Add(_enemy);
    }

    public void LoadInvader(SuperManager.InvaderSaveData _saveData)
    {
        Invader enemy = Instantiate(Enemies[EnemyNames.Invader].GetPrefab()).GetComponent<Invader>();

        enemy.Initialize(_saveData.enemyData.level, _saveData.scale);
        enemy.transform.position = _saveData.enemyData.position;
        enemy.transform.rotation = _saveData.enemyData.orientation;
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.enemyData.targetPosition));
        enemy.SetState(_saveData.enemyData.state);
        enemy.SetSpawnWave(_saveData.enemyData.enemyWave);
        enemy.SetHealth(_saveData.enemyData.health);

        enemies.Add(enemy);
    }

    public void LoadHeavyInvader(SuperManager.HeavyInvaderSaveData _saveData)
    {
        HeavyInvader enemy = Instantiate(Enemies[EnemyNames.HeavyInvader].GetPrefab()).GetComponent<HeavyInvader>();

        enemy.Initialize(_saveData.enemyData.level, _saveData.equipment);
        enemy.transform.position = _saveData.enemyData.position;
        enemy.transform.rotation = _saveData.enemyData.orientation;
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.enemyData.targetPosition));
        enemy.SetState(_saveData.enemyData.state);
        enemy.SetSpawnWave(_saveData.enemyData.enemyWave);
        enemy.SetHealth(_saveData.enemyData.health);

        enemies.Add(enemy);
    }

    public void LoadFlyingInvader(SuperManager.EnemySaveData _saveData)
    {
        FlyingInvader enemy = Instantiate(Enemies[EnemyNames.FlyingInvader].GetPrefab()).GetComponent<FlyingInvader>();

        enemy.Initialize(_saveData.level);
        enemy.transform.position = _saveData.position;
        enemy.transform.rotation = _saveData.orientation;
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.targetPosition));
        enemy.SetState(_saveData.state);
        enemy.SetSpawnWave(_saveData.enemyWave);
        enemy.SetHealth(_saveData.health);

        enemies.Add(enemy);
    }

    public void LoadPetard(SuperManager.EnemySaveData _saveData)
    {
        Petard enemy = Instantiate(Enemies[EnemyNames.Petard].GetPrefab()).GetComponent<Petard>();

        enemy.Initialize(_saveData.level);
        enemy.transform.position = _saveData.position;
        enemy.transform.rotation = _saveData.orientation;
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.targetPosition));
        enemy.SetState(_saveData.state);
        enemy.SetSpawnWave(_saveData.enemyWave);
        enemy.SetHealth(_saveData.health);

        enemies.Add(enemy);
    }

    public void LoadRam(SuperManager.EnemySaveData _saveData)
    {
        BatteringRam enemy = Instantiate(Enemies[EnemyNames.BatteringRam].GetPrefab()).GetComponent<BatteringRam>();

        enemy.Initialize(_saveData.level);
        enemy.transform.position = _saveData.position;
        enemy.transform.rotation = _saveData.orientation;
        enemy.SetTarget(StructureManager.FindStructureAtPosition(_saveData.targetPosition));
        enemy.SetState(_saveData.state);
        enemy.SetSpawnWave(_saveData.enemyWave);
        enemy.SetHealth(_saveData.health);

        enemies.Add(enemy);
    }

    public void OnEnemyDeath(Enemy _enemy)
    {
        enemiesKilled++;
        if (enemies.Contains(_enemy))
        {
            enemies.Remove(_enemy);
        }
        int wave = _enemy.GetSpawnWave();
        if (waveEnemyCounts.ContainsKey(wave))
        {
            WaveData data = waveEnemyCounts[wave];
            data.ReportEnemyDead();
            waveEnemyCounts[wave] = data;
        }
    }

    public Enemy[] GetEnemies()
    {
        return enemies.ToArray();
    }

    public int GetWaveCurrent()
    {
        return wave;
    }

    public void SetWave(int _wave)
    {
        wave = _wave;
    }

    public float GetTime()
    {
        return time;
    }

    public void SetTime(float _time)
    {
        time = _time;
    }

    private void UpdateSpawnSettings()
    {
        int currentLevel = SuperManager.GetInstance().GetCurrentLevel();
        foreach (LevelSetting setting in sortedLevelSettings[currentLevel])
        {
            if (setting.wave <= wave)
            {
                currentSettings[setting.enemy] = (setting.enemyLevel != 0, setting.enemyLevel);
            }
        }
    }

    public int GetWavesSurvived()
    {
        int total = 0;
        foreach (WaveData data in waveEnemyCounts.Values)
        {
            if (data.WaveSurvived())
            {
                total++;
            }
        }
        return total;
    }

    public bool GetWaveSurvived(int _wave)
    {
        if (waveEnemyCounts.ContainsKey(_wave))
        {
            if (waveEnemyCounts[_wave].WaveSurvived())
            {
                return true;
            }
        }
        else
        {
            if (wave > _wave)
            {
                return true;
            }
        }
        return false;
    }

    public int GetEnemyCurrentLevel(string _enemyName)
    {
        return currentSettings[_enemyName].Item2;
    }

    public void OnObjectiveComplete()
    {
        enemiesKilled = 0;
    }

    public void SpawnNextWave()
    {
        // get the time that would have passed
        float timeSkipped = time;
        time = 0f;

        // get the increase of the increment that would have occured
        float incrementIncrease = tokensScalar * timeSkipped;

        // add to tokens based on the time that's passed.
        tokens += (tokenIncrement + (incrementIncrease / 2f)) * timeSkipped;

        // increase the increment
        tokenIncrement += incrementIncrease;

        spawning = true;
    }

    public bool GetCurrentWaveSurvived()
    {
        return GetWaveSurvived(wave);
    }

    public bool CanSpawnNextWave()
    {
        return GetCurrentWaveSurvived() || GetWaveCurrent() == 0;
    }

    public int GetEnemiesLeftCurrentWave()
    {
        if (waveEnemyCounts.ContainsKey(wave))
        {
            return waveEnemyCounts[wave].enemiesRemaining;
        }
        return 0;
    }
}
