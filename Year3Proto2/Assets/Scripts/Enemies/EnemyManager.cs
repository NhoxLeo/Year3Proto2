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

public struct EnemyLevelSetting
{
    public string enemy;
    public int enemyLevel;
    public int level;
    public int wave;
    public EnemyLevelSetting(int _level, int _wave, string _enemy, int _enemyLevel)
    {
        enemy = _enemy;
        enemyLevel = _enemyLevel;
        level = _level;
        wave = _wave;
    }
}

public static class EnemyNames
{
    public static string Invader = "Invader";
    public static string HeavyInvader = "Heavy Invader";
    //public static string FlyingInvader = "Flying Invader";
    //public static string ExplosiveInvader = "Petard";
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
    [SerializeField] private Vector2 timeVariance = new Vector2(20, 80);
    [SerializeField] private bool spawning = false;

    [Header("Enemies")]
    [SerializeField] private List<Transform> enemyPrefabs;
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
    private int waveCounter = 0;

    public static Dictionary<string, EnemyDefinition> Enemies = new Dictionary<string, EnemyDefinition>()
    {
        { EnemyNames.Invader, new EnemyDefinition(1.0f, 1) },
        { EnemyNames.HeavyInvader, new EnemyDefinition(0.25f, 4) }
    };

    private readonly List<EnemyLevelSetting> levelSettings = new List<EnemyLevelSetting>
    {
        // Level 1
        new EnemyLevelSetting(0, 1, EnemyNames.Invader, 1),
        new EnemyLevelSetting(0, 5, EnemyNames.HeavyInvader, 1),
    };

    private Dictionary<string, (bool, int)> currentSettings = new Dictionary<string, (bool, int)>();

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
        TileBehaviour[] tileBehaviours = FindObjectsOfType<TileBehaviour>();
        for (int i = 0; i < tileBehaviours.Length; i++)
        {
            float distance = (tileBehaviours[i].transform.position - transform.position).sqrMagnitude;
            if (distance > this.distance) this.distance = distance;
        }
        distance = Mathf.Sqrt(distance) + radiusOffset;

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
        Vector3 location = new Vector3(Mathf.Sin(Random.Range(0.0f, 180f)) * distance, 0.0f, Mathf.Cos(Random.Range(0.0f, 180f)) * distance);

        Transform instantiatedAirship = Instantiate(airshipPrefab, location, Quaternion.identity, transform);

        Airship airship = instantiatedAirship.GetComponent<Airship>();
        if (airship) {
            if (airship.HasTarget()) {
                airship.Embark(transforms, pointerParent);
            }
        }
    }

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg, Samuel Fortune
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.RightBracket))
        {
            if (tokens < 10f)
            {
                tokens = 10f;
            }
            spawning = true;
            time = 0f;
        }
        if (spawning)
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                waveCounter++;

                UpdateSpawnSettings();

                time = Random.Range(timeVariance.x, timeVariance.y);

                float enemiesToSpawn = tokens * (1f + GetWeightage());
                enemiesToSpawn = Mathf.Clamp(enemiesToSpawn, minEnemies, maxEnemies);

                Transform[] dedicatedEnemies = DedicateEnemies((int)enemiesToSpawn);

                if (dedicatedEnemies.Length > 0)
                {
                    //Dedicate enemies to airships
                    List<Transform[]> dedicatedAirships = DedicateAirships(dedicatedEnemies);
                    for (int i = 0; i < dedicatedAirships.Count; i++)
                    {
                        SpawnAirship(dedicatedAirships[i]);
                    }

                    messageBox.ShowMessage("Invaders incoming!", 3.5f);
                    GameManager.CreateAudioEffect("horn", transform.position);
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

        for (int i = 0; i < enemies.Length; i++)
        {
            currentBatch[i % enemiesPerAirship] = enemies[i];
            if (((i + 1) % enemiesPerAirship) == 0)
            {
                enemiesInAirships.Add(currentBatch);
                currentBatch = new Transform[enemiesPerAirship];
            }
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
    * @Description: Certain parameters of the EnemyWaveSystem, token data in particular, need to be loaded and saved.
    ***************************************/
    public void LoadSystemFromData(SuperManager.MatchSaveData _data)
    {
        spawning = _data.spawning;

        weightageScalar = _data.waveSystemWeightageScalar;
        tokenIncrement = _data.waveSystemTokenIncrement;
        tokensScalar = _data.waveSystemTokenScalar;
        time = _data.waveSystemTime;
        timeVariance = _data.waveSystemTimeVariance;
        tokens = _data.waveSystemTokens;
    }

    /**************************************
    * Name of the Function: SaveSystemToData
    * @Author: Samuel Fortune
    * @Parameter: ref SuperManager.MatchSaveData _data, the data to save information to
    * @Return: void
    * @Description: Allows the SuperManager to load the state of the EnemyWaveSystem when it loads a match from file.
    ***************************************/
    public void SaveSystemToData(ref SuperManager.MatchSaveData _data)
    {
        _data.spawning = spawning;

        _data.waveSystemWeightageScalar = weightageScalar;
        _data.waveSystemTokenIncrement = tokenIncrement;
        _data.waveSystemTokenScalar = tokensScalar;
        _data.waveSystemTime = time;
        _data.waveSystemTimeVariance = new SuperManager.SaveVector3(timeVariance);
        _data.waveSystemTokens = tokens;
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

    public void OnEnemyDeath(Enemy _enemy)
    {
        enemiesKilled++;
        if (enemies.Contains(_enemy))
        {
            enemies.Remove(_enemy);
        }
    }

    public Enemy[] GetEnemies()
    {
        return enemies.ToArray();
    }

    public int GetWaveCurrent()
    {
        return waveCounter;
    }

    public void SetWave(int _wave)
    {
        waveCounter = _wave;
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
        foreach (EnemyLevelSetting setting in levelSettings)
        {
            if (setting.level == SuperManager.GetInstance().GetCurrentLevel())
            {
                if (setting.wave == waveCounter)
                {
                    currentSettings[setting.enemy] = (setting.enemyLevel != 0, setting.enemyLevel);
                }
            }
        }
    }
}
