using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

    private float time = 0.0f;
    private float tokens = 0.0f;
    private float tokenIncrement = 0.0f;

    private MessageBox messageBox;

    private void Start()
    {
        messageBox = FindObjectOfType<MessageBox>();
    }

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
            if (dedicatedEnemies.Length > 0)
            {
                //Dedicate enemies to airships
                //List<Transform> dedicatedAirships = DedicateAirships(dedicatedEnemies);
                //if(dedicatedAirships.Count > 0) airshipSpawner.Spawn(dedicatedAirships);

                messageBox.ShowMessage("Invaders incoming!", 3.5f);
                GameManager.CreateAudioEffect("horn", transform.position);
            }

            tokens = 0.0f;
        }

        tokens += tokenIncrement * Time.deltaTime;
    }

    private List<Transform[]> DedicateAirships(Transform[] enemies)
    {
        List<Transform[]> enemiesInAirships = new List<Transform[]>();
        Transform[] currentBatch = new Transform[enemiesPerAirship];
        for(int i = 0; i > enemies.Length; i++)
        {
            if((i % enemiesPerAirship) == 0)
            {
                enemiesInAirships.Add(currentBatch);
                currentBatch = new Transform[enemiesPerAirship];
            }
        }
        return enemiesInAirships;
    }

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
