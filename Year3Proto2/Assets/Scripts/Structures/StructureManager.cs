﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructureState
{
    SELECTING,
    MOVING
};

public class StructureManager : MonoBehaviour
{
    private List<Structure> structures;
    private Transform structure;
    private Vector3 previousPosition;
    private StructureState structureState = StructureState.SELECTING;

    public Transform tileHighlight;

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        int groundLayer = LayerMask.NameToLayer("Ground");
        int structureLayer = LayerMask.NameToLayer("Structure");
        groundLayer = 1 << groundLayer;
        structureLayer = 1 << structureLayer;
        RaycastHit hit;

        if (structureState == StructureState.MOVING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, groundLayer))
            {
                if (structure != null)
                {
                    // If we hit a tile...
                    if (hit.collider.name.Contains("Tile"))
                    {
                        // If the tile we hit has an attached object...
                        if (hit.transform.GetComponent<TileBehaviour>().GetAttached())
                        {
                            Vector3 hitPos = hit.point;
                            hitPos.y = structure.position.y;
                            structure.position = hitPos;
                        }
                        else // if the tile we hit does not have an attached object...
                        {
                            Vector3 stuctPos = structure.position;
                            stuctPos.x = hit.transform.position.x;
                            stuctPos.z = hit.transform.position.z;
                            structure.position = stuctPos;
                            // If the user clicked the LMB...
                            if (Input.GetMouseButtonDown(0))
                            {
                                structureState = StructureState.SELECTING;
                                structure.GetComponent<Structure>().attachedTile = hit.transform.gameObject;
                                hit.transform.GetComponent<TileBehaviour>().Attach(structure.gameObject);

                                tileHighlight.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
        else if (structureState == StructureState.SELECTING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, structureLayer))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.GetComponent<Structure>() != null)
                    {
                        structure = hit.transform;
                        structure.GetComponent<Structure>().attachedTile.GetComponent<TileBehaviour>().Detach();
                        structure.GetComponent<Structure>().attachedTile = null;
                        previousPosition = structure.position;
                        structureState = StructureState.MOVING;

                        tileHighlight.gameObject.SetActive(false);
                    }
                }

                float yPos = tileHighlight.position.y;

                tileHighlight.position = new Vector3(hit.point.x, yPos, hit.point.y);
            }
        }
    }
}
