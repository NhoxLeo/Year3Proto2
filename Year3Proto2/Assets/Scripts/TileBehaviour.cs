using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileBehaviour : MonoBehaviour
{
    [SerializeField] 
    [Tooltip("Determines whether or not the player can place structures on the tile.")]
    private bool isPlayable;

    [SerializeField] 
    [Tooltip("Determines whether or not the enemy can spawn on the tile.")]
    private bool isValidSpawnTile;

    private static GameObject CliffFacePrefab = null;
    private static GameObject SnowCliffFacePrefab = null;
    private static GameObject BoundaryPrefab = null;
    private static GameObject SnowBoundaryPrefab = null;
    private static Transform CliffFaceParent = null;
    private static Material SnowMat = null;
    private static GameObject BoundaryParent = null;

    //Determines whether or not it is being approached by an airship.")]
    private bool isApproached;

    private int ID;
    private static int nextID = 0;
    private static int GenerateID()
    {
        return nextID++;
    }

    public static int GetTileID(TileBehaviour _tile)
    {
        return _tile.ID;
    }

    public JSTileData GenerateJSTileData()
    {
        JSTileData tileData = new JSTileData
        {
            ID = ID,
            position = transform.position
        };
        tileData.adjacentTiles = new JSTileNeighborContainer();
        for (int i = 0; i < 4; i++)
        {
            if (GetAdjacentTiles().ContainsKey((TileCode)i))
            {
                tileData.adjacentTiles[i] = GetAdjacentTiles()[(TileCode)i].ID;
            }
            else
            {
                tileData.adjacentTiles[i] = -1;
            }
        }
        tileData.diagonalTiles = new JSTileNeighborContainer();
        for (int i = 0; i < 4; i++)
        {
            if (GetDiagonalTiles().ContainsKey(i))
            {
                tileData.diagonalTiles[i] = GetDiagonalTiles()[i].ID;
            }
            else
            {
                tileData.diagonalTiles[i] = -1;
            }
        }
        return tileData;
    }

    public enum TileCode
    { 
        north = 0,
        east,
        south,
        west
    }

    private Dictionary<TileCode, TileBehaviour> adjacentTiles;
    private Dictionary<int, TileBehaviour> diagonalTiles;

    private Structure attachedStructure = null;

    public Dictionary<TileCode, TileBehaviour> GetAdjacentTiles()
    {
        if (adjacentTiles == null)
        {
            FindAdjacentTiles();
        }
        return adjacentTiles;
    }

    public Dictionary<int, TileBehaviour> GetDiagonalTiles()
    {
        if (diagonalTiles == null)
        {
            FindDiagonalTiles();
        }
        return diagonalTiles;
    }

    private void FindDiagonalTiles()
    {
        // 0 - NE, 1 - SE, 2 - SW, 3 - NW
        diagonalTiles = new Dictionary<int, TileBehaviour>();

        Dictionary<TileCode, TileBehaviour> adjacents = GetAdjacentTiles();

        if (adjacents.ContainsKey(TileCode.north))
        {
            Dictionary<TileCode, TileBehaviour> northAdjacents = adjacents[TileCode.north].GetAdjacentTiles();

            if (northAdjacents.ContainsKey(TileCode.east))
            {
                diagonalTiles.Add(0, northAdjacents[TileCode.east]);
            }

            if (northAdjacents.ContainsKey(TileCode.west))
            {
                diagonalTiles.Add(3, northAdjacents[TileCode.west]);
            }
        }

        if (adjacents.ContainsKey(TileCode.south))
        {
            Dictionary<TileCode, TileBehaviour> southAdjacents = adjacents[TileCode.south].GetAdjacentTiles();

            if (southAdjacents.ContainsKey(TileCode.east))
            {
                diagonalTiles.Add(1, southAdjacents[TileCode.east]);
            }

            if (southAdjacents.ContainsKey(TileCode.west))
            {
                diagonalTiles.Add(2, southAdjacents[TileCode.west]);
            }
        }

        if (adjacents.ContainsKey(TileCode.east))
        {
            Dictionary<TileCode, TileBehaviour> eastAdjacents = adjacents[TileCode.east].GetAdjacentTiles();

            if (eastAdjacents.ContainsKey(TileCode.north))
            {
                if (!diagonalTiles.ContainsKey(0))
                {
                    diagonalTiles.Add(0, eastAdjacents[TileCode.north]);
                }
            }

            if (eastAdjacents.ContainsKey(TileCode.south))
            {
                if (!diagonalTiles.ContainsKey(1))
                {
                    diagonalTiles.Add(1, eastAdjacents[TileCode.south]);
                }
            }
        }

        if (adjacents.ContainsKey(TileCode.west))
        {
            Dictionary<TileCode, TileBehaviour> westAdjacents = adjacents[TileCode.west].GetAdjacentTiles();

            if (westAdjacents.ContainsKey(TileCode.north))
            {
                if (!diagonalTiles.ContainsKey(3))
                {
                    diagonalTiles.Add(3, westAdjacents[TileCode.north]);
                }
            }

            if (westAdjacents.ContainsKey(TileCode.south))
            {
                if (!diagonalTiles.ContainsKey(2))
                {
                    diagonalTiles.Add(2, westAdjacents[TileCode.south]);
                }
            }
        }
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
        GetCliffParent();
        SpawnCliffFaces();
        ID = GenerateID();
        if (!isPlayable)
        {
            Instantiate(GetBoundaryPrefab(), transform.position + Vector3.up * 0.501f, Quaternion.identity, GetBoundaryParent().transform);
        }
    }

    private void Start()
    {
        if (SuperManager.GetInstance().GetSnow())
        {
            GetComponent<MeshRenderer>().material = GetSnowMat();
        }
    }

    private void SpawnCliffFaces()
    {
        GetAdjacentTiles();
        for (int i = 0; i < 4; i++)
        {
            if (!adjacentTiles.ContainsKey((TileCode)i))
            {
                Instantiate(GetCliffFace(), transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.Euler(0f, 90f + 90f * i, 0f), CliffFaceParent);
            }
        }
    }

    private static GameObject GetCliffFace()
    {
        if (SuperManager.GetInstance().GetSnow())
        {
            if (!SnowCliffFacePrefab)
            {
                SnowCliffFacePrefab = Resources.Load("IslandEdgeCliff_Snow") as GameObject;
            }
            return SnowCliffFacePrefab;
        }
        else
        {
            if (!CliffFacePrefab)
            {
                CliffFacePrefab = Resources.Load("IslandEdgeCliff1") as GameObject;
            }
            return CliffFacePrefab;
        }
    }

    private static Transform GetCliffParent()
    {
        if (!CliffFaceParent)
        {
            CliffFaceParent = new GameObject("Cliff Faces").transform;
        }
        return CliffFaceParent;
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
            Attach(hit.transform.GetComponent<Structure>());
            return true;
        }
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

    public static Material GetSnowMat()
    {
        if (!SnowMat)
        {
            SnowMat = Resources.Load("Materials/Ground_Grass_Snow") as Material;
        }
        return SnowMat;
    }

    public static GameObject GetBoundaryPrefab()
    {
        if (SuperManager.GetInstance().GetSnow())
        {
            if (!SnowBoundaryPrefab)
            {
                SnowBoundaryPrefab = Resources.Load("Boundary_Snow") as GameObject;
            }
            return SnowBoundaryPrefab;
        }
        else
        {
            if (!BoundaryPrefab)
            {
                BoundaryPrefab = Resources.Load("Boundary") as GameObject;
            }
            return BoundaryPrefab;
        }
    }

    public static GameObject GetBoundaryParent()
    {
        if (!BoundaryParent)
        {
            BoundaryParent = new GameObject("Boundary Objects");
        }
        return BoundaryParent;
    }
}
