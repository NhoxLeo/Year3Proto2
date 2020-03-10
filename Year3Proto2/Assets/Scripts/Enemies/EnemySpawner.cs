using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabrication")]
    public GameObject[] enemies;

    [Header("Variables")]
    public int enemiesPerWave = 8;
    public float cooldown = 1.0f;

    private TileBehaviour[] tileBehaviours;

    private List<EnemyWave> enemyWaves;
    public EnemyWave enemyWave;

    private void Start()
    {
        enemyWaves = new List<EnemyWave>();
        tileBehaviours = FindObjectsOfType<TileBehaviour>();
    }

    private void FixedUpdate()
    {
        cooldown -= Time.deltaTime;
        if (cooldown <= 0.0f)
        {
            cooldown = 30.0f;

            EnemyWave enemyWave = Instantiate(this.enemyWave, transform.parent);
            enemyWave.Initialize(GetAvailableTiles(), enemiesPerWave);
            enemyWaves.Add(enemyWave);
        }

        enemyWaves.ForEach(enemyWave => enemyWave.Check(enemyWaves));
    }

    public List<TileBehaviour> GetAvailableTiles()
    {
        List<TileBehaviour> tileBehaviours = new List<TileBehaviour>();

        foreach(TileBehaviour tileBehaviour in this.tileBehaviours)
        {
            if (tileBehaviour.GetAttached() == null) tileBehaviours.Add(tileBehaviour);
        }

        return tileBehaviours;
    }

}
