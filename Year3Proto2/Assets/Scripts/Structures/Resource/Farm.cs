using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : ResourceStructure
{
    public bool wasPlacedOnPlains = false;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        wasPlacedOnPlains = false;
        resourceType = ResourceType.food;
        structureName = "Farm";
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = productionTime;
            GameManager game = FindObjectOfType<GameManager>();
            game.AddBatch(new Batch(tileBonus * batchSize * 3, resourceType));
        }
    }

    public void CalculateTileBonus()
    {
        tileBonus = 1;
        if (wasPlacedOnPlains)
        {
            tileBonus++;
        }

        // If the Lumber Mill is placed on a tile...
        if (attachedTile)
        {
            // For each possible tile
            for (int i = 0; i < 4; i++)
            {
                if (attachedTile.adjacentTiles.ContainsKey((TileBehaviour.TileCode)i))
                {
                    Structure adjStructure = attachedTile.adjacentTiles[(TileBehaviour.TileCode)i].GetAttached();
                    // If there is a structure on the tile...
                    if (adjStructure)
                    {
                        if (adjStructure.IsStructure("Plains Environment"))
                        { tileBonus++; }
                    }
                }
            }
        }
        //Debug.Log("New tile bonus for " + gameObject.ToString() + " is " + tileBonus.ToString());
    }

    public override int GetProductionVolume()
    {
        return tileBonus * batchSize * 3;
    }
}
