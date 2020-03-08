using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    public bool wasPlacedOnForest = false;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
        wasPlacedOnForest = false;
        resourceType = ResourceType.wood;
        structureName = "Lumber Mill";
    }

    // Update is called once per frame
    void Update()
    {
        ResourceUpdate();
    }
    public void CalculateTileBonus()
    {
        tileBonus = 1;
        if (wasPlacedOnForest)
        {
            tileBonus++;
        }

        // If the Lumber Mill is placed on a tile...
        if (attachedTile)
        {
            TileBehaviour tileBehaviour = attachedTile.GetComponent<TileBehaviour>();

            // For each possible tile
            for (int i = 0; i < 4; i++)
            {
                if (tileBehaviour.adjacentTiles.ContainsKey((TileBehaviour.TileCode)i))
                {
                    GameObject adjStructure = tileBehaviour.adjacentTiles[(TileBehaviour.TileCode)i].GetAttached();
                    // If there is a structure on the tile...
                    if (adjStructure)
                    {
                        if (adjStructure.GetComponent<Structure>().IsStructure("Forest Environment"))
                        { tileBonus++; }
                    }
                }
            }
        }
        Debug.Log("New tile bonus for " + gameObject.ToString() + " is " + tileBonus.ToString());
    }


}
