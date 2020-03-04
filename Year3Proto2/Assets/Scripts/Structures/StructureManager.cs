using System.Collections;
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

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (structureState == StructureState.MOVING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << 10))
            {
                if (structure != null)
                {

                    Vector3 gridpos = new Vector3(
                        Mathf.Floor(hit.point.x),
                        Mathf.Floor(hit.point.y),
                        Mathf.Floor(hit.point.z)
                    );

                    structure.position = new Vector3(
                        Mathf.Round(gridpos.x),
                        Mathf.Round(structure.position.y),
                        Mathf.Round(gridpos.z)
                    );
                }

                if (Input.GetMouseButtonDown(0))
                {
                    structureState = StructureState.SELECTING;
                }
            }
        }
        else if (structureState == StructureState.SELECTING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << 9))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.GetComponent<ResourceStructure>() != null)
                    {
                        structure = hit.transform;
                        previousPosition = structure.position;
                        structureState = StructureState.MOVING;
                    }
                }
            }
        }
    }
}
