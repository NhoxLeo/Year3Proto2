using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabrication")]
    public GameObject[] enemies;

    [Header("Variables")]
    public int enemiesPerWave = 8;

    private TileBehaviour[] tileBehaviours;
    private float cooldown = 1.0f;

    private void Start()
    {
        tileBehaviours = FindObjectsOfType<TileBehaviour>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;
        
        if(cooldown <= 0.0f)
        {
            cooldown = 10.0f;

            List<TileBehaviour> availableTiles = GetAvailableTiles();

            if (availableTiles.Count > 1)
            {
                TileBehaviour tileBehaviour = GetAvailableTile(availableTiles);
                if (tileBehaviour != null)
                {
                    for(int i = 0; i < enemiesPerWave; i++)
                    {
                        Vector3 position = tileBehaviour.transform.position;
                        position.y += 0.55f;

                        GameObject @object = Instantiate(enemies[Random.Range(0, enemies.Length)], position, Quaternion.identity, transform);

                        Vector2 enemyPosition = CalcPosition(i, 4, 0.3f);

                        position.x += enemyPosition.x;
                        position.z += enemyPosition.y;

                        @object.transform.position = position;
                        @object.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
                    }
                }
            }
        }
    }

    public TileBehaviour GetAvailableTile(List<TileBehaviour> tileBehaviours)
    {
        TileBehaviour tileBehaviour = tileBehaviours[Random.Range(0, tileBehaviours.Count - 1)];
        if (tileBehaviour == null) return null;

        foreach (KeyValuePair<TileBehaviour.TileCode, TileBehaviour> keyValuePair in tileBehaviour.adjacentTiles)
        {
            if(keyValuePair.Value.attachedStructure)
            {
                return GetAvailableTile(tileBehaviours);
            }
        }

        return tileBehaviour;
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

    Vector2 CalcPosition(int index, int columns, float space)
    {
        float posX = (index % columns) * space;
        float posY = (index / columns) * space;
        return new Vector2(posX, posY);
    }
}
