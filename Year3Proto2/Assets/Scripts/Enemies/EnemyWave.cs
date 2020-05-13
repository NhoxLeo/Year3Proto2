using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWave : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();

    private bool initialised = false;

    public Enemy[] enemyPrefabs;

    public int enemyCount
    {
        get
        {
            return enemies.Count;
        }
    }

    public void Check(List<EnemyWave> enemyWaves)
    {
        enemies.RemoveAll(enemy => enemy == null);

        if (enemies.Count <= 0 && initialised)
        {
            enemyWaves.Remove(this);
            Destroy(gameObject);
        }
    }

    public void Initialize(List<TileBehaviour> availableTiles, int enemiesPerWave)
    {
        if (availableTiles.Count > 1)
        {
            TileBehaviour tileBehaviour = GetAvailableTile(availableTiles);
            if (tileBehaviour != null)
            {
                GameManager.CreateAudioEffect("horn", tileBehaviour.transform.position, 1, false);
                for (int i = 0; i < enemiesPerWave; i++)
                {
                    Vector3 position = tileBehaviour.transform.position;
                    position.y += 0.55f;

                    Enemy enemy = Instantiate(enemyPrefabs[Random.Range(0, enemyPrefabs.Length)], position, Quaternion.identity, transform);

                    Vector2 enemyPosition = CalculatePosition(i, 4, 0.3f);

                    position.x += enemyPosition.x;
                    position.z += enemyPosition.y;

                    enemy.transform.position = position;
                    enemies.Add(enemy);
                }
            }
        }

        initialised = true;
    }

    public TileBehaviour GetAvailableTile(List<TileBehaviour> tileBehaviours)
    {
        return tileBehaviours[Random.Range(0, tileBehaviours.Count - 1)];
    }

    Vector2 CalculatePosition(int index, int columns, float space)
    {
        float posX = index % columns * space;
        float posY = index / columns * space;
        return new Vector2(posX, posY);
    }
}
