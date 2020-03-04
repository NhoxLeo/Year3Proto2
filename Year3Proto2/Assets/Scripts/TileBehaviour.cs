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

    private GameObject attachedStructure;

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
        int tcLayer = LayerMask.NameToLayer("TileCollider");
        tcLayer = 1 << tcLayer;
        int structLayer = LayerMask.NameToLayer("Structure");
        structLayer = 1 << structLayer;
        Color colour = Color.green;
        colour.a = .1f;
        // North
        if (Physics.Raycast(tileCollider.transform.position, Vector3.forward, out RaycastHit hit, .8f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.north, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
                Debug.DrawRay(tileCollider.transform.position, Vector3.forward * .8f, Color.green, 2f);
            }
        }
        // East
        if (Physics.Raycast(tileCollider.transform.position, Vector3.right, out hit, .8f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.east, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
                Debug.DrawRay(tileCollider.transform.position, Vector3.right * .8f, Color.green, 2f);
            }
        }
        // South
        if (Physics.Raycast(tileCollider.transform.position, Vector3.back, out hit, .8f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.south, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
                Debug.DrawRay(tileCollider.transform.position, Vector3.back * .8f, Color.green, 2f);
            }
        }
        // West
        if (Physics.Raycast(tileCollider.transform.position, Vector3.left, out hit, .8f, tcLayer))
        {
            // if we hit a TileCollider
            if (hit.collider.name == "TileCollider")
            {
                adjacentTiles.Add(TileCode.west, hit.collider.GetComponentInParent<TileBehaviour>());
                hit.collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", colour);
                Debug.DrawRay(tileCollider.transform.position, Vector3.left * .8f, Color.green, 2f);
            }
        }

        if (Physics.Raycast(tileCollider.transform.position, Vector3.up, out hit, 1.6f, structLayer))
        {
            attachedStructure = hit.transform.gameObject;
        }

        // Turn on the collider
        tcBoxCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject GetAttached()
    {
        return attachedStructure;
    }

    public void Attach(GameObject _structure)
    {
        attachedStructure = _structure;
    }

    public void Detach()
    {
        attachedStructure = null;
    }
}
