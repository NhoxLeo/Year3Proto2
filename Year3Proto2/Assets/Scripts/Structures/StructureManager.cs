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
    private Structure currentStructure;
    private StructureState structureState = StructureState.SELECTING;

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (structureState == StructureState.MOVING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << 10))
            {
                if (currentStructure != null)
                {
                    Vector3 newPosition;

                    Vector3 gridpos = new Vector3(
                        Mathf.Floor(hit.point.x),
                        Mathf.Floor(hit.point.y),
                        Mathf.Floor(hit.point.z)
                    );

                    newPosition = new Vector3(
                        Mathf.Round(gridpos.x),
                        Mathf.Round(currentStructure.transform.position.y),
                        Mathf.Round(gridpos.z)
                    );

                    if (Input.GetMouseButtonDown(0))
                    {
                        currentStructure.transform.position = newPosition;
                        structureState = StructureState.SELECTING;
                    }
                }
            }
        }
        else if (structureState == StructureState.SELECTING)
        {
            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << 9))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.GetComponent<Structure>() != null)
                    {
                        currentStructure = hit.transform.gameObject.GetComponent<Structure>();
                        structureState = StructureState.MOVING;
                    }
                }
            }
        }
    }
}
