using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberMill : ResourceStructure
{
    private float tileBonus = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ResourceStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CalculateTileBonus()
    {
        float calculatedTileBonus = 1f;

        if (attachedTile)
        {
            TileBehaviour tileBehaviour = attachedTile.GetComponent<TileBehaviour>();
            // If the tile has a tile north of it.
            if (tileBehaviour.adjacentTiles.ContainsKey(TileBehaviour.TileCode.north))
            {
                TileBehaviour northTile = tileBehaviour.adjacentTiles[TileBehaviour.TileCode.north];
                GameObject northStructure = northTile.GetAttached();
                if (northStructure)
                {
                    //northStructure.GetComponent<Structure>()
                }
            }
            
        }
    }
}
