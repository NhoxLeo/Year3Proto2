using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileCode
    { 
        north = 0,
        east,
        south,
        west
    }

    public Dictionary<TileCode, TileBehaviour> adjacentTiles;

    // Start is called before the first frame update
    void Start()
    {
        adjacentTiles = new Dictionary<TileCode, TileBehaviour>();

        // Get the child
        GameObject tileCollider = transform.Find("TileCollider").gameObject;

        // Turn off the collider
        BoxCollider tcBoxCollider = tileCollider.GetComponent<BoxCollider>();
        tcBoxCollider.enabled = false;

        // Cast 4 rays to get adjacent tiles, store them
        int tcLayer = LayerMask.NameToLayer("TileCollider");
        tcLayer = 1 << tcLayer;
        // North
        if (Physics.Raycast(tileCollider.transform.position, Vector3.forward, out RaycastHit hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.north, hit.collider.GetComponentInParent<TileBehaviour>());
                //adjacentTiles[TileCode.north].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            }
        }
        // East
        if (Physics.Raycast(tileCollider.transform.position, Vector3.right, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.east, hit.collider.GetComponentInParent<TileBehaviour>());
                //adjacentTiles[TileCode.east].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            }
        }
        // South
        if (Physics.Raycast(tileCollider.transform.position, Vector3.back, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.south, hit.collider.GetComponentInParent<TileBehaviour>());
                //adjacentTiles[TileCode.south].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            }
        }
        // West
        if (Physics.Raycast(tileCollider.transform.position, Vector3.left, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.west, hit.collider.GetComponentInParent<TileBehaviour>());
                //adjacentTiles[TileCode.west].transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
            }
        }

        // Turn on the collider
        tcBoxCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        GameObject tileCollider = transform.Find("TileCollider").gameObject;
        Debug.DrawRay(tileCollider.transform.position, Vector3.forward);
        Debug.DrawRay(tileCollider.transform.position, Vector3.back);
        Debug.DrawRay(tileCollider.transform.position, Vector3.right);
        Debug.DrawRay(tileCollider.transform.position, Vector3.left);
        */
    }
}
