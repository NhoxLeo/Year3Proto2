using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructManState
{
    selecting,
    moving
};

public struct ResourceBundle
{
    public int woodCost;
    public int metalCost;
    public int foodCost;

    public ResourceBundle(int _wCost, int _mCost, int _fCost)
    {
        woodCost = _wCost;
        metalCost = _mCost;
        foodCost = _fCost;
    }
}

public struct StructureDefinition
{
    public GameObject structure;
    public ResourceBundle resourceCost;

    public StructureDefinition(GameObject _structure, ResourceBundle _cost)
    {
        structure = _structure;
        resourceCost = _cost;
    }

}

public class StructureManager : MonoBehaviour
{
    private Structure structure;
    private TileBehaviour structureOldTile;
    private bool structureFromStore;
    public Transform tileHighlight;
    private StructManState structureState = StructManState.selecting;
    public Dictionary<string, StructureDefinition> structureDict;
    private GameManager gameMan;
    public bool isOverUI = false;

    private void Start()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            //     NAME                                                 NAME                                         wC  mC  fC
            { "Lumber Mill", new StructureDefinition(Resources.Load("Lumber Mill") as GameObject, new ResourceBundle(100, 0, 0)) },
            { "Lumber Pile", new StructureDefinition(Resources.Load("Lumber Pile") as GameObject, new ResourceBundle(100, 0, 0)) }
        };
        gameMan = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!isOverUI)
        {
            if (!tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(true);
            if (structureState == StructManState.moving)
            {
                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    if (structure.transform != null)
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
                                hitPos.y = structure.sitHeight;
                                structure.transform.position = hitPos;

                                if (tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(false);

                                if (attached.GetComponent<Structure>().IsStructure("Forest Environment"))
                                {
                                    if (structure.IsStructure("Lumber Mill"))
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
                                Vector3 structPos = structure.transform.position;
                                structPos.x = hit.transform.position.x;
                                structPos.y = structure.sitHeight;
                                structPos.z = hit.transform.position.z;
                                structure.transform.position = structPos;

                                Vector3 highlightPos = structPos;
                                highlightPos.y = 0.501f;
                                tileHighlight.position = highlightPos;

                                if (!tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(true);

                                // If the user clicked the LMB...
                                if (Input.GetMouseButtonDown(0))
                                {
                                    if ((structureFromStore && BuyBuilding()) || !structureFromStore)
                                    {
                                        // Attach the structure to the tile and vica versa
                                        if (attached)
                                        {
                                            attached.GetComponent<Structure>().attachedTile.GetComponent<TileBehaviour>().Detach(true);
                                        }
                                        hit.transform.GetComponent<TileBehaviour>().Attach(structure.gameObject, true);
                                        if (structure.IsStructure("Lumber Mill"))
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
                                        else if (structure.GetStructureType() == StructureType.storage)
                                        {
                                            gameMan.CalculateStorageMaximum();
                                        }
                                        gameMan.OnStructurePlace();
                                        if (structureFromStore)
                                        {
                                            FindObjectOfType<BuildPanel>().UINoneSelected();
                                        }
                                        structureState = StructManState.selecting;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    ResetBuilding();
                }
            }
            else if (structureState == StructManState.selecting)
            {
                // If the player clicks the LMB...
                if (Input.GetMouseButtonDown(0))
                {
                    // If the player has clicked on a structure...
                    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure")))
                    {
                        // If the hit transform has a structure component... (SHOULD ALWAYS)
                        if (hit.transform.GetComponent<Structure>())
                        {
                            // If the structure is NOT an environment structure, and not the longhaus
                            if (hit.transform.GetComponent<Structure>().GetStructureType() != StructureType.environment
                                && hit.transform.GetComponent<Structure>().GetStructureName() != "Longhaus")
                            {
                                structureFromStore = false;
                                structure = hit.transform.GetComponent<Structure>();
                                structureOldTile = structure.attachedTile.GetComponent<TileBehaviour>();
                                // Detach the structure from it's tile, and vica versa.
                                structure.attachedTile.GetComponent<TileBehaviour>().Detach(true);
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
        if (isOverUI)
        {
            if (tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(false);
            HideBuilding();
        }
    }

    public void SetIsOverUI(bool _isOverUI)
    {
        isOverUI = _isOverUI;
    }

    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            return gameMan.playerData.AttemptPurchase(structureDict[structure.GetStructureName()].resourceCost);
        }
        return false;
    }

    public bool SetBuilding(string _building)
    {
        if (structureState != StructManState.moving)
        {
            structureFromStore = true;
            GameObject LPinstance = Instantiate(structureDict[_building].structure, Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
            structure = LPinstance.GetComponent<Structure>();
            // Put the manager back into moving mode.
            structureState = StructManState.moving;
            return true;
        }
        return false;
    }

    private void HideBuilding()
    {
        if (structure && structureState == StructManState.moving)
        {
            structure.transform.position = Vector3.down * 10f;
        }
    }

    public bool SetBuilding(BuildPanel.Buildings _buildingID)
    {
        return SetBuilding(IDToStructureName(_buildingID));
    }

    public void ResetBuilding()
    {
        if (structure)
        {
            if (structureState == StructManState.moving)
            {
                if (structureFromStore)
                {
                    Destroy(structure.gameObject);
                    FindObjectOfType<BuildPanel>().UINoneSelected();
                }
                else
                {
                    Vector3 structPos = structure.transform.position;
                    structPos.x = structureOldTile.transform.position.x;
                    structPos.y = structure.sitHeight;
                    structPos.z = structureOldTile.transform.position.z;
                    structure.transform.position = structPos;
                    structureOldTile.GetComponent<TileBehaviour>().Attach(structure.gameObject, true);
                    structureOldTile = null;
                }
                structureState = StructManState.selecting;
            }
            else
            {
                //Debug.LogError("ResetBuilding() when structureState is selecting");
            }
        }
        else
        {
            //Debug.LogError("ResetBuilding() when there is no structure");
        }
    }

    public static string IDToStructureName(BuildPanel.Buildings _buildingID)
    {
        switch (_buildingID)
        {
            case BuildPanel.Buildings.None:
                Debug.LogError("0 is not a building.");
                return "ERROR";
            case BuildPanel.Buildings.Archer:
                return "ERROR";
            case BuildPanel.Buildings.Catapult:
                return "ERROR";
            case BuildPanel.Buildings.Farm:
                return "ERROR";
            case BuildPanel.Buildings.Silo:
                return "ERROR";
            case BuildPanel.Buildings.LumberMill:
                return "Lumber Mill";
            case BuildPanel.Buildings.LumberPile:
                return "Lumber Pile";
            case BuildPanel.Buildings.Mine:
                return "ERROR";
            case BuildPanel.Buildings.MetalStorage:
                return "ERROR";
            default:
                Debug.LogError("default is not a building.");
                return "ERROR";
        }
    }
}
