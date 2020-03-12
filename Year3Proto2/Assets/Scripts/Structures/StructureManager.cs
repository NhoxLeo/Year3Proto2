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
    public int foodCost;
    public int metalCost;
    public int woodCost;
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
    public ResourceBundle resourceCost;
    public GameObject structure;
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
    public bool isOverUI = false;
    public Transform selectedTileHighlight = null;
    public Dictionary<string, StructureDefinition> structureDict;
    public Transform tileHighlight = null;
    private BuildingInfo buildingInfo;
    private EnvInfo envInfo;
    private GameManager gameMan;
    private Structure hoveroverStructure;
    private float hoveroverTime;
    private MessageBox messageBox;
    private Structure selectedStructure;
    private Structure structure;
    private bool structureFromStore;
    private TileBehaviour structureOldTile;
    private StructManState structureState = StructManState.selecting;
    private bool towerPlaced = false;
    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            if (gameMan.playerData.AttemptPurchase(structureDict[structure.GetStructureName()].resourceCost))
            {
                return true;
            }
            ShowMessage("You can't afford that!", 1.5f);
        }
        return false;
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

    public bool SetBuilding(BuildPanel.Buildings _buildingID)
    {
        return SetBuilding(StructureNames[_buildingID]);
    }

    public void SetIsOverUI(bool _isOverUI)
    {
        isOverUI = _isOverUI;
    }

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
        envInfo = FindObjectOfType<EnvInfo>();
        hoveroverStructure = null;
        hoveroverTime = 0f;
    }

    private void HideBuilding()
    {
        if (structure && structureState == StructManState.moving)
        {
            structure.transform.position = Vector3.down * 10f;
        }
    }

    void PlayerMouseOver(Structure _structure)
    {
        if (!hoveroverStructure)
        {
            hoveroverStructure = _structure;
            hoveroverTime += Time.deltaTime;
        }
        else if (hoveroverStructure == _structure)
        {
            hoveroverTime += Time.deltaTime;
        }
        else
        {
            hoveroverStructure = _structure;
            hoveroverTime = 0f;
        }
        if (hoveroverTime > 1.0f)
        {
            SetHoverInfo(_structure);
        }
    }

    void SetHoverInfo(Structure _structure)
    {
        envInfo.SetVisibility(true);
        switch (_structure.GetStructureName())
        {
            case "Longhaus":
                envInfo.ShowInfo("The Longhaus is your base of operations, protect it at all costs! The Longhaus generates a small amount of wood & food and an even smaller amount of metal.");
                break;
            case "Forest Environment":
                envInfo.ShowInfo("Placing a Lumber Mill (LM) on this tile will destroy the forest, and provide a bonus to the LM. Placing a LM adjacent to this tile with provide a bonus to the LM.");
                break;
            case "Hill Environment":
                envInfo.ShowInfo("Placing a Mine on this tile will destroy the hill, and provide a bonus to the Mine. Placing a Mine adjacent to this tile with provide a bonus to the Mine.");
                break;
            case "Plains Environment":
                envInfo.ShowInfo("Placing a Farm on this tile will destroy the plains, and provide a bonus to the Farm. Placing a Farm adjacent to this tile with provide a bonus to the Farm.");
                break;
            case "Farm":
                envInfo.ShowInfo("The Farm generates Food. It gains a bonus from all plains tiles surrounding it, and an additional bonus if placed on a plains tile.");
                break;
            case "Lumber Mill":
                envInfo.ShowInfo("The Lumber Mill generates Wood. It gains a bonus from all forest tiles surrounding it, and an additional bonus if placed on a forest tile.");
                break;
            case "Mine":
                envInfo.ShowInfo("The Mine generates Metal. It gains a bonus from all hill tiles surrounding it, and an additional bonus if placed on a hill tile.");
                break;
            case "Granary":
                envInfo.ShowInfo("The Granary stores Food. If it is broken, you will lose the additional capacity it gives you, and any excess Food you have will be lost.");
                break;
            case "Lumber Pile":
                envInfo.ShowInfo("The Lumber Pile stores Wood. If it is broken, you will lose the additional capacity it gives you, and any excess Wood you have will be lost.");
                break;
            case "Metal Storehouse":
                envInfo.ShowInfo("The Metal Storehouse stores Metal. If it is broken, you will lose the additional capacity it gives you, and any excess Metal you have will be lost.");
                break;
            case "Archer Tower":
                envInfo.ShowInfo("The Archer Tower fires arrows at enemy units.");
                break;
            case "Catapult":
                envInfo.ShowInfo("The Catapult fires explosive fireballs at enemy units.");
                break;
        }

    }

    void ShowMessage(string _message, float _duration)
    {
        if (gameMan.tutorialDone)
        {
            messageBox.ShowMessage(_message, _duration);
        }
    }

    private void Update()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        envInfo.SetVisibility(false);
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
                                        if (attached.IsStructure("Forest Environment") && structure.IsStructure("Lumber Mill")) { canPlaceHere = true; }
                                        else if (attached.IsStructure("Hill Environment") && structure.IsStructure("Mine")) { canPlaceHere = true; }
                                        else if (attached.IsStructure("Plains Environment") && structure.IsStructure("Farm")) { canPlaceHere = true; }
                                        else if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.attack) { canPlaceHere = true; }
                                    }
                                    // if the tile we hit does not have an attached object...
                                    else { canPlaceHere = true; }
                                    if (canPlaceHere)
                                    {
                                        if (attached)
                                        {
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
                                            //if (messageBoxCurrent == "This will consume the environment tile." || messageBoxCurrent == "You cannot place that structure on that tile.") { messageBox.HideMessage(); }
                                        }

                                        // If player cannot afford the structure, set to red.
                                        if (!gameMan.playerData.CanAfford(structureDict[structure.GetStructureName()].resourceCost))
                                        {
                                            structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
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
                                                if (!towerPlaced)
                                                {
                                                    FindObjectOfType<EnemySpawner>().Begin();
                                                    towerPlaced = true;
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
                    DeselectStructure();
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
                        Structure hitStructure = hit.transform.GetComponent<Structure>();
                        if (hitStructure.attachedTile.GetPlayable())
                        {
                            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                            highlightpos = hit.transform.position;
                            highlightpos.y = 0.501f;
                            tileHighlight.position = highlightpos;

                            PlayerMouseOver(hitStructure);
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                        }
                    }
                    else
                    {
                        hoveroverStructure = null;
                        hoveroverTime = 0f;
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

                            PlayerMouseOver(hitStructure);

                            if (Input.GetMouseButtonDown(0))
                            {
                                SelectStructure(hitStructure);
                                structureState = StructManState.selected;
                            }
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                            envInfo.SetVisibility(false);
                        }
                    }
                }
                else
                {
                    hoveroverStructure = null;
                    hoveroverTime = 0f;
                }
            }
        }
        if (isOverUI)
        {
            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
            //if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
            HideBuilding();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameMan.RepairAll();
        }
    }
}
