using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    private float tileBonus = 0f;
    public bool wasPlacedOnForest = false;
    public float productionTime = 3f;
    private float remainingTime = 3f;
    private int batchSize = 5;


    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        resourceType = ResourceType.wood;
    }

    // Update is called once per frame
    void Update()
    {
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = productionTime;
            FindObjectOfType<GameManager>().AddWood(batchSize);
        }
    }

    public void CalculateTileBonus()
    {
        float calculatedTileBonus = 1f;

        // If the Lumber Mill is placed on a tile...
        if (attachedTile)
        {
            TileBehaviour tileBehaviour = attachedTile.GetComponent<TileBehaviour>();

            // For each possible tile
            for (int i = 0; i < 4; i++)
            {
                if (tileBehaviour.adjacentTiles.ContainsKey((TileBehaviour.TileCode)i))
                {
                    TileBehaviour adjTile = tileBehaviour.adjacentTiles[(TileBehaviour.TileCode)i];
                    GameObject adjStructure = adjTile.GetAttached();
                    // If there is a structure on the tile...
                    if (adjStructure)
                    {
                        // If that structure is an environment structure...
                        if (adjStructure.GetComponent<Structure>().GetStructureType() == StructureType.environment)
                        {
                            // If the environment structure is a forest...
                            if (adjStructure.GetComponent<EnvironmentStructure>().GetEnvironmentType() == EnvironmentStructure.EnvironmentType.forest)
                            {
                                calculatedTileBonus++;
                            }
                        }
                    }
                }
            }   
        }
        tileBonus = calculatedTileBonus;
        Debug.Log(tileBonus);
    }


}
