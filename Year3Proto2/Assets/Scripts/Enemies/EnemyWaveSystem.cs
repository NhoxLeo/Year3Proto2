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
// File Name    : EnemyWaveSystem.cs
// Description  : Dedicates waves of enemies to airships.
// Author       : Tjeu Vreeburg 
// Mail         : tjeu.vreeburg@gmail.com
//

public class EnemyWaveSystem : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private EnemySpawner enemySpawner;
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
    private const int InvaderTokenCost = 1;
    private const int HeavyInvaderTokenCost = 4;

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
        float PiByTwo = Mathf.PI / 2f;
        Vector3 location = new Vector3(Mathf.Sin(Random.Range(0.0f, PiByTwo)) * distance, 0.0f, Mathf.Cos(Random.Range(0.0f, PiByTwo)) * distance)
        {
            y = 0.0f
        };
        //Vector3 location = new Vector3(-1f * distance, 0.0f, Mathf.Cos(angle) * distance);
        Transform instantiatedAirship = Instantiate(airshipPrefab, location, Quaternion.identity, transform);

        Airship airship = instantiatedAirship.GetComponent<Airship>();
        if (airship) {
            airship.SetSpawner(enemySpawner);
            if (airship.HasTarget()) {
                airship.Embark(transforms, pointerParent);
            }
        }
    }

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftBracket) && Input.GetKeyDown(KeyCode.RightBracket))
        {
            spawning = true;
            time = 0f;
        }
        if (spawning)
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                enemySpawner.SetWaveCurrent(enemySpawner.GetWaveCurrent() + 1);
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

        // When there are only enough enemies for one airship.
        if(enemies.Length < enemiesPerAirship)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                currentBatch[i] = enemies[i];
            }
            enemiesInAirships.Add(currentBatch);
            return enemiesInAirships;
        }

        // When there are more enemies to dedicate per airship.
        for (int i = 0; i < enemies.Length; i++)
        {
            if ((i % enemiesPerAirship) == 0)
            {
                enemiesInAirships.Add(currentBatch);
                currentBatch = new Transform[enemiesPerAirship];
            }
            currentBatch[i % enemiesPerAirship] = enemies[i];
        }
        return enemiesInAirships;
    }

    /**************************************
    * Name of the Function: DedicateEnemies
    * @Author: Tjeu Vreeburg
    * @Parameter: Integer
    * @Return: Transform Array
    ***************************************/
    private Transform[] DedicateEnemies(int _enemiesLeftTokens)
    {
        List<Transform> enemies = new List<Transform>();

        // spend 1 tokens to get an Invader, or (25% chance) try to spend 4 to get a Heavy Invader
        int tokensLeft = _enemiesLeftTokens;
        while (tokensLeft > InvaderTokenCost) // while the system can still afford an Invader
        {
            if (tokensLeft >= HeavyInvaderTokenCost)
            {
                if (Random.Range(0.0f, 1.0f) > 0.75f)
                {
                    enemies.Add(enemyPrefabs[1]);
                    tokensLeft -= HeavyInvaderTokenCost;
                }
                else
                {
                    enemies.Add(enemyPrefabs[0]);
                    tokensLeft -= InvaderTokenCost;
                }
            }
            else
            {
                enemies.Add(enemyPrefabs[0]);
                tokensLeft -= InvaderTokenCost;
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
        SuperManager superManager = SuperManager.GetInstance();
        if (superManager)
        {
            // Data is serialized correctly
            if (superManager.CheckData())
            {
                int researchCompleted = superManager.GetResearch().ToList().RemoveAll(entry => !entry.Value);
                int currentStructures = FindObjectsOfType<Structure>().Length;
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
}
