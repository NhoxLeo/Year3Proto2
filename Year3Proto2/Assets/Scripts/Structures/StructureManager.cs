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

    public static implicit operator Vector3(ResourceBundle _rb)
    {
        Vector3 vec;
        vec.x = _rb.woodCost;
        vec.y = _rb.metalCost;
        vec.z = _rb.foodCost;
        return vec;
    }

    public static ResourceBundle operator + (ResourceBundle _LHS, ResourceBundle _RHS)
    {
        ResourceBundle sum;
        sum.woodCost = _LHS.woodCost + _RHS.woodCost;
        sum.metalCost = _LHS.metalCost + _RHS.metalCost;
        sum.foodCost = _LHS.foodCost + _RHS.foodCost;
        return sum;
    }

    public ResourceBundle(Vector3 _vec)
    {
        woodCost = (int)_vec.x;
        metalCost = (int)_vec.y;
        foodCost = (int)_vec.z;
    }

    public bool IsEmpty()
    {
        return woodCost == 0 && metalCost == 0 && foodCost == 0;
    }
}

public struct StructureDefinition
{
    public ResourceBundle originalCost;
    public GameObject structurePrefab;
    public StructureDefinition(GameObject _structure, ResourceBundle _cost)
    {
        structurePrefab = _structure;
        originalCost = _cost;
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
    public static Dictionary<string, BuildPanel.Buildings> StructureIDs = new Dictionary<string, BuildPanel.Buildings>
    {
        { "Archer Tower", BuildPanel.Buildings.Archer },
        { "Catapult Tower", BuildPanel.Buildings.Catapult },
        { "Farm", BuildPanel.Buildings.Farm },
        { "Granary", BuildPanel.Buildings.Granary },
        { "Lumber Mill", BuildPanel.Buildings.LumberMill },
        { "Lumber Pile", BuildPanel.Buildings.LumberPile },
        { "Mine", BuildPanel.Buildings.Mine },
        { "Metal Storage", BuildPanel.Buildings.MetalStorage }
    };
    public Dictionary<BuildPanel.Buildings, int> StructureCounts = new Dictionary<BuildPanel.Buildings, int>
    {
        { BuildPanel.Buildings.Archer, 0 },
        { BuildPanel.Buildings.Catapult, 0 },
        { BuildPanel.Buildings.Farm, 0 },
        { BuildPanel.Buildings.Granary, 0 },
        { BuildPanel.Buildings.LumberMill, 0 },
        { BuildPanel.Buildings.LumberPile, 0 },
        { BuildPanel.Buildings.Mine, 0 },
        { BuildPanel.Buildings.MetalStorage, 0 }
    };

    [Header("Procedural Generation Parameters")]
    public Vector2 plainsEnvironmentBounds;
    public Vector2 hillsEnvironmentBounds;
    public Vector2 forestEnvironmentBounds;

    [Header("General Setup")]
    public Transform selectedTileHighlight = null;
    public Transform tileHighlight = null;

    public Dictionary<string, StructureDefinition> structureDict;
    public Dictionary<string, ResourceBundle> structureCosts;
    [HideInInspector]
    public Canvas canvas;
    [HideInInspector]
    public GameObject healthBarPrefab;
    [HideInInspector]
    public bool isOverUI = false;

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
    private BuildPanel panel;
    private GameObject buildingPuff;
    private EnemySpawner enemySpawner;
    private HUDManager HUDman;
    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            ResourceBundle cost = structureCosts[structure.GetStructureName()];
            if (gameMan.playerData.AttemptPurchase(cost))
            {
                IncreaseStructureCost(structure.GetStructureName());
                HUDman.ShowResourceDelta(cost, true);
                return true;
            }
            ShowMessage("You can't afford that!", 1.5f);
        }
        return false;
    }

    public void IncreaseStructureCost(string _structureName)
    {
        int buildingCount = ++StructureCounts[StructureIDs[_structureName]];
        Vector3 newCost = (2f + buildingCount) / 2f * (Vector3)structureDict[_structureName].originalCost;
        structureCosts[_structureName] = new ResourceBundle(newCost);
        panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = newCost;
    }

    public void DecreaseStructureCost(string _structureName)
    {
        int buildingCount = --StructureCounts[StructureIDs[_structureName]];
        if (buildingCount < 0) { buildingCount = StructureCounts[StructureIDs[_structureName]] = 0; }
        Vector3 newCost = (2f + buildingCount) / 2f * (Vector3)structureDict[_structureName].originalCost;
        structureCosts[_structureName] = new ResourceBundle(newCost);
        panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = newCost;
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
                    panel.UINoneSelected();
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
            GameObject structureInstance = Instantiate(structureDict[_building].structurePrefab, Vector3.down * 10f, Quaternion.Euler(0f, 0f, 0f));
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

    private void ProceduralGeneration(int _seed = 0)
    {
        if (_seed != 0) { Random.InitState(_seed); }
        
        // find our totals
        int forestTotal = Random.Range((int)forestEnvironmentBounds.x, (int)forestEnvironmentBounds.y + 1);
        int hillsTotal = Random.Range((int)hillsEnvironmentBounds.x, (int)hillsEnvironmentBounds.y + 1);
        int plainsTotal = Random.Range((int)plainsEnvironmentBounds.x, (int)plainsEnvironmentBounds.y + 1);

        // get all the tiles
        TileBehaviour[] tiles = FindObjectsOfType<TileBehaviour>();
        List<TileBehaviour> playableTiles = new List<TileBehaviour>();
        for (int i = 0; i < tiles.Length; i++)
        {
            // if the tile is playable and it doesn't have a structure already
            if (tiles[i].GetPlayable() && tiles[i].GetAttached() == null)
            {
                playableTiles.Add(tiles[i]);
            }
        }

        // Generate Forests
        int forestPlaced = 0;
        while (forestPlaced < forestTotal)
        {
            if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = playableTiles[Random.Range(0, playableTiles.Count)];

            if (!PGInstatiateEnvironment("Forest Environment", tile))
            {
                // Something has gone wrong
                //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                //tile.GetComponent<MeshRenderer>().enabled = false;
            }

            // update forestPlaced
            forestPlaced++;
            playableTiles.Remove(tile);
            if (forestPlaced == forestTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                TileBehaviour tileI = tile.adjacentTiles[(TileBehaviour.TileCode)i]; 
                if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                if (playableTiles.Contains(tileI))
                {
                    if (!PGInstatiateEnvironment("Forest Environment", tileI))
                    {
                        // Something has gone wrong
                        //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                        //tileI.GetComponent<MeshRenderer>().enabled = false;
                    }

                    // update forestPlaced
                    forestPlaced++;
                    playableTiles.Remove(tileI);
                    if (forestPlaced == forestTotal) { break; }
                }
            }
        }

        // Generate Hills
        int hillsPlaced = 0;
        while (hillsPlaced < hillsTotal)
        {
            if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = playableTiles[Random.Range(0, playableTiles.Count)];

            if (!PGInstatiateEnvironment("Hills Environment", tile))
            {
                // Something has gone wrong
                //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                //tile.GetComponent<MeshRenderer>().enabled = false;
            }

            // update hillsPlaced
            hillsPlaced++;
            playableTiles.Remove(tile);
            if (hillsPlaced == hillsTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                TileBehaviour tileI = tile.adjacentTiles[(TileBehaviour.TileCode)i]; 
                if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                if (playableTiles.Contains(tileI))
                {
                    if (!PGInstatiateEnvironment("Hills Environment", tileI))
                    {
                        // Something has gone wrong
                        //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                        //tileI.GetComponent<MeshRenderer>().enabled = false;
                    }

                    // update hillsPlaced
                    hillsPlaced++;
                    playableTiles.Remove(tileI);
                    if (hillsPlaced == hillsTotal) { break; }
                }
            }
        }

        // Generate Plains
        int plainsPlaced = 0;
        while (plainsPlaced < plainsTotal)
        {
            if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = playableTiles[Random.Range(0, playableTiles.Count)];

            if (!PGInstatiateEnvironment("Plains Environment", tile))
            {
                // Something has gone wrong
                //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                //tile.GetComponent<MeshRenderer>().enabled = false;
            }

            // update plainsPlaced
            plainsPlaced++;
            playableTiles.Remove(tile);
            if (plainsPlaced == plainsTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                TileBehaviour tileI = tile.adjacentTiles[(TileBehaviour.TileCode)i];
                if (playableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                if (playableTiles.Contains(tileI))
                {
                    if (!PGInstatiateEnvironment("Plains Environment", tileI))
                    {
                        // Something has gone wrong
                        //Debug.LogError("PG: PGInstatiateEnvironment returned false");
                        //tileI.GetComponent<MeshRenderer>().enabled = false;
                    }

                    // update plainsPlaced
                    plainsPlaced++;
                    playableTiles.Remove(tileI);
                    if (plainsPlaced == plainsTotal) { break; }
                }
            }
        }
    }

    private bool PGInstatiateEnvironment(string _environmentType, TileBehaviour _tile)
    {
        // create the structure
        Structure structure = Instantiate(structureDict[_environmentType].structurePrefab).GetComponent<Structure>();

        // move the new structure to the tile
        Vector3 structPos = _tile.transform.position;
        structPos.y = structure.sitHeight;
        structure.transform.position = structPos;
        //structure.transform.SetPositionAndRotation(structPos, structure.transform.rotation);

        _tile.Attach(structure);
        return true;
        //return _tile.DetectStructure();
    }

    private void Awake()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            // NAME                                                     NAME                                               wC       mC      fC
            { "Longhaus",       new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(600,     200,    0)) },

            { "Archer Tower",   new StructureDefinition(Resources.Load("Archer Tower") as GameObject,   new ResourceBundle(150,     50,     0)) },
            { "Catapult Tower", new StructureDefinition(Resources.Load("Catapult Tower") as GameObject, new ResourceBundle(200,     250,    0)) },

            { "Farm",           new StructureDefinition(Resources.Load("Farm") as GameObject,           new ResourceBundle(40,      0,      0)) },
            { "Granary",        new StructureDefinition(Resources.Load("Granary") as GameObject,        new ResourceBundle(120,     0,      0)) },

            { "Lumber Mill",    new StructureDefinition(Resources.Load("Lumber Mill") as GameObject,    new ResourceBundle(60,      20,     0)) },
            { "Lumber Pile",    new StructureDefinition(Resources.Load("Lumber Pile") as GameObject,    new ResourceBundle(120,     0,      0)) },

            { "Mine",           new StructureDefinition(Resources.Load("Mine") as GameObject,           new ResourceBundle(100,     20,     0)) },
            { "Metal Storage",  new StructureDefinition(Resources.Load("Metal Storage") as GameObject,  new ResourceBundle(120,     80,     0)) },

            { "Forest Environment", new StructureDefinition(Resources.Load("Forest Environment") as GameObject, new ResourceBundle(0, 0, 0)) },
            { "Hills Environment",  new StructureDefinition(Resources.Load("HillsEnvironment") as GameObject,   new ResourceBundle(0, 0, 0)) },
            { "Plains Environment", new StructureDefinition(Resources.Load("PlainsEnvironment") as GameObject,  new ResourceBundle(0, 0, 0)) },
        };
        structureCosts = new Dictionary<string, ResourceBundle>
        {
            // NAME                                 wC       mC      fC
            { "Longhaus",       new ResourceBundle(600,     200,    0) },

            { "Archer Tower",   new ResourceBundle(150,     50,     0) },
            { "Catapult Tower", new ResourceBundle(200,     250,    0) },

            { "Farm",           new ResourceBundle(40,      0,      0) },
            { "Granary",        new ResourceBundle(120,     0,      0) },

            { "Lumber Mill",    new ResourceBundle(60,      20,     0) },
            { "Lumber Pile",    new ResourceBundle(120,     0,      0) },

            { "Mine",           new ResourceBundle(100,     20,     0) },
            { "Metal Storage",  new ResourceBundle(120,     80,     0) }
        };
        panel = FindObjectOfType<BuildPanel>();
        gameMan = FindObjectOfType<GameManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        canvas = FindObjectOfType<Canvas>();
        messageBox = FindObjectOfType<MessageBox>();
        envInfo = FindObjectOfType<EnvInfo>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        HUDman = FindObjectOfType<HUDManager>();
        healthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        buildingPuff = Resources.Load("BuildEffect") as GameObject;
        structureFromStore = false;
        structureOldTile = null;
        structure = null;
        selectedStructure = null;
        hoveroverStructure = null;
        hoveroverTime = 0f;
    }

    private void Start()
    {
        /*
        TileBehaviour[] tiles = FindObjectsOfType<TileBehaviour>();
        for (int i = 0; i < tiles.Length; i++)
        {
            // if the tile is playable and it doesn't have a structure already
            if (tiles[i].GetPlayable() && tiles[i].GetAttached() == null)
            {
                //tiles[i].enabled = false;
                tiles[i].gameObject.SetActive(false);
            }
        }
        */
        ProceduralGeneration();
    }

    private void HideBuilding()
    {
        if (structure && structureState == StructManState.moving)
        {
            structure.transform.position = Vector3.down * 10f;
        }
    }

    private void PlayerMouseOver(Structure _structure)
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

    private void SetHoverInfo(Structure _structure)
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
            case "Catapult Tower":
                envInfo.ShowInfo("The Catapult fires explosive fireballs at enemy units.");
                break;
        }

    }

    private void ShowMessage(string _message, float _duration)
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
            switch (structureState)
            {
                case StructManState.selecting:
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
                    break;
                case StructManState.selected:
                    StructureType selectedStructType = selectedStructure.GetStructureType();
                    if (Input.GetKeyDown(KeyCode.Delete) &&
                        selectedStructType != StructureType.environment &&
                        selectedStructType != StructureType.longhaus)
                    {
                        selectedStructure.Damage(selectedStructure.GetHealth());
                        DeselectStructure();
                        structureState = StructManState.selecting;
                        break;
                    }

                    if (!selectedStructure)
                    {
                        DeselectStructure();
                        structureState = StructManState.selecting;
                        break;
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
                                    if (hitStructure != selectedStructure)
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
                                break;
                            }
                        }

                        StructureType structureType = selectedStructure.GetStructureType();
                        if (structureType == StructureType.resource || structureType == StructureType.attack)
                        {
                            if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                selectedStructure.IncreaseFoodAllocation();
                            }
                            if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                selectedStructure.DecreaseFoodAllocation();
                            }
                            if (Input.GetKeyDown(KeyCode.UpArrow))
                            {
                                selectedStructure.SetFoodAllocationMax();
                            }
                            if (Input.GetKeyDown(KeyCode.DownArrow))
                            {
                                selectedStructure.SetFoodAllocationMin();
                            }
                        }

                    }
                    break;
                case StructManState.moving:
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
                                            else if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.storage) { canPlaceHere = true; }
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
                                                else if (attached.GetStructureType() == StructureType.environment && structure.GetStructureType() == StructureType.storage)
                                                {
                                                    structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.yellow);
                                                }
                                            }
                                            else // the tile can be placed on, and has no attached structure
                                            {
                                                structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.green);
                                            }

                                            // If player cannot afford the structure, set to red.
                                            if (!gameMan.playerData.CanAfford(structureCosts[structure.GetStructureName()]))
                                            {
                                                structure.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.red);
                                            }

                                            Vector3 structPos = hit.transform.position;
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
                                                    
                                                    Instantiate(buildingPuff, structure.transform.position, Quaternion.Euler(-90f, 0f, 0f));
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
                                                        else if (attachedStructType == StructureType.environment && (structType == StructureType.attack || structType == StructureType.storage))
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
                                                        panel.UINoneSelected();
                                                        panel.ResetBuildingSelected();
                                                    }
                                                    if (!towerPlaced)
                                                    {
                                                        enemySpawner.Begin();
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
                        panel.ResetBuildingSelected();
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
                    break;
            }
        }
        if (isOverUI)
        {
            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
            HideBuilding();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gameMan.repairAll = true;
            gameMan.RepairAll();
        }
    }
}
