using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabrication")]
    public GameObject[] enemies;

    [Header("Variables")]
    public int enemiesPerWave = 8;
    public int newEnemiesPerWave = 4;
    public float cooldown = 30.0f;
    public float timeBetweenWaves = 30.0f;
    private int waveCounter = 0;
    private TileBehaviour[] tileBehaviours;
    private bool begin = false;

    private MessageBox messageBox;
    private List<EnemyWave> enemyWaves;
    public EnemyWave enemyWave;
    List<TileBehaviour> availableTiles;

    private void Start()
    {
        enemyWaves = new List<EnemyWave>();
        availableTiles = new List<TileBehaviour>();
        messageBox = FindObjectOfType<MessageBox>();
        foreach (TileBehaviour tileBehaviour in FindObjectsOfType<TileBehaviour>())
        {
            if (tileBehaviour.GetSpawnTile()) availableTiles.Add(tileBehaviour);
        }
    }

    private void FixedUpdate()
    {
        if (begin)
        {
            cooldown -= Time.deltaTime;
            if (cooldown <= 0.0f)
            {
                waveCounter++;
                if (waveCounter == 1)
                {
                    messageBox.ShowMessage("Invaders incoming!", 3.5f);
                }
                cooldown = timeBetweenWaves;
                enemiesPerWave += newEnemiesPerWave;
                EnemyWave enemyWave = Instantiate(this.enemyWave, transform);
                enemyWave.Initialize(availableTiles, enemiesPerWave);
                enemyWaves.Add(enemyWave);
            }

            enemyWaves.ForEach(enemyWave => enemyWave.Check(enemyWaves));
        }
    }

    public List<TileBehaviour> GetAvailableTiles()
    {
        return availableTiles;
    }

    public int GetWaveCurrent()
    {
        return waveCounter;
    }

    public void Begin()
    {
        begin = true;
    }

}
