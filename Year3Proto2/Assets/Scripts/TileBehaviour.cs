using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class TileBehaviour : MonoBehaviour
{
    [SerializeField] 
    [Tooltip("Determines whether or not the player can place structures on the tile.")]
    private bool isPlayable;

    [SerializeField] 
    [Tooltip("Determines whether or not the enemy can spawn on the tile.")]
    private bool isValidSpawnTile;

    //Determines whether or not it is being approached by an airship.")]
    private bool isApproached;
    
    public enum TileCode
    { 
        north = 0,
        east,
        south,
        west
    }

    private Dictionary<TileCode, TileBehaviour> adjacentTiles;

    private Structure attachedStructure = null;

    public Dictionary<TileCode, TileBehaviour> GetAdjacentTiles()
    {
        if (adjacentTiles == null)
        {
            FindAdjacentTiles();
        }
        return adjacentTiles;
    }

    private void FindAdjacentTiles()
    {
        if (adjacentTiles != null)
        {
            return;
        }

        adjacentTiles = new Dictionary<TileCode, TileBehaviour>();

        // Get the child
        GameObject tileCollider = transform.Find("TileCollider").gameObject;

        // Turn off the collider
        BoxCollider tcBoxCollider = tileCollider.GetComponent<BoxCollider>();
        tcBoxCollider.enabled = false;

        // Cast 4 rays to get adjacent tiles, store them
        int tcLayer = 1 << LayerMask.NameToLayer("TileCollider");

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
        }

        // Turn on the collider
        tcBoxCollider.enabled = true;
    }

    void Awake()
    {
        DetectStructure();
    }

    public Structure GetAttached()
    {
        return attachedStructure;
    }

    public bool DetectStructure()
    {
        int structLayer = 1 << LayerMask.NameToLayer("Structure");
        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 1.6f, structLayer))
        {
            //Debug.DrawLine(transform.position, transform.position + Vector3.up * 1.6f, Color.green, 20.0f);
            Attach(hit.transform.GetComponent<Structure>());
            return true;
        }
        //Debug.DrawLine(transform.position, transform.position + Vector3.up * 1.6f, Color.red, 20.0f);
        return false;
    }

    public void SetApproached(bool _isApproached)
    {
        isApproached = _isApproached;
    }

    public void Attach(Structure _structure)
    {
        _structure.attachedTile = this;
        _structure.isPlaced = true;
        attachedStructure = _structure;
    }

    public void Detach()
    {
        if (attachedStructure)
        {
            attachedStructure.attachedTile = null;
            attachedStructure.isPlaced = false;
            attachedStructure = null;
        }
        else
        {
            Debug.LogWarning("Detach called on tile with no attachedStructure: " + name);
        }
    }

    public bool GetPlayable()
    {
        return isPlayable;
    }

    public bool GetSpawnTile()
    {
        return isValidSpawnTile;
    }

    public bool GetApproached()
    {
        return isApproached;
    }
}
