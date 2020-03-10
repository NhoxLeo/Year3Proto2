﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StructManState
{
    selecting,
    selected,
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
    public static ResourceBundle operator *(ResourceBundle _kStructureDefinition, float _otherHS)
    {
        ResourceBundle newStructure = _kStructureDefinition;
        newStructure.woodCost = Mathf.RoundToInt(newStructure.woodCost * _otherHS);
        newStructure.metalCost = Mathf.RoundToInt(newStructure.metalCost * _otherHS);
        newStructure.foodCost = Mathf.RoundToInt(newStructure.foodCost * _otherHS);
        return newStructure;
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

    public static Dictionary<BuildPanel.Buildings, string> StructureNames = new Dictionary<BuildPanel.Buildings, string>
    {
        { BuildPanel.Buildings.Archer, "Archer Tower" },
        { BuildPanel.Buildings.Catapult, "Catapult Tower" },
        { BuildPanel.Buildings.Farm, "Farm" },
        { BuildPanel.Buildings.Granary, "Granary" },
        { BuildPanel.Buildings.LumberMill, "Lumber Mill" },
        { BuildPanel.Buildings.LumberPile, "Lumber Pile" },
        { BuildPanel.Buildings.Mine, "Mine" },
        { BuildPanel.Buildings.MetalStorage, "Metal Storage" }
    };

    private Structure structure;
    private Structure selectedStructure;
    private TileBehaviour structureOldTile;
    private BuildingInfo buildingInfo;
    private bool structureFromStore;
    public Transform tileHighlight;
    public Transform selectedTileHighlight;
    private StructManState structureState = StructManState.selecting;
    public Dictionary<string, StructureDefinition> structureDict;
    private GameManager gameMan;
    public bool isOverUI = false;

    private void Start()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            // NAME                                                     NAME                                               wC       mC      fC
            { "Longhaus",       new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(150,     20,     0)) },
            { "Lumber Mill",    new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(80,      20,     0)) },
            { "Lumber Pile",    new StructureDefinition(Resources.Load("Lumber Pile") as GameObject,    new ResourceBundle(120,     0,      0)) },
            { "Mine",           new StructureDefinition(Resources.Load("Mine") as GameObject,           new ResourceBundle(60,      60,     0)) },
            { "Metal Storage",  new StructureDefinition(Resources.Load("Metal Storage") as GameObject,  new ResourceBundle(130,     20,     0)) },
            { "Farm",           new StructureDefinition(Resources.Load("Farm") as GameObject,           new ResourceBundle(40,      0,      0)) },
            { "Granary",        new StructureDefinition(Resources.Load("Granary") as GameObject,        new ResourceBundle(120,     0,      0)) },
            { "Archer Tower",   new StructureDefinition(Resources.Load("Archer Tower") as GameObject,   new ResourceBundle(90,      30,     0)) },
            { "Catapult Tower", new StructureDefinition(Resources.Load("Catapult Tower") as GameObject, new ResourceBundle(150,     100,    0)) }
        };
        gameMan = FindObjectOfType<GameManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
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
                if (selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(false);
                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    if (structure.transform != null)
                    {
                        // If we hit a tile...
                        if (hit.collider.name.Contains("Tile"))
                        {
                            TileBehaviour tile = hit.transform.GetComponent<TileBehaviour>();
                            if (tile)
                            {
                                if (tile.GetPlayable())
                                {
                                    bool canPlaceHere = false;
                                    // If the tile we hit has an attached object...
                                    Structure attached = tile.GetAttached();
                                    if (attached)
                                    {
                                        Vector3 hitPos = hit.point;
                                        hitPos.y = structure.sitHeight;
                                        structure.transform.position = hitPos;

                                        structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);

                                        if (tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(false);
                                        if (selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(false);

                                        // Special condition: Lumber Mills can be placed on Forest Environment
                                        if (attached.IsStructure("Forest Environment") && structure.IsStructure("Lumber Mill")) { canPlaceHere = true; }
                                        // Special condition: Lumber Mills can be placed on Hill Environment
                                        else if (attached.IsStructure("Hill Environment") && structure.IsStructure("Mine")) { canPlaceHere = true; }
                                        // Special condition: Lumber Mills can be placed on Forest Environment
                                        else if (attached.IsStructure("Plains Environment") && structure.IsStructure("Farm")) { canPlaceHere = true; }
                                    }
                                    // if the tile we hit does not have an attached object...
                                    else { canPlaceHere = true; }
                                    if (canPlaceHere)
                                    {
                                        structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                                        Vector3 structPos = hit.transform.transform.position;
                                        structPos.y = structure.sitHeight;
                                        structure.transform.position = structPos;

                                        Vector3 highlightPos = structPos;
                                        highlightPos.y = 0.501f;
                                        tileHighlight.position = highlightPos;
                                        selectedTileHighlight.position = highlightPos;

                                        if (!tileHighlight.gameObject.activeSelf) tileHighlight.gameObject.SetActive(true);
                                        if (!selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(true);

                                        // If the user clicked the LMB...
                                        if (Input.GetMouseButtonDown(0))
                                        {
                                            if ((structureFromStore && BuyBuilding()) || !structureFromStore)
                                            {
                                                // Attach the structure to the tile and vica versa
                                                if (attached) { attached.attachedTile.Detach(); }
                                                tile.Attach(structure);
                                                structure.OnPlace();
                                                if (structure.IsStructure("Lumber Mill"))
                                                {
                                                    if (attached)
                                                    {
                                                        if (attached.IsStructure("Forest Environment"))
                                                        {
                                                            // The structure is being placed on a forest
                                                            Destroy(attached.gameObject);
                                                            gameMan.playerData.AddBatch(new Batch(50, ResourceType.wood));
                                                            structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                                                            //structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", new Color(0.7f, 1.0f, 0.7f, 1.0f));
                                                        }
                                                    }
                                                }
                                                else if (structure.IsStructure("Mine"))
                                                {
                                                    if (attached)
                                                    {
                                                        if (attached.IsStructure("Hill Environment"))
                                                        {
                                                            // The structure is being placed on a forest
                                                            Destroy(attached.gameObject);
                                                            gameMan.playerData.AddBatch(new Batch(50, ResourceType.metal));
                                                            structure.GetComponent<Mine>().wasPlacedOnHills = true;
                                                            //structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", new Color(0.7f, 1.0f, 0.7f, 1.0f));
                                                        }
                                                    }
                                                }
                                                else if (structure.IsStructure("Farm"))
                                                {
                                                    if (attached)
                                                    {
                                                        if (attached.IsStructure("Plains Environment"))
                                                        {
                                                            // The structure is being placed on a forest
                                                            Destroy(attached.gameObject);
                                                            gameMan.playerData.AddBatch(new Batch(50, ResourceType.food));
                                                            structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                                                            //structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", new Color(0.7f, 1.0f, 0.7f, 1.0f));
                                                        }
                                                    }
                                                }
                                                gameMan.OnStructurePlace();
                                                if (structureFromStore)
                                                {
                                                    FindObjectOfType<BuildPanel>().UINoneSelected();
                                                    FindObjectOfType<BuildPanel>().ResetBuildingSelected();
                                                }
                                                SelectStructure(structure);
                                                structureState = StructManState.selected;
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }
                if (Input.GetMouseButtonDown(1))
                {
                    ResetBuilding();
                    FindObjectOfType<BuildPanel>().ResetBuildingSelected();
                    if (structureFromStore)
                    {
                        DeselectStructure();
                        structureState = StructManState.selecting;
                    }
                    else
                    {
                        SelectStructure(structure);
                        structureState = StructManState.selected;
                    }
                }
            }
            else if (structureState == StructManState.selected)
            {
                // If the player clicks the LMB...
                if (Input.GetMouseButtonDown(0))
                {
                    // If the player has clicked on a structure...
                    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure")))
                    {
                        Structure hitStructure = hit.transform.GetComponent<Structure>();
                        // If the hit transform has a structure component... (SHOULD ALWAYS)
                        if (hitStructure)
                        {
                            if (hitStructure == selectedStructure)
                            {
                                // If the structure is NOT an environment structure, and not the longhaus
                                if (hitStructure.GetStructureType() != StructureType.environment && hitStructure.GetStructureName() != "Longhaus")
                                {
                                    structureFromStore = false;
                                    structure = hitStructure;
                                    structureOldTile = structure.attachedTile;
                                    // Detach the structure from it's tile, and vica versa.
                                    structure.attachedTile.Detach();
                                    structure.OnDeselected();
                                    // Put the manager back into moving mode.
                                    structureState = StructManState.moving;
                                    buildingInfo.showPanel = false;
                                }
                            }
                            else
                            {
                                SelectStructure(hitStructure);
                            }
                        }
                        else // The hit transform hasn't got a structure component
                        {
                            Debug.LogError(hit.transform.ToString() + " is on the structure layer, but it doesn't have a structure component.");
                        }
                    }
                    else
                    {
                        DeselectStructure();
                        structureState = StructManState.selecting;
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

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    // increases the food allocation of the selected structure.
                    if (selectedStructure.GetStructureType() == StructureType.resource)
                    {
                        selectedStructure.GetComponent<ResourceStructure>().IncreaseFoodAllocation();
                        FindObjectOfType<BuildingInfo>().SetInfo();
                    }
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    // decreases the food allocation of the selected structure.
                    if (selectedStructure.GetStructureType() == StructureType.resource)
                    {
                        selectedStructure.GetComponent<ResourceStructure>().DecreaseFoodAllocation();
                        FindObjectOfType<BuildingInfo>().SetInfo();
                    }
                }
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // increases the food allocation of the selected structure.
                    if (selectedStructure.GetStructureType() == StructureType.resource)
                    {
                        selectedStructure.GetComponent<ResourceStructure>().SetFoodAllocationMax();
                        FindObjectOfType<BuildingInfo>().SetInfo();
                    }
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    // decreases the food allocation of the selected structure.
                    if (selectedStructure.GetStructureType() == StructureType.resource)
                    {
                        selectedStructure.GetComponent<ResourceStructure>().SetFoodAllocationMin();
                        FindObjectOfType<BuildingInfo>().SetInfo();
                    }
                }
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    ResourceBundle repairCost = selectedStructure.RepairCost();
                    if (gameMan.playerData.CanAfford(repairCost))
                    {
                        gameMan.playerData.DeductResource(ResourceType.wood, repairCost.woodCost);
                        gameMan.playerData.DeductResource(ResourceType.metal, repairCost.metalCost);
                        gameMan.playerData.DeductResource(ResourceType.food, repairCost.foodCost);
                        selectedStructure.Repair();
                    }
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
                        Structure hitStructure = hit.transform.GetComponent<Structure>();
                        // If the hit transform has a structure component... (SHOULD ALWAYS)
                        if (hitStructure)
                        {
                            SelectStructure(hitStructure);
                            structureState = StructManState.selected;
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
            if (selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(false);
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
            DeselectStructure();
            structureFromStore = true;
            GameObject LPinstance = Instantiate(structureDict[_building].structure, Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
            structure = LPinstance.GetComponent<Structure>();
            // Put the manager back into moving mode.
            structureState = StructManState.moving;
            if (selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(false);
            selectedStructure = null;
            buildingInfo.showPanel = false;
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
        return SetBuilding(StructureNames[_buildingID]);
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
                    structureOldTile.Attach(structure);
                    structureOldTile = null;
                }
                structureState = StructManState.selected;
            }
        }
    }

    public void SelectStructure(Structure _structure)
    {
        DeselectStructure();
        selectedStructure = _structure;

        selectedStructure.OnSelected();

        if (!selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(true);

        Vector3 highlightpos = selectedTileHighlight.position;
        highlightpos.x = selectedStructure.attachedTile.transform.position.x;
        highlightpos.y = 0.501f;
        highlightpos.z = selectedStructure.attachedTile.transform.position.z;
        selectedTileHighlight.position = highlightpos;

        buildingInfo.SetTargetBuilding(selectedStructure.gameObject, selectedStructure.GetStructureName());
        buildingInfo.showPanel = true;
    }

    public void DeselectStructure()
    {
        if (selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(false);
        if (selectedStructure)
        {
            selectedStructure.OnDeselected();
            selectedStructure = null;
        }
        buildingInfo.showPanel = false;
    }
}
