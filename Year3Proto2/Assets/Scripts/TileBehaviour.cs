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
        Color colour = Color.green;
        colour.a = .1f;
        // North
        if (Physics.Raycast(tileCollider.transform.position, Vector3.forward, out RaycastHit hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.north, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
            }
        }
        // East
        if (Physics.Raycast(tileCollider.transform.position, Vector3.right, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.east, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
            }
        }
        // South
        if (Physics.Raycast(tileCollider.transform.position, Vector3.back, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.south, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
            }
        }
        // West
        if (Physics.Raycast(tileCollider.transform.position, Vector3.left, out hit, 2f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.west, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
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
