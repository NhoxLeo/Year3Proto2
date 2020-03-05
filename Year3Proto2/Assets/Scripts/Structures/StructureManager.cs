﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructManState
{
    selecting,
    moving
};

public class StructureManager : MonoBehaviour
{
    private Transform structure;
    private StructManState structureState = StructManState.selecting;

    public Transform tileHighlight;

    public Dictionary<string, GameObject> structureDict;

    private GameManager gameMan;

    private void Start()
    {
        structureDict = new Dictionary<string, GameObject>
        {
            { "Lumber Mill", Resources.Load("Lumber Mill") as GameObject },
            { "Lumber Pile", Resources.Load("Lumber Pile") as GameObject }
        };
        gameMan = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (structureState == StructManState.moving)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                if (structure != null)
                {
                    // If we hit a tile...
                    if (hit.collider.name.Contains("Tile"))
                    {
                        bool canPlaceHere = false;
                        // If the tile we hit has an attached object...
                        GameObject attached = hit.transform.GetComponent<TileBehaviour>().GetAttached();
                        if (attached)
                        {
                            Vector3 hitPos = hit.point;
                            hitPos.y = structure.GetComponent<Structure>().sitHeight;
                            structure.position = hitPos;
                            
                            if(tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(false);

                            if (attached.GetComponent<Structure>().IsStructure("Forest Environment"))
                            {
                                if (structure.GetComponent<Structure>().IsStructure("Lumber Mill"))
                                {
                                    // Special condition: Lumber Mills can be placed on Forest Environment
                                    canPlaceHere = true;
                                }
                            }
                        }
                        else // if the tile we hit does not have an attached object...
                        {
                            canPlaceHere = true;

                        }
                        if (canPlaceHere)
                        {
                            Vector3 structPos = structure.position;
                            structPos.x = hit.transform.position.x;
                            structPos.y = structure.GetComponent<Structure>().sitHeight;
                            structPos.z = hit.transform.position.z;
                            structure.position = structPos;

                            Vector3 highlightPos = structPos;
                            highlightPos.y = 0.501f;
                            tileHighlight.position = highlightPos;

                            if (!tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(true);

                            // If the user clicked the LMB...
                            if (Input.GetMouseButtonDown(0))
                            {
                                // Attach the structure to the tile and vica versa
                                if (attached)
                                {
                                    attached.GetComponent<Structure>().attachedTile.GetComponent<TileBehaviour>().Detach(true);
                                }
                                hit.transform.GetComponent<TileBehaviour>().Attach(structure.gameObject, true);
                                if (structure.GetComponent<Structure>().IsStructure("Lumber Mill"))
                                {
                                    if (attached)
                                    {
                                        if (attached.GetComponent<Structure>().IsStructure("Forest Environment"))
                                        {
                                            // The structure is being placed on a forest
                                            Destroy(attached);
                                            gameMan.playerData.AddBatch(new Batch(50, ResourceType.wood));
                                            structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                                            structure.GetComponent<MeshRenderer>().material = Resources.Load("TreeMaterial") as Material;
                                        }
                                    }
                                }
                                else if (structure.GetComponent<Structure>().GetStructureType() == StructureType.storage)
                                {
                                    gameMan.CalculateStorageMaximum();
                                }
                                structureState = StructManState.selecting;
                                gameMan.OnStructurePlace();
                            }
                        }
                    }
                }
            }
        }
        else if (structureState == StructManState.selecting)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                // replace with attempt cost
                if (gameMan.playerData.GetResource(ResourceType.wood) >= 40)
                {
                    gameMan.playerData.DeductResource(ResourceType.wood, 40);
                    GameObject LMinstance = Instantiate(structureDict["Lumber Mill"], Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
                    structure = LMinstance.transform;
                    // Put the manager back into moving mode.
                    structureState = StructManState.moving;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // replace with attempt cost
                if (gameMan.playerData.GetResource(ResourceType.wood) >= 20)
                {
                    gameMan.playerData.DeductResource(ResourceType.wood, 20);
                    GameObject LPinstance = Instantiate(structureDict["Lumber Pile"], Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
                    structure = LPinstance.transform;
                    // Put the manager back into moving mode.
                    structureState = StructManState.moving;
                }
            }


            // If the player clicks the LMB...
            if (Input.GetMouseButtonDown(0))
            {
                // If the player has clicked on a structure...
                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure")))
                {
                    // If the hit transform has a structure component... (SHOULD ALWAYS)
                    if (hit.transform.GetComponent<Structure>())
                    {
                        // If the structure is NOT an environment structure.
                        if (hit.transform.GetComponent<Structure>().GetStructureType() != StructureType.environment)
                        {
                            structure = hit.transform;
                            // Detach the structure from it's tile, and vica versa.
                            structure.GetComponent<Structure>().attachedTile.GetComponent<TileBehaviour>().Detach(true);
                            // Put the manager back into moving mode.
                            structureState = StructManState.moving;
                        }
                    }
                    else // The hit transform hasn't got a structure component
                    {
                        Debug.LogError(hit.transform.ToString() + " is on the structure layer, but it doesn't have a structure component.");
                    }
                }
            }

            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
            {
                Vector3 highlightpos = tileHighlight.position;
                highlightpos.x = hit.transform.position.x;
                highlightpos.y = 0.501f;
                highlightpos.z = hit.transform.position.z;

                tileHighlight.position = highlightpos;
            }
        }
    }
}
