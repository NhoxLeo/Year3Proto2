using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    [SerializeField] [Tooltip("Determines whether or not the player can place structures on the tile.")]
    private bool isPlayable;
    [SerializeField] [Tooltip("Determines whether or not the enemy can spawn on the tile.")]
    private bool isValidSpawnTile;
    
    public enum TileCode
    { 
        north = 0,
        east,
        south,
        west
    }

    public Dictionary<TileCode, TileBehaviour> adjacentTiles;

    public Structure attachedStructure;

    // Start is called before the first frame update
    void Start()
    {
        attachedStructure = null;
        adjacentTiles = new Dictionary<TileCode, TileBehaviour>();

        // Get the child
        GameObject tileCollider = transform.Find("TileCollider").gameObject;

        // Turn off the collider
        BoxCollider tcBoxCollider = tileCollider.GetComponent<BoxCollider>();
        tcBoxCollider.enabled = false;

        // Cast 4 rays to get adjacent tiles, store them
        int tcLayer = 1 << LayerMask.NameToLayer("TileCollider");
        int structLayer = 1 << LayerMask.NameToLayer("Structure");

        // North
        if (Physics.Raycast(tileCollider.transform.position, Vector3.forward, out RaycastHit hit, .8f, tcLayer))
        {
            adjacentTiles.Add(TileCode.north, hit.collider.GetComponentInParent<TileBehaviour>());
        }
        // East
        if (Physics.Raycast(tileCollider.transform.position, Vector3.right, out hit, .8f, tcLayer))
        {
            adjacentTiles.Add(TileCode.east, hit.collider.GetComponentInParent<TileBehaviour>());
        }
        // South
        if (Physics.Raycast(tileCollider.transform.position, Vector3.back, out hit, .8f, tcLayer))
        {
            adjacentTiles.Add(TileCode.south, hit.collider.GetComponentInParent<TileBehaviour>());
        }
        // West
        if (Physics.Raycast(tileCollider.transform.position, Vector3.left, out hit, .8f, tcLayer))
        {
            adjacentTiles.Add(TileCode.west, hit.collider.GetComponentInParent<TileBehaviour>());
            //hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
        }

        if (Physics.Raycast(tileCollider.transform.position, Vector3.up, out hit, 1.6f, structLayer))
        {
            Attach(hit.transform.GetComponent<Structure>());
        }

        // Turn on the collider
        tcBoxCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Structure GetAttached()
    {
        return attachedStructure;
    }

    public void Attach(Structure _structure)
    {
        _structure.attachedTile = this;
        _structure.isPlaced = true;
        attachedStructure = _structure;
    }

    public void Detach()
    {
        attachedStructure.attachedTile = null;
        attachedStructure.isPlaced = false;
        attachedStructure = null;
    }

    public bool GetPlayable()
    {
        return isPlayable;
    }

    public bool GetSpawnTile()
    {
        return isValidSpawnTile;
    }
}
