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
    [SerializeField] private float weightageScalar = 0.2f;
    [SerializeField] private float tokensScalar = 0.05f;
    [SerializeField] private Vector2 timeVariance = new Vector2(20, 80);

    [Header("Enemies")]
    [SerializeField] private List<Transform> enemyPrefabs;
    [SerializeField] private float maxEnemies = 300.0f;
    [SerializeField] private int enemiesPerAirship = 9;

    [Header("Airship")]
    [SerializeField] private AirshipSpawner airshipSpawner;

    [SerializeField] private float time = 0.0f;
    private float tokens = 0.0f;
    private float tokenIncrement = 0.0f;

    private MessageBox messageBox;


    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Start()
    {
        messageBox = FindObjectOfType<MessageBox>();
    }

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        time -= Time.deltaTime;
        if(time <= 0.0f)
        {
            tokenIncrement += tokensScalar;
            time = Random.Range(timeVariance.x, timeVariance.y);

            float enemiesToSpawn = tokens + GetWeightage();
            enemiesToSpawn = Mathf.Clamp(enemiesToSpawn, 0.0f, maxEnemies);

            Transform[] dedicatedEnemies = DedicateEnemies((int) enemiesToSpawn);
            Debug.Log("Dedicated Enemies: " + dedicatedEnemies.Length);

            if (dedicatedEnemies.Length > 0)
            {
                //Dedicate enemies to airships
                List<Transform[]> dedicatedAirships = DedicateAirships(dedicatedEnemies);
                Debug.Log("Dedicated Airships: " + dedicatedAirships.Count);

                foreach (Transform[] transforms in dedicatedAirships) airshipSpawner.Spawn(transforms);

                messageBox.ShowMessage("Invaders incoming!", 3.5f);
                GameManager.CreateAudioEffect("horn", transform.position);
            }

            tokens = 0.0f;
        }

        tokens += tokenIncrement * Time.deltaTime;
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

        if (enemies.Length > enemiesPerAirship)
        {
            for (int i = 0; i > enemies.Length; i++)
            {
                currentBatch[i] = enemies[i];
                if ((i % enemiesPerAirship) == 0)
                {
                    enemiesInAirships.Add(currentBatch);
                    currentBatch = new Transform[enemiesPerAirship];
                }
            }
        }
        if (enemiesInAirships.Count <= 0) enemiesInAirships.Add(currentBatch);
        return enemiesInAirships;
    }

    /**************************************
    * Name of the Function: DedicateEnemies
    * @Author: Tjeu Vreeburg
    * @Parameter: Integer
    * @Return: Transform Array
    ***************************************/
    private Transform[] DedicateEnemies(int enemiesLeft)
    {
        Transform[] enemies = new Transform[enemiesLeft];

        for (int i = 0; i < enemiesLeft; i++)
        {
            float random = Random.Range(0.0f, 1.0f);
            if (random < 0.20f)
            {
                enemies[i] = enemyPrefabs[1];
                enemiesLeft -= 1;
            }
            else
            {
                enemies[i] = enemyPrefabs[0];
                enemiesLeft -= 1;
            }
        }
        return enemies;
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
                int researchCompleted = superManager.GetResearch().ToList().RemoveAll(entry => !entry.Value); // 5 out of 20
                int currentStructures = FindObjectsOfType<ResourceStructure>().Length; //10
                return researchCompleted + currentStructures * weightageScalar;
            }
        }
        return 0;
    }
}
