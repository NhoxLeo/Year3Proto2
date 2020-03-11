using System.Collections;
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
    public Canvas canvas;
    public GameObject healthBarPrefab;
    private Structure structure;
    private Structure selectedStructure;
    private TileBehaviour structureOldTile;
    private BuildingInfo buildingInfo;
    private bool structureFromStore;
    public Transform tileHighlight = null;
    public Transform selectedTileHighlight = null;
    private StructManState structureState = StructManState.selecting;
    public Dictionary<string, StructureDefinition> structureDict;
    private GameManager gameMan;
    public bool isOverUI = false;
    private MessageBox messageBox;

    private void Awake()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            // NAME                                                     NAME                                               wC       mC      fC
            { "Longhaus",       new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(150,     20,     0)) },

            { "Archer Tower",   new StructureDefinition(Resources.Load("Archer Tower") as GameObject,   new ResourceBundle(90,      30,     0)) },
            { "Catapult Tower", new StructureDefinition(Resources.Load("Catapult Tower") as GameObject, new ResourceBundle(150,     100,    0)) },

            { "Farm",           new StructureDefinition(Resources.Load("Farm") as GameObject,           new ResourceBundle(40,      0,      0)) },
            { "Granary",        new StructureDefinition(Resources.Load("Granary") as GameObject,        new ResourceBundle(120,     0,      0)) },

            { "Lumber Mill",    new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(80,      20,     0)) },
            { "Lumber Pile",    new StructureDefinition(Resources.Load("Lumber Pile") as GameObject,    new ResourceBundle(120,     0,      0)) },

            { "Mine",           new StructureDefinition(Resources.Load("Mine") as GameObject,           new ResourceBundle(60,      60,     0)) },
            { "Metal Storage",  new StructureDefinition(Resources.Load("Metal Storage") as GameObject,  new ResourceBundle(130,     20,     0)) },
        };
        gameMan = FindObjectOfType<GameManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        canvas = FindObjectOfType<Canvas>();
        messageBox = FindObjectOfType<MessageBox>();
        healthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        structureFromStore = false;
        structureOldTile = null;
        structure = null;
        selectedStructure = null;
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!isOverUI)
        {
            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
            if (structureState == StructManState.moving)
            {
                if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
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

                                        if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                        if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
                                        // Special condition: Lumber Mills can be placed on Forest Environment
                                        if (attached.IsStructure("Forest Environment") && structure.IsStructure("Lumber Mill"))
                                        {
                                            canPlaceHere = true;
                                            //SendMessage("This will consume the Forest.", 0f);
                                        }
                                        // Special condition: Lumber Mills can be placed on Hill Environment
                                        else if (attached.IsStructure("Hill Environment") && structure.IsStructure("Mine"))
                                        {
                                            canPlaceHere = true;
                                            //SendMessage("This will consume the Hill.", 0f);
                                        }
                                        // Special condition: Lumber Mills can be placed on Forest Environment
                                        else if (attached.IsStructure("Plains Environment") && structure.IsStructure("Farm"))
                                        {
                                            canPlaceHere = true;
                                            //SendMessage("This will consume the Plains.", 0f);
                                        }
                                        else if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.attack)
                                        {
                                            canPlaceHere = true;
                                        }
                                        else
                                        {
                                            messageBox.ShowMessage("You cannot place that structure on that tile.");
                                        }
                                    }
                                    // if the tile we hit does not have an attached object...
                                    else { canPlaceHere = true; }
                                    if (canPlaceHere)
                                    {
                                        if (attached)
                                        {
                                            messageBox.ShowMessage("This will consume the environment tile.");
                                            if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.resource)
                                            {
                                                structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
                                            }
                                            else if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.attack)
                                            {
                                                structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
                                            }
                                        }
                                        else // the tile can be placed on, and has no attached structure
                                        {
                                            structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
                                            string messageBoxCurrent = messageBox.GetCurrentMessage();
                                            if (messageBoxCurrent == "This will consume the environment tile." || messageBoxCurrent == "You cannot place that structure on that tile.") { messageBox.HideMessage(); }
                                        }

                                        Vector3 structPos = hit.transform.transform.position;
                                        structPos.y = structure.sitHeight;
                                        structure.transform.position = structPos;

                                        Vector3 highlightPos = structPos;
                                        highlightPos.y = 0.501f;
                                        tileHighlight.position = highlightPos;
                                        selectedTileHighlight.position = highlightPos;

                                        if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                                        //if (!selectedTileHighlight.gameObject.activeSelf) selectedTileHighlight.gameObject.SetActive(true);

                                        // If the user clicked the LMB...
                                        if (Input.GetMouseButtonDown(0))
                                        {
                                            if ((structureFromStore && BuyBuilding()) || !structureFromStore)
                                            {
                                                GameManager.CreateAudioEffect("build", structure.transform.position);
                                                structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                                                // Attach the structure to the tile and vica versa
                                                if (attached) { attached.attachedTile.Detach(); }
                                                tile.Attach(structure);
                                                structure.OnPlace();
                                                if (attached)
                                                {
                                                    StructureType attachedStructType = attached.GetStructureType();
                                                    StructureType structType = structure.GetStructureType();
                                                    if (attachedStructType == StructureType.environment && structType == StructureType.resource)
                                                    {
                                                        Destroy(attached.gameObject);
                                                        messageBox.HideMessage();
                                                        switch (structure.GetStructureName())
                                                        {
                                                            case "Lumber Mill":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.wood));
                                                                structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                                                                break;
                                                            case "Farm":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.food));
                                                                structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                                                                break;
                                                            case "Mine":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.metal));
                                                                structure.GetComponent<Mine>().wasPlacedOnHills = true;
                                                                break;
                                                        }
                                                    }
                                                    else if (attachedStructType == StructureType.environment && structType == StructureType.attack)
                                                    {
                                                        switch (attached.GetStructureName())
                                                        {
                                                            case "Forest Environment":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.wood));
                                                                break;
                                                            case "Hill Environment":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.metal));
                                                                break;
                                                            case "Plains Environment":
                                                                gameMan.playerData.AddBatch(new Batch(50, ResourceType.food));
                                                                break;
                                                        }
                                                        Destroy(attached.gameObject);
                                                        messageBox.HideMessage();
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
                    messageBox.HideMessage();
                }
            }
            else if (structureState == StructManState.selected)
            {
                if (!selectedStructure)
                {
                    structureState = StructManState.selecting;
                }
                else
                {
                    Vector3 highlightpos = selectedStructure.transform.position;
                    highlightpos.y = 0.501f;
                    selectedTileHighlight.position = highlightpos;

                    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                    {
                        if (hit.transform.GetComponent<TileBehaviour>().GetPlayable())
                        {
                            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                            highlightpos = hit.transform.position;
                            highlightpos.y = 0.501f;
                            tileHighlight.position = highlightpos;
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                        }

                    }

                    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure")))
                    {
                        if (hit.transform.GetComponent<Structure>().attachedTile.GetPlayable())
                        {
                            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                            highlightpos = hit.transform.position;
                            highlightpos.y = 0.501f;
                            tileHighlight.position = highlightpos;
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                        }
                    }

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
                                    /* MOVING STRUCTURES DO NOT DELETE*/

                                    /*
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
                                    */
                                }
                                else
                                {
                                    if (hitStructure.attachedTile.GetPlayable())
                                    {
                                        SelectStructure(hitStructure);
                                    }
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
                }
            }
            else if (structureState == StructManState.selecting)
            {
                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
                {
                    if (hit.transform.GetComponent<TileBehaviour>().GetPlayable())
                    {
                        if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                        Vector3 highlightpos = hit.transform.position;
                        highlightpos.y = 0.501f;
                        tileHighlight.position = highlightpos;
                    }
                    else
                    {
                        if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                    }
                }

                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure")))
                {
                    Structure hitStructure = hit.transform.GetComponent<Structure>();
                    // If the hit transform has a structure component... (SHOULD ALWAYS)
                    if (hitStructure)
                    {
                        if (hitStructure.attachedTile.GetPlayable())
                        {
                            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                            Vector3 highlightpos = hit.transform.position;
                            highlightpos.y = 0.501f;
                            tileHighlight.position = highlightpos;
                            if (Input.GetMouseButtonDown(0))
                            {
                                SelectStructure(hitStructure);
                                structureState = StructManState.selected;
                            }
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                        }
                    }
                }
            }
        }
        if (isOverUI)
        {
            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
            //if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
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
            if (gameMan.playerData.AttemptPurchase(structureDict[structure.GetStructureName()].resourceCost))
            {
                return true;
            }
            messageBox.ShowMessage("You can't afford that!", 2f);
        }
        return false;
    }

    public bool SetBuilding(string _building)
    {
        if (structureState != StructManState.moving)
        {
            DeselectStructure();
            structureFromStore = true;
            GameObject structureInstance = Instantiate(structureDict[_building].structure, Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
            structure = structureInstance.GetComponent<Structure>();
            // Put the manager back into moving mode.
            structureState = StructManState.moving;
            if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
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
                    Vector3 structPos = structureOldTile.transform.position;
                    structPos.y = structure.sitHeight;
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

        Vector3 highlightpos = selectedStructure.attachedTile.transform.position;
        highlightpos.y = 0.501f;
        selectedTileHighlight.position = highlightpos;

        buildingInfo.SetTargetBuilding(selectedStructure.gameObject, selectedStructure.GetStructureName());
        buildingInfo.showPanel = true;
    }

    public void DeselectStructure()
    {
        if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
        if (selectedStructure)
        {
            selectedStructure.OnDeselected();
            selectedStructure = null;
        }
        buildingInfo.showPanel = false;
    }
}
