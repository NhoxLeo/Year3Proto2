//
// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2018 Media Design School.
//
// File Name        : StructureManager.cs
// Description      : Manager object that handles structures and structure related events.
// Author           : Samuel Fortune
// Mail             : Samuel.For7933@mediadesign.school.nz
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

public enum StructManState
{
    Selecting,
    Selected,
    Moving
};

public enum Priority
{
    BalancedProduction,
    Food,
    Wood,
    Metal,
    Defensive
}

[Serializable]
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

[Serializable]
public struct ProceduralGenerationParameters
{
    public SuperManager.SaveVector3 forestParameters;
    public SuperManager.SaveVector3 hillsParameters;
    public SuperManager.SaveVector3 plainsParameters;
    public bool useSeed;
    public int seed;
}

[ExecuteInEditMode]
public class StructureManager : MonoBehaviour
{
    // constants
    public static string PathSaveData;
    public static string PathPGP;

    // PRE-DEFINED
    private StructManState structureState = StructManState.Selecting;
    private bool towerPlaced = false;
    private bool structureFromStore = false;
    private Structure hoveroverStructure = null;
    private Structure selectedStructure = null;
    private Structure structure = null;
    private TileBehaviour structureOldTile = null;
    private float hoveroverTime = 0f;
    private int nextStructureID = 0;

    public static Dictionary<BuildPanel.Buildings, string> StructureNames = new Dictionary<BuildPanel.Buildings, string>
    {
        { BuildPanel.Buildings.Ballista, "Ballista Tower" },
        { BuildPanel.Buildings.Catapult, "Catapult Tower" },
        { BuildPanel.Buildings.Barracks, "Barracks" },
        { BuildPanel.Buildings.Farm, "Farm" },
        { BuildPanel.Buildings.Granary, "Granary" },
        { BuildPanel.Buildings.LumberMill, "Lumber Mill" },
        { BuildPanel.Buildings.LumberPile, "Lumber Pile" },
        { BuildPanel.Buildings.Mine, "Mine" },
        { BuildPanel.Buildings.MetalStorage, "Metal Storage" }
    };
    public static Dictionary<BuildPanel.Buildings, string> StructureDescriptions = new Dictionary<BuildPanel.Buildings, string>
    {
        { BuildPanel.Buildings.Ballista, "Fires deadly bolts at individual targets." },
        { BuildPanel.Buildings.Catapult, "Fires a large flaming boulder. Damages enemies in a small area." },
        { BuildPanel.Buildings.Barracks, "Spawns soldiers, who automatically attack enemies." },
        { BuildPanel.Buildings.Farm, "Collects Food from nearby plains tiles. Bonus if constructed on plains." },
        { BuildPanel.Buildings.Granary, "Increases maximum Food storage capacity." },
        { BuildPanel.Buildings.LumberMill, "Collects Wood from nearby forest tiles. Bonus if constructed on a forest." },
        { BuildPanel.Buildings.LumberPile, "Increases maximum Wood storage capacity." },
        { BuildPanel.Buildings.Mine, "Collects Metal from nearby rocky hill tiles. Bonus if constructed on hills." },
        { BuildPanel.Buildings.MetalStorage, "Increases maximum Metal storage capacity." }
    };
    public static Dictionary<string, BuildPanel.Buildings> StructureIDs = new Dictionary<string, BuildPanel.Buildings>
    {
        { StructureNames[BuildPanel.Buildings.Ballista], BuildPanel.Buildings.Ballista },
        { StructureNames[BuildPanel.Buildings.Catapult], BuildPanel.Buildings.Catapult },
        { StructureNames[BuildPanel.Buildings.Barracks], BuildPanel.Buildings.Barracks },
        { StructureNames[BuildPanel.Buildings.Farm], BuildPanel.Buildings.Farm },
        { StructureNames[BuildPanel.Buildings.Granary], BuildPanel.Buildings.Granary },
        { StructureNames[BuildPanel.Buildings.LumberMill], BuildPanel.Buildings.LumberMill },
        { StructureNames[BuildPanel.Buildings.LumberPile], BuildPanel.Buildings.LumberPile },
        { StructureNames[BuildPanel.Buildings.Mine], BuildPanel.Buildings.Mine },
        { StructureNames[BuildPanel.Buildings.MetalStorage], BuildPanel.Buildings.MetalStorage }
    };
    public Dictionary<BuildPanel.Buildings, int> structureCounts = new Dictionary<BuildPanel.Buildings, int>
    {
        { BuildPanel.Buildings.Ballista, 0 },
        { BuildPanel.Buildings.Catapult, 0 },
        { BuildPanel.Buildings.Barracks, 0 },
        { BuildPanel.Buildings.Farm, 0 },
        { BuildPanel.Buildings.Granary, 0 },
        { BuildPanel.Buildings.LumberMill, 0 },
        { BuildPanel.Buildings.LumberPile, 0 },
        { BuildPanel.Buildings.Mine, 0 },
        { BuildPanel.Buildings.MetalStorage, 0 }
    };
    private Dictionary<int, Structure> playerStructureDict = new Dictionary<int, Structure>();

    // Defined in window
    [HideInInspector]
    public Vector2Int plainsEnvironmentBounds;
    [HideInInspector]
    public Vector2Int hillsEnvironmentBounds;
    [HideInInspector]
    public Vector2Int forestEnvironmentBounds;
    [HideInInspector]
    public float recursivePGrowthChance;
    [HideInInspector]
    public float recursiveHGrowthChance;
    [HideInInspector]
    public float recursiveFGrowthChance;
    [HideInInspector]
    public bool editorGenerated = false;
    [HideInInspector]
    public bool useSeed = false;
    [HideInInspector]
    public int seed = 0;
    //public float recursiveGrowthFalloff;

    // Defined in runtime
    private List<TileBehaviour> PGPlayableTiles;

    public Transform selectedTileHighlight = null;
    public Transform tileHighlight = null;

    [HideInInspector]
    public bool isOverUI = false;

    // DEFINED IN AWAKE
    public Dictionary<string, StructureDefinition> structureDict;
    public Dictionary<string, ResourceBundle> structureCosts;

    [HideInInspector]
    public Canvas canvas;
    [HideInInspector]
    public GameObject healthBarPrefab;
    [HideInInspector]
    public GameObject villagerWidgetPrefab;

    private SuperManager superMan;
    private HUDManager HUDman;
    private GameManager gameMan;
    private BuildPanel panel;
    private GameObject buildingPuff;
    private EnemySpawner enemySpawner;
    private EnemyWaveSystem enemyWaveSystem;
    private BuildingInfo buildingInfo;
    private EnvInfo envInfo;
    private MessageBox messageBox;
    private List<Structure> allocationStructures = null;

    public bool IsThisStructureSelected(Structure _structure)
    {
        return selectedStructure == _structure;
    }
    
    private void DefineDictionaries()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            // NAME                                                     NAME                                                                       wC       mC      fC
            { "Longhaus",           new StructureDefinition(Resources.Load("Structures/Longhaus")               as GameObject,  new ResourceBundle(600,     200,    0)) },

            { "Ballista Tower",     new StructureDefinition(Resources.Load("Structures/Defense/Ballista Tower") as GameObject,  new ResourceBundle(150,     50,     0)) },
            { "Catapult Tower",     new StructureDefinition(Resources.Load("Structures/Defense/Catapult Tower") as GameObject,  new ResourceBundle(200,     250,    0)) },
            { "Barracks",           new StructureDefinition(Resources.Load("Structures/Defense/Barracks")       as GameObject,  new ResourceBundle(200,     250,    0)) },

            { "Farm",               new StructureDefinition(Resources.Load("Structures/Resource/Farm")          as GameObject,  new ResourceBundle(40,      0,      0)) },
            { "Lumber Mill",        new StructureDefinition(Resources.Load("Structures/Resource/Lumber Mill")   as GameObject,  new ResourceBundle(60,      20,     0)) },
            { "Mine",               new StructureDefinition(Resources.Load("Structures/Resource/Mine")          as GameObject,  new ResourceBundle(100,     20,     0)) },

            { "Granary",            new StructureDefinition(Resources.Load("Structures/Storage/Granary")        as GameObject,  new ResourceBundle(120,     0,      0)) },
            { "Lumber Pile",        new StructureDefinition(Resources.Load("Structures/Storage/Lumber Pile")    as GameObject,  new ResourceBundle(120,     0,      0)) },
            { "Metal Storage",      new StructureDefinition(Resources.Load("Structures/Storage/Metal Storage")  as GameObject,  new ResourceBundle(120,     80,     0)) },

            { "Forest",             new StructureDefinition(Resources.Load("Resources/Forest")                  as GameObject,  new ResourceBundle(0,       0,      0)) },
            { "Hills",              new StructureDefinition(Resources.Load("Resources/Hills")                   as GameObject,  new ResourceBundle(0,       0,      0)) },
            { "Plains",             new StructureDefinition(Resources.Load("Resources/Plains")                  as GameObject,  new ResourceBundle(0,       0,      0)) },
        };

        structureCosts = new Dictionary<string, ResourceBundle>
        {
            // NAME                                    wC       mC      fC
            { "Ballista Tower",     new ResourceBundle(150,     50,     0) },
            { "Catapult Tower",     new ResourceBundle(200,     250,    0) },
            { "Barracks",           new ResourceBundle(200,     100,    0) },

            { "Farm",               new ResourceBundle(40,      0,      0) },
            { "Lumber Mill",        new ResourceBundle(60,      20,     0) },
            { "Mine",               new ResourceBundle(100,     20,     0) },

            { "Granary",            new ResourceBundle(120,     0,      0) },
            { "Lumber Pile",        new ResourceBundle(120,     0,      0) },
            { "Metal Storage",      new ResourceBundle(120,     80,     0) }
        };
    }

    private void Awake()
    {
        PathSaveData = GetSaveDataPath();
        PathPGP = GetPGPPath();
        DefineDictionaries();
        panel = FindObjectOfType<BuildPanel>();
        gameMan = FindObjectOfType<GameManager>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        canvas = FindObjectOfType<Canvas>();
        messageBox = FindObjectOfType<MessageBox>();
        envInfo = FindObjectOfType<EnvInfo>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
        enemyWaveSystem = FindObjectOfType<EnemyWaveSystem>();
        HUDman = FindObjectOfType<HUDManager>();
        superMan = SuperManager.GetInstance();
        healthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        villagerWidgetPrefab = Resources.Load("VillagerAllocationWidget") as GameObject;
        buildingPuff = Resources.Load("BuildEffect") as GameObject;
        GlobalData.longhausDead = false;
    }

    private void OnApplicationQuit()
    {
        superMan.SaveCurrentMatch();
    }

    public static string GetSaveDataPath()
    {
        return PathSaveData ?? (PathSaveData = Application.persistentDataPath + "/saveData.dat");
    }

    public static string GetPGPPath()
    {
        return PathPGP ?? (PathPGP = Application.persistentDataPath + "/PGP.dat");
    }

    public static Structure FindStructureAtPosition(Vector3 _position)
    {
        Structure returnStructure = null;
        if (Physics.Raycast(_position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, LayerMask.GetMask("Structure")))
        {
            returnStructure = hit.collider.gameObject.GetComponent<Structure>();
        }
        return returnStructure;
    }

    public static TileBehaviour FindTileAtPosition(int _posX, int _posZ)
    {
        TileBehaviour result = null;
        Vector3 position = new Vector3
        {
            x = _posX,
            y = 0f,
            z = _posZ
        };
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, LayerMask.GetMask("Ground")))
        {
            result = hit.collider.gameObject.GetComponent<TileBehaviour>();
        }
        return result;
    }

    public Structure FindStructureWithID(int _ID)
    {
        if (playerStructureDict.ContainsKey(_ID))
        {
            return playerStructureDict[_ID];
        }
        else
        {
            return null;
        }
    }

    private void Start()
    {
#if UNITY_EDITOR
        // before we start, lets get rid of any NON PG clones
        ProceduralGenerationWindow.ClearClones(true);
#endif
        LoadPGPFromFile();
        if (Application.isPlaying)
        {
            if (!superMan.LoadCurrentMatch())
            {
                // if there is no current match
                editorGenerated = false;
                foreach (Structure structure in FindObjectsOfType<Structure>())
                {
                    if (structure.name.Contains("PG"))
                    {
                        editorGenerated = true;
                        break;
                    }
                }
                if (!editorGenerated)
                {
                    ProceduralGeneration(useSeed, seed);
                }
            }
        }
    }

    private void LoadPGPFromFile()
    {
        // try to access file
        BinaryFormatter bf = new BinaryFormatter();
        // if we found the file...
        if (System.IO.File.Exists(GetPGPPath()))
        {
            System.IO.FileStream file = System.IO.File.Open(PathPGP, System.IO.FileMode.Open);
            ProceduralGenerationParameters PGP = (ProceduralGenerationParameters)bf.Deserialize(file);
            hillsEnvironmentBounds = PGP.hillsParameters;
            recursiveHGrowthChance = PGP.hillsParameters.z;
            forestEnvironmentBounds = PGP.forestParameters;
            recursiveFGrowthChance = PGP.forestParameters.z;
            plainsEnvironmentBounds = PGP.plainsParameters;
            recursivePGrowthChance = PGP.plainsParameters.z;
            seed = PGP.seed;
            useSeed = PGP.useSeed;
            file.Close();
        }
        else
        {
            // if we can't access the file, we'll make one with default parameters
            System.IO.FileStream file = System.IO.File.Create(PathPGP);
            // File does not exist, load defaults and save
            ProceduralGenerationParameters PGP = GetPGPHardPreset(0);
            hillsEnvironmentBounds = PGP.hillsParameters;
            recursiveHGrowthChance = PGP.hillsParameters.z;
            forestEnvironmentBounds = PGP.forestParameters;
            recursiveFGrowthChance = PGP.forestParameters.z;
            plainsEnvironmentBounds = PGP.plainsParameters;
            recursivePGrowthChance = PGP.plainsParameters.z;
            seed = PGP.seed;
            useSeed = PGP.useSeed;
            bf.Serialize(file, PGP);
            file.Close();
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            envInfo.SetVisibility(false);
            if (!isOverUI)
            {
                if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
                switch (structureState)
                {
                    case StructManState.Selecting:
                        if (!Input.GetMouseButton(1))
                        {
                            if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Structure")))
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
                                            structureState = StructManState.Selected;
                                        }
                                    }
                                    else
                                    {
                                        if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                        envInfo.SetVisibility(false);
                                    }
                                }
                            }
                            else if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
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
                            else
                            {
                                if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                hoveroverStructure = null;
                                hoveroverTime = 0f;
                            }
                        }
                        else
                        {
                            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                            hoveroverStructure = null;
                            hoveroverTime = 0f;
                        }
                        break;
                    case StructManState.Selected:
                        if (!selectedStructure)
                        {
                            DeselectStructure();
                            structureState = StructManState.Selecting;
                            break;
                        }

                        StructureType selectedStructType = selectedStructure.GetStructureType();
                        if (Input.GetKeyDown(KeyCode.Delete) &&
                            selectedStructType != StructureType.Environment &&
                            selectedStructType != StructureType.Longhaus)
                        {
                            DestroySelectedBuilding();
                            break;
                        }
                        else
                        {
                            Vector3 highlightpos = selectedStructure.transform.position;
                            highlightpos.y = 0.501f;
                            selectedTileHighlight.position = highlightpos;

                            if (!Input.GetMouseButton(1))
                            {
                                if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Structure")))
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
                                else if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
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
                                else
                                {
                                    if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                    hoveroverStructure = null;
                                    hoveroverTime = 0f;
                                }

                                // If the player clicks the LMB...
                                if (Input.GetMouseButtonDown(0))
                                {
                                    // If the player has clicked on a structure...
                                    if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Structure")))
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
                                        structureState = StructManState.Selecting;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                hoveroverStructure = null;
                                hoveroverTime = 0f;
                            }
                        }
                        break;
                    case StructManState.Moving:
                        if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
                        if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
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
                                            StructureType newStructureType = structure.GetStructureType();
                                            if (attached)
                                            { 
                                                StructureType attachedStructureType = attached.GetStructureType();
                                                Vector3 hitPos = hit.point;
                                                hitPos.y = structure.sitHeight;
                                                structure.transform.position = hitPos;

                                                SetStructureColour(Color.red);

                                                if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                                                if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }

                                                if (attached.IsStructure("Forest Environment") && structure.IsStructure("Lumber Mill"))
                                                { 
                                                    if (!attached.GetComponent<EnvironmentStructure>().GetExploited())
                                                    {
                                                        canPlaceHere = true;
                                                    }
                                                }
                                                else if (attached.IsStructure("Hills Environment") && structure.IsStructure("Mine"))
                                                {
                                                    if (!attached.GetComponent<EnvironmentStructure>().GetExploited())
                                                    {
                                                        canPlaceHere = true;
                                                    }
                                                }
                                                else if (attached.IsStructure("Plains Environment") && structure.IsStructure("Farm"))
                                                {
                                                    if (!attached.GetComponent<EnvironmentStructure>().GetExploited())
                                                    {
                                                        canPlaceHere = true;
                                                    }
                                                }
                                                else if (attachedStructureType == StructureType.Environment)
                                                {
                                                    if (newStructureType == StructureType.Attack || newStructureType == StructureType.Defense || newStructureType == StructureType.Storage)
                                                    {
                                                        canPlaceHere = true;
                                                    }
                                                }
                                            }
                                            //else { canPlaceHere = hitFogMask; }
                                            else 
                                            { 
                                                canPlaceHere = true;
                                            }
                                            // if the structure can be placed here...
                                            if (canPlaceHere)
                                            {
                                                if (structure.GetStructureType() == StructureType.Attack)
                                                {
                                                    structure.GetComponent<AttackStructure>().ShowRangeDisplay(true);
                                                }

                                                if (attached)
                                                {
                                                    StructureType attachedStructureType = attached.GetStructureType();
                                                    if (attachedStructureType == StructureType.Environment)
                                                    {
                                                        if (newStructureType == StructureType.Resource)
                                                        {
                                                            SetStructureColour(Color.green);
                                                        }
                                                        else if (newStructureType == StructureType.Attack || newStructureType == StructureType.Defense || newStructureType == StructureType.Storage)
                                                        {
                                                            SetStructureColour(Color.yellow);
                                                        }
                                                    }
                                                }
                                                else // the tile can be placed on, and has no attached structure
                                                {
                                                    SetStructureColour(Color.green);
                                                }

                                                // If player cannot afford the structure, set to red.
                                                if (!gameMan.playerResources.CanAfford(structureCosts[structure.GetStructureName()]))
                                                {
                                                    SetStructureColour(Color.red);
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
                                                        SetStructureColour(Color.white);
                                                        // Attach the structure to the tile and vica versa
                                                        if (attached) { attached.attachedTile.Detach(); }
                                                        tile.Attach(structure);
                                                        structure.SetID(GetNewID());
                                                        structure.OnPlace();
                                                        Instantiate(buildingPuff, structure.transform.position, Quaternion.Euler(-90f, 0f, 0f));
                                                        if (attached)
                                                        {
                                                            StructureType attachedStructType = attached.GetStructureType();
                                                            StructureType structType = structure.GetStructureType();
                                                            if (attachedStructType == StructureType.Environment && structType == StructureType.Resource)
                                                            {
                                                                messageBox.HideMessage();
                                                                switch (structure.GetStructureName())
                                                                {
                                                                    case "Lumber Mill":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.Wood));
                                                                        structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                                                                        break;
                                                                    case "Farm":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.Food));
                                                                        structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                                                                        break;
                                                                    case "Mine":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.Metal));
                                                                        structure.GetComponent<Mine>().wasPlacedOnHills = true;
                                                                        break;
                                                                }
                                                            }
                                                            Destroy(attached.gameObject);
                                                        }
                                                        gameMan.OnStructurePlace();
                                                        enemySpawner.OnStructurePlaced();
                                                        if (structureFromStore)
                                                        {
                                                            panel.UINoneSelected();
                                                            panel.ResetBuildingSelected();
                                                        }
                                                        if (!towerPlaced)
                                                        {
                                                            if (!enemyWaveSystem.GetSpawning())
                                                            {
                                                                enemyWaveSystem.SetSpawning(true);
                                                            }
                                                            /*
                                                            if (!enemySpawner.IsSpawning())
                                                            {
                                                                enemySpawner.ToggleSpawning();
                                                            }
                                                            */
                                                            towerPlaced = true;
                                                        }
                                                        SelectStructure(structure);
                                                        if (structure.GetStructureType() == StructureType.Resource)
                                                        {
                                                            structure.AllocateVillager();
                                                        }
                                                        structureState = StructManState.Selected;
                                                        playerStructureDict.Add(structure.GetID(), structure);
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
                                structureState = StructManState.Selecting;
                            }
                            else
                            {
                                SelectStructure(structure);
                                structureState = StructManState.Selected;
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

    public void DestroySelectedBuilding()
    {
        selectedStructure.DeallocateAll();
        float health = selectedStructure.GetHealth();
        float maxHealth = selectedStructure.GetMaxHealth();
        selectedStructure.Damage(health);
        ResourceBundle compensation = new ResourceBundle(0.5f * (health / maxHealth) * (Vector3)structureCosts[selectedStructure.GetStructureName()]);
        gameMan.playerResources.AddResourceBundle(compensation);
        FindObjectOfType<HUDManager>().ShowResourceDelta(compensation, false);
        DeselectStructure();
        structureState = StructManState.Selecting;
    }

    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            ResourceBundle cost = structureCosts[structure.GetStructureName()];
            if (gameMan.playerResources.AttemptPurchase(cost))
            {
                IncreaseStructureCost(structure.GetStructureName());
                HUDman.ShowResourceDelta(cost, true);
                return true;
            }
            ShowMessage("You can't afford that!", 1.5f);
        }
        return false;
    }

    private void IncreaseStructureCost(string _structureName)
    {
        if (structureCosts.ContainsKey(_structureName))
        {
            structureCounts[StructureIDs[_structureName]]++;
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = CalculateStructureCost(_structureName);
        }
    }

    public void DecreaseStructureCost(string _structureName)
    {
        if (structureCosts.ContainsKey(_structureName))
        {
            structureCounts[StructureIDs[_structureName]]--;
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = CalculateStructureCost(_structureName);
        }
    }

    private Vector3 CalculateStructureCost(string _structureName)
    {
        //float increaseCoefficient = superMan.CurrentLevelHasModifier(SuperManager.SnoballPrices) ? 2f : 4f;
        if (superMan.CurrentLevelHasModifier(SuperManager.SnoballPrices))
        {
            Vector3 newCost = (4f + structureCounts[StructureIDs[_structureName]]) / 4f * (Vector3)structureDict[_structureName].originalCost;
            structureCosts[_structureName] = new ResourceBundle(newCost);
        }
        return structureCosts[_structureName];
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
            if (structureState == StructManState.Moving)
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
                structureState = StructManState.Selected;
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
        if (structureState != StructManState.Moving)
        {
            DeselectStructure();
            structureFromStore = true;
            GameObject structureInstance = Instantiate(structureDict[_building].structurePrefab);
            structureInstance.transform.position = Vector3.down * 10f;
            structure = structureInstance.GetComponent<Structure>();
            // Put the manager back into moving mode.
            structureState = StructManState.Moving;
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

    public void SetNextStructureID(int _ID)
    {
        nextStructureID = _ID;
    }

    public int GetNextStructureID()
    {
        return nextStructureID;
    }

    public int GetNewID()
    {
        return nextStructureID++;
    }

    public void ProceduralGeneration(bool _useSeed = false, int _seed = 0)
    {
        if (_useSeed) { UnityEngine.Random.InitState(_seed); }
        
        // find our totals
        int forestTotal = UnityEngine.Random.Range(forestEnvironmentBounds.x, forestEnvironmentBounds.y + 1);
        int hillsTotal = UnityEngine.Random.Range(hillsEnvironmentBounds.x, hillsEnvironmentBounds.y + 1);
        int plainsTotal = UnityEngine.Random.Range(plainsEnvironmentBounds.x, plainsEnvironmentBounds.y + 1);

        // get all the tiles
        TileBehaviour[] tiles = FindObjectsOfType<TileBehaviour>();
        PGPlayableTiles = new List<TileBehaviour>();
        for (int i = 0; i < tiles.Length; i++)
        {
            if (Application.isEditor) { tiles[i].DetectStructure(); }
            // if the tile is playable and it doesn't have a structure already
            if (tiles[i].GetPlayable() && tiles[i].GetAttached() == null)
            {
                PGPlayableTiles.Add(tiles[i]);
            }
        }

        int hillsPlaced = 0;
        while (hillsPlaced < hillsTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander("Hills Environment", tile, ref hillsPlaced, hillsTotal, recursiveHGrowthChance);
        }

        int forestPlaced = 0;
        while (forestPlaced < forestTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander("Forest Environment", tile, ref forestPlaced, forestTotal, recursiveFGrowthChance);
        }

        int plainsPlaced = 0;
        while (plainsPlaced < plainsTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander("Plains Environment", tile, ref plainsPlaced, plainsTotal, recursivePGrowthChance);
        }
    }

    public void ProceduralGenerationA(int _seed)
    {

        if (_seed != 0) { UnityEngine.Random.InitState(_seed); }

        // find our totals
        int forestTotal = UnityEngine.Random.Range(forestEnvironmentBounds.x, forestEnvironmentBounds.y + 1);
        int hillsTotal = UnityEngine.Random.Range(hillsEnvironmentBounds.x, hillsEnvironmentBounds.y + 1);
        int plainsTotal = UnityEngine.Random.Range(plainsEnvironmentBounds.x, plainsEnvironmentBounds.y + 1);

        // get all the tiles
        TileBehaviour[] tiles = FindObjectsOfType<TileBehaviour>();
        PGPlayableTiles = new List<TileBehaviour>();
        for (int i = 0; i < tiles.Length; i++)
        {
            // if the tile is playable and it doesn't have a structure already
            if (tiles[i].GetPlayable() && tiles[i].GetAttached() == null)
            {
                PGPlayableTiles.Add(tiles[i]);
            }
        }

        // Generate Forests
        int forestPlaced = 0;
        while (forestPlaced < forestTotal)
        {
            if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];

            PGInstatiateEnvironment("Forest Environment", tile);

            // update forestPlaced
            forestPlaced++;
            PGPlayableTiles.Remove(tile);
            if (forestPlaced == forestTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToTile = tile.GetAdjacentTiles();
                if (adjacentsToTile.ContainsKey((TileBehaviour.TileCode)i))
                {
                    TileBehaviour tileI = adjacentsToTile[(TileBehaviour.TileCode)i];
                    if (PGPlayableTiles.Contains(tileI))
                    {
                        PGInstatiateEnvironment("Forest Environment", tileI);

                        // update forestPlaced
                        forestPlaced++;
                        PGPlayableTiles.Remove(tileI);
                        if (forestPlaced == forestTotal) { break; }
                    }
                }
            }
        }

        // Generate Hills
        int hillsPlaced = 0;
        while (hillsPlaced < hillsTotal)
        {
            if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];

            PGInstatiateEnvironment("Hills Environment", tile);

            // update hillsPlaced
            hillsPlaced++;
            PGPlayableTiles.Remove(tile);
            if (hillsPlaced == hillsTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToTile = tile.GetAdjacentTiles();
                if (adjacentsToTile.ContainsKey((TileBehaviour.TileCode)i))
                {
                    TileBehaviour tileI = adjacentsToTile[(TileBehaviour.TileCode)i];
                    if (PGPlayableTiles.Contains(tileI))
                    {
                        PGInstatiateEnvironment("Hills Environment", tileI);

                        // update hillsPlaced
                        hillsPlaced++;
                        PGPlayableTiles.Remove(tileI);
                        if (hillsPlaced == hillsTotal) { break; }
                    }
                }
            }
        }

        // Generate Plains
        int plainsPlaced = 0;
        while (plainsPlaced < plainsTotal)
        {
            if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];

            PGInstatiateEnvironment("Plains Environment", tile);

            // update plainsPlaced
            plainsPlaced++;
            PGPlayableTiles.Remove(tile);
            if (plainsPlaced == plainsTotal) { break; }

            // now try the tiles around it
            for (int i = 0; i < 4; i++)
            {
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
                Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToTile = tile.GetAdjacentTiles();
                if (adjacentsToTile.ContainsKey((TileBehaviour.TileCode)i))
                {
                    TileBehaviour tileI = adjacentsToTile[(TileBehaviour.TileCode)i];
                    if (PGPlayableTiles.Contains(tileI))
                    {
                        PGInstatiateEnvironment("Plains Environment", tileI);

                        // update plainsPlaced
                        plainsPlaced++;
                        PGPlayableTiles.Remove(tileI);
                        if (plainsPlaced == plainsTotal) { break; }
                    }
                }
            }
        }
    }

    private void SetStructureColour(Color _colour)
    {
        foreach (Material mat in structure.GetComponent<MeshRenderer>().materials)
        {
            mat.SetColor("_BaseColor", _colour);
        }
    }

    private void PGRecursiveWander(string _environmentType, TileBehaviour _tile, ref int _placed, int _max, float _recursiveChance)
    {
        if (_placed == _max)
        {
            return;
        }
        // plant the environment on the tile,
        // remove the tile from PGPlayableTiles
        // for each face of the tile, if that face is in PGPlayableTiles, roll the dice on PGRecursiveWander
        if (PGPlayableTiles.Contains(_tile))
        {
            _placed++;
            PGPlayableTiles.Remove(_tile);
            PGInstatiateEnvironment(_environmentType, _tile);
        }

        // now try the tiles around it
        for (int i = 0; i < 4; i++)
        {
            if (_placed == _max) { break; }

            Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToTile = _tile.GetAdjacentTiles();
            if (adjacentsToTile.ContainsKey((TileBehaviour.TileCode)i))
            {
                TileBehaviour tileI = adjacentsToTile[(TileBehaviour.TileCode)i];
                if (PGPlayableTiles.Contains(tileI))
                {
                    if (UnityEngine.Random.Range(0f, 100f) <= _recursiveChance * 100f)
                    {
                        PGRecursiveWander(_environmentType, tileI, ref _placed, _max, _recursiveChance);
                    }
                }
            }
        }
    }

    private void PGInstatiateEnvironment(string _environmentType, TileBehaviour _tile)
    {
        // define the structures when in editor mode
        if (!Application.isPlaying)
        {
            if (structureDict == null) { DefineDictionaries(); }
        }
        // create the structure
        Structure structure = Instantiate(structureDict[_environmentType].structurePrefab).GetComponent<Structure>();
        string structName = structure.name;
        structure.name = "PG " + structName;
        // move the new structure to the tile
        Vector3 structPos = _tile.transform.position;
        structPos.y = structure.sitHeight;
        structure.transform.position = structPos;

        _tile.Attach(structure);
    }

    private void HideBuilding()
    {
        if (structure && structureState == StructManState.Moving)
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
            case "Hills Environment":
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
            case "Metal Storage":
                envInfo.ShowInfo("The Metal Storehouse stores Metal. If it is broken, you will lose the additional capacity it gives you, and any excess Metal you have will be lost.");
                break;
            case "Ballista Tower":
                envInfo.ShowInfo("The Ballista Tower fires bolts at enemy units.");
                break;
            case "Catapult Tower":
                envInfo.ShowInfo("The Catapult fires explosive fireballs at enemy units.");
                break;
            case "Barracks":
                envInfo.ShowInfo("The Barracks spawns soldiers which attack enemy units automatically.");
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

    public void LoadBuilding(SuperManager.StructureSaveData _saveData)
    {
        Structure newStructure = Instantiate(structureDict[_saveData.structure].structurePrefab).GetComponent<Structure>();
        newStructure.transform.position = _saveData.position;
        if (FindTileAtXZ(_saveData.position.x, _saveData.position.z, out TileBehaviour tile))
        {
            tile.Attach(newStructure);
        }
        else
        {
            Debug.LogError("No tile at _saveData.position, failed to load.");
            return;
        }
        newStructure.SetAllocated(_saveData.villagers);
        ResourceStructure resourceStructComp = newStructure.gameObject.GetComponent<ResourceStructure>();
        if (resourceStructComp)
        {
            if (_saveData.structure == "Farm")
            {
                newStructure.gameObject.GetComponent<Farm>().wasPlacedOnPlains = _saveData.wasPlacedOn; 
            }
            if (_saveData.structure == "Mine") 
            { 
                newStructure.gameObject.GetComponent<Mine>().wasPlacedOnHills = _saveData.wasPlacedOn; 
            }
            if (_saveData.structure == "Lumber Mill") 
            { 
                newStructure.gameObject.GetComponent<LumberMill>().wasPlacedOnForest = _saveData.wasPlacedOn; 
            }
        }
        Barracks barracksComponent = newStructure.gameObject.GetComponent<Barracks>();
        if (barracksComponent) 
        { 
            //barracksComponent.SetTimeTrained(_saveData.timeTrained);
        }
        if (_saveData.exploited)
        { 
            EnvironmentStructure environmentComponent = newStructure.gameObject.GetComponent<EnvironmentStructure>();
            if (environmentComponent)
            {
                environmentComponent.SetExploited(true);
                environmentComponent.SetExploiterID(_saveData.exploiterID);
            }
        }
        newStructure.isPlaced = true;
        newStructure.SetHealth(_saveData.health);
        newStructure.fromSaveData = true;
        newStructure.SetID(_saveData.ID);
        playerStructureDict.Add(_saveData.ID, newStructure);
    }

    private bool FindTileAtXZ(float _x, float _z, out TileBehaviour _tile)
    {
        _tile = null;
        Vector3 startPos = new Vector3(_x, 20, _z);
        bool hit = Physics.Raycast(startPos, Vector3.down, out RaycastHit hitInfo, 40f, LayerMask.GetMask("TileCollider"));
        if (hit) { _tile = hitInfo.collider.transform.parent.GetComponent<TileBehaviour>(); }
        return hit;
    }

    public static ProceduralGenerationParameters GetPGPHardPreset(int _preset)
    {
        ProceduralGenerationParameters pgp = new ProceduralGenerationParameters();
        switch (_preset)
        {
            case 0:
                pgp.hillsParameters = new SuperManager.SaveVector3(55f, 70f, 0.25f);
                pgp.forestParameters = new SuperManager.SaveVector3(80f, 90f, 0.3f);
                pgp.plainsParameters = new SuperManager.SaveVector3(85f, 100f, 0.3f);
                pgp.seed = 0;
                pgp.useSeed = false;
                break;
            default:
                break;
        }
        return pgp;
    }

    public void SetPriority(Priority priority)
    {
        if (allocationStructures == null)
        {
            allocationStructures = new List<Structure>();
        }
        else
        {
            allocationStructures.Clear();
        }
        allocationStructures.AddRange(FindObjectsOfType<ResourceStructure>());
        //allocationStructures.AddRange(FindObjectsOfType<AttackStructure>());
        //allocationStructures.AddRange(FindObjectsOfType<DefenseStructure>());
        DeallocateAll();

        switch (priority)
        {
            case Priority.BalancedProduction:
                // first even out with food
                AAProduceMinimumFood();

                // then distribute to all resources fairly
                AADistributeResources();

                // then distribute to defenses
                //AADistributeProtection();

                break;
            case Priority.Food:
                // first put all into food
                AAFillResourceType(ResourceType.Food);

                // then distribute to all resources fairly
                AADistributeResources();

                // then distribute to defenses
                //AADistributeProtection();

                break;
            case Priority.Wood:
                // first even out with food
                AAProduceMinimumFood();

                // then put all into wood
                AAFillResourceType(ResourceType.Wood);

                // then distribute to all resources fairly
                AADistributeResources();

                // then distribute to defenses
                //AADistributeProtection();
                break;
            case Priority.Metal:
                // first even out with food
                AAProduceMinimumFood();

                // then put all into wood
                AAFillResourceType(ResourceType.Metal);

                // then distribute to all resources fairly
                AADistributeResources();

                // then distribute to defenses
                //AADistributeProtection();
                break;
            case Priority.Defensive:
                // then distribute to defenses
                //AADistributeProtection();

                // first even out with food
                //AAProduceMinimumFood();

                // then distribute to all resources fairly
                //AADistributeResources();
                break;
            default:
                break;
        }
    }

    private void DeallocateAll()
    {
        foreach (Structure structure in FindObjectsOfType<ResourceStructure>())
        {
            structure.DeallocateAll();
        }
        /*
        foreach (Structure structure in FindObjectsOfType<DefenseStructure>())
        {
            structure.DeallocateAll();
        }
        foreach (Structure structure in FindObjectsOfType<AttackStructure>())
        {
            structure.DeallocateAll();
        }
        */
    }

    private void AAProduceMinimumFood()
    {
        int villagersRemaining = Longhaus.GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }
        // get the necessary amount of food for holding even
        float foodConsumptionPerSec = Longhaus.GetFoodConsumptionPerSec();
        List<Farm> farms = new List<Farm>();
        foreach (Structure structure in allocationStructures)
        {
            Farm farmComponent = structure.GetComponent<Farm>();
            if (farmComponent)
            {
                farms.Add(farmComponent);
            }
        }
        if (farms.Count > 0)
        {
            farms.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else
        {
            return;
        }
        float foodProductionPerSec = 0f;
        foreach(Farm farm in farms)
        {
            for (int i = 0; i < 3; i++)
            {
                farm.AllocateVillager();
                villagersRemaining--;
                foodProductionPerSec += farm.GetResourcePerVillPerSec();
                if (foodProductionPerSec >= foodConsumptionPerSec || villagersRemaining == 0)
                {
                    break;
                }
            }
            if (foodProductionPerSec >= foodConsumptionPerSec || villagersRemaining == 0)
            {
                break;
            }
        }
    }

    private void AAFillResourceType(ResourceType _resource)
    {
        // fill up the relevant structures until out of villagers or out of structures
        int villagersRemaining = Longhaus.GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }
        List<ResourceStructure> resStructures = new List<ResourceStructure>();
        switch (_resource)
        {
            case ResourceType.Wood:
                foreach (Structure structure in allocationStructures)
                {
                    LumberMill lumberComponent = structure.GetComponent<LumberMill>();
                    if (lumberComponent)
                    {
                        resStructures.Add(lumberComponent);
                    }
                }
                break;
            case ResourceType.Metal:
                foreach (Structure structure in allocationStructures)
                {
                    Mine mineComponent = structure.GetComponent<Mine>();
                    if (mineComponent)
                    {
                        resStructures.Add(mineComponent);
                    }
                }
                break;
            case ResourceType.Food:
                foreach (Structure structure in allocationStructures)
                {
                    Farm farmComponent = structure.GetComponent<Farm>();
                    if (farmComponent)
                    {
                        resStructures.Add(farmComponent);
                    }
                }
                break;
            default:
                break;
        }
        if (resStructures.Count > 0)
        {
            
            resStructures.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else
        {
            return;
        }
        foreach (ResourceStructure resStructure in resStructures)
        {
            for (int i = 0; i < 3; i++)
            {
                resStructure.AllocateVillager();
                villagersRemaining--;
                if (villagersRemaining == 0)
                {
                    break;
                }
            }
            if (villagersRemaining == 0)
            {
                break;
            }
        }
    }

    private void AADistributeProtection()
    {
        int villagersRemaining = Longhaus.GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }
        // allocate as fairly as possible between defensive structures untill either we are out of villagers, or out of structures
        List<AttackStructure> attackStructures = new List<AttackStructure>();
        foreach (Structure structure in allocationStructures)
        {
            AttackStructure attackStructure = structure.GetComponent<AttackStructure>();
            if (attackStructure)
            {
                attackStructures.Add(attackStructure);
            }
        }
        List<DefenseStructure> defenseStructures = new List<DefenseStructure>();
        foreach (Structure structure in allocationStructures)
        {
            DefenseStructure defenseStructure = structure.GetComponent<DefenseStructure>();
            if (defenseStructure)
            {
                defenseStructures.Add(defenseStructure);
            }
        }
        if (defenseStructures.Count == 0 && attackStructures.Count == 0)
        { 
            return;
        }
        // for each structure, allocate one villager, repeat untill villagers are empty or all structures are full
        for (int i = 0; i < 3; i++)
        {
            foreach (AttackStructure attack in attackStructures)
            {
                attack.AllocateVillager();
                villagersRemaining--;
                if (villagersRemaining == 0)
                {
                    break;
                }
            }
            if (villagersRemaining == 0)
            {
                break;
            }
            foreach (DefenseStructure defense in defenseStructures)
            {
                defense.AllocateVillager();
                villagersRemaining--;
                if (villagersRemaining == 0)
                {
                    break;
                }
            }
            if (villagersRemaining == 0)
            {
                break;
            }
        }
    }

    //
    // Date           : 29/07/2020
    // Author         : Sam
    // Input          : no parameters
    // Description    : Part of the Automatic Allocation System. Distributes villagers to resource structures fairly until either there are no more available villagers or all available structures are full.
    //
    private void AADistributeResources()
    {
        int villagersRemaining = Longhaus.GetAvailable();
        if (villagersRemaining == 0)
        {
            return;
        }

        List<Farm> farms = new List<Farm>();
        List<LumberMill> lumberMills = new List<LumberMill>();
        List<Mine> mines = new List<Mine>();

        foreach (Structure structure in allocationStructures)
        {
            Farm farmComponent = structure.GetComponent<Farm>();
            if (farmComponent)
            {
                farms.Add(farmComponent);
            }
        }
        foreach (Structure structure in allocationStructures)
        {
            LumberMill lumberComponent = structure.GetComponent<LumberMill>();
            if (lumberComponent)
            {
                lumberMills.Add(lumberComponent);
            }
        }
        foreach (Structure structure in allocationStructures)
        {
            Mine mineComponent = structure.GetComponent<Mine>();
            if (mineComponent)
            {
                mines.Add(mineComponent);
            }
        }

        if (farms.Count > 0)
        {
            farms.Sort(ResourceStructure.SortTileBonusDescending());
        }
        if (lumberMills.Count > 0)
        {
            lumberMills.Sort(ResourceStructure.SortTileBonusDescending());
        }
        if (mines.Count > 0)
        {
            mines.Sort(ResourceStructure.SortTileBonusDescending());
        }
        else if (farms.Count == 0 && lumberMills.Count == 0)
        {
            return;
        }

        List<Structure> farmStructures = new List<Structure>();
        List<Structure> lumberMillStructures = new List<Structure>();
        List<Structure> mineStructures = new List<Structure>();

        farmStructures.AddRange(farms);
        lumberMillStructures.AddRange(lumberMills);
        mineStructures.AddRange(mines);

        int farmCap = 0;
        int lumberCap = 0;
        int mineCap = 0;

        foreach (Farm farm in farms)
        {
            farmCap += 3 - farm.GetAllocated();
        }
        foreach (LumberMill lumberMill in lumberMills)
        {
            lumberCap += 3 - lumberMill.GetAllocated();
        }
        foreach (Mine mine in mines)
        {
            mineCap += 3 - mine.GetAllocated();
        }

        Vector3 velocity = gameMan.GetResourceVelocity();

        float foodProduction = velocity.z;
        float woodProduction = velocity.x;
        float metalProduction = velocity.y;

        ResourceType highest = ResourceType.Metal;
        if (foodProduction >= woodProduction && foodProduction >= metalProduction)
        {
            highest = ResourceType.Food;
        }
        else if (woodProduction >= foodProduction && woodProduction >= metalProduction)
        {
            highest = ResourceType.Wood;
        }


        while (villagersRemaining > 0 && (lumberCap > 0 || mineCap > 0 || farmCap > 0))
        {
            while (villagersRemaining > 0 && farmCap > 0 && (highest != ResourceType.Food || (lumberCap == 0 && mineCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(farmStructures);
                foodProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    farmCap--;
                    villagersRemaining--;
                }

                float highestProduction = woodProduction > metalProduction ? woodProduction : metalProduction;
                if (foodProduction > highestProduction)
                {
                    highest = ResourceType.Food;
                }
            }
            while (villagersRemaining > 0 && lumberCap > 0 && (highest != ResourceType.Wood || (farmCap == 0 && mineCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(lumberMillStructures);
                woodProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    lumberCap--;
                    villagersRemaining--;
                }

                float highestProduction = foodProduction > metalProduction ? foodProduction : metalProduction;
                if (woodProduction > highestProduction)
                {
                    highest = ResourceType.Wood;
                }
            }
            while (villagersRemaining > 0 && mineCap > 0 && (highest != ResourceType.Metal || (farmCap == 0 && lumberCap == 0)))
            {
                float resourceAdded = AAAlocateIntoNext(mineStructures);
                metalProduction += resourceAdded;
                if (resourceAdded > 0.0f)
                {
                    mineCap--;
                    villagersRemaining--;
                }

                float highestProduction = foodProduction > woodProduction ? foodProduction : woodProduction;
                if (metalProduction > highestProduction)
                {
                    highest = ResourceType.Metal;
                }
            }
        }
    }

    //
    // Date           : 29/07/2020
    // Author         : Sam
    // Input          : List<Structure> _structures, the list of structures to allocate into sequentially
    // Description    : Part of the Automatic Allocation System. Allocates a single villager to the first structure in _structures that has space for it.
    //
    private float AAAlocateIntoNext(List<Structure> _structures)
    {
        for (int i = 0; i < _structures.Count; i++)
        {
            if (_structures[i].GetAllocated() < 3)
            {
                _structures[i].AllocateVillager();
                ResourceStructure iAsResStruct = _structures[i].GetComponent<ResourceStructure>();
                if (iAsResStruct)
                {
                    return iAsResStruct.GetResourcePerVillPerSec();
                }
            }
        }
        return 0.0f;
    }
}

#if UNITY_EDITOR
public class ProceduralGenerationWindow : EditorWindow
{
    public static string kPathPresets;

    [MenuItem("EnvStructGeneration/Procedural Generation Window")]
    public static void Init()
    {
        ProceduralGenerationWindow window = (ProceduralGenerationWindow)GetWindow(typeof(ProceduralGenerationWindow));
        window.Show();
    }

    AnimBool PGseed;
    StructureManager selectedSM = null;
    ProceduralGenerationParameters currentPreset;
    Dictionary<string, ProceduralGenerationParameters> pgpPresets;
    string currentPresetName;
    bool showPresets = false;

    void OnEnable()
    {
        PGseed = new AnimBool(false);
        PGseed.valueChanged.AddListener(Repaint);
        kPathPresets = Application.dataPath + "/EnvStructGeneration/pgpPresets.dat";
    }

    private void Update()
    {
        if (!selectedSM)
        {
            selectedSM = FindObjectOfType<StructureManager>();
            LoadEditorData();
        }
    }

    void OnGUI()
    {
        if (selectedSM)
        {
            Vector2Int userReturn = EditorGUILayout.Vector2IntField("Hills Min/Max", currentPreset.hillsParameters);
            currentPreset.hillsParameters.x = userReturn.x; currentPreset.hillsParameters.y = userReturn.y;
            int percentageGrowthChance = (int)(currentPreset.hillsParameters.z * 100f);
            currentPreset.hillsParameters.z = EditorGUILayout.IntSlider("Hills G. Chance (%)", percentageGrowthChance, 0, 100) / 100f;

            userReturn = EditorGUILayout.Vector2IntField("Forest Min/Max", currentPreset.forestParameters);
            currentPreset.forestParameters.x = userReturn.x; currentPreset.forestParameters.y = userReturn.y;
            percentageGrowthChance = (int)(currentPreset.forestParameters.z * 100f);
            currentPreset.forestParameters.z = EditorGUILayout.IntSlider("Forest G. Chance (%)", percentageGrowthChance, 0, 100) / 100f;

            userReturn = EditorGUILayout.Vector2IntField("Plains Min/Max", currentPreset.plainsParameters);
            currentPreset.plainsParameters.x = userReturn.x; currentPreset.plainsParameters.y = userReturn.y;
            percentageGrowthChance = (int)(currentPreset.plainsParameters.z * 100f);
            currentPreset.plainsParameters.z = EditorGUILayout.IntSlider("Plains G. Chance (%)", percentageGrowthChance, 0, 100) / 100f;

            currentPreset.useSeed = EditorGUILayout.Toggle("Generate From Seed", currentPreset.useSeed);
            PGseed.target = currentPreset.useSeed;

            if (EditorGUILayout.BeginFadeGroup(PGseed.faded))
            {
                currentPreset.seed = EditorGUILayout.IntField("Seed", currentPreset.seed);
            }
            EditorGUILayout.EndFadeGroup();

            if (GUILayout.Button("Generate New Environment"))
            {
                SetSMProperties();
                if (selectedSM.editorGenerated) { ClearEnvironment(); }
                selectedSM.ProceduralGeneration(currentPreset.useSeed, currentPreset.seed);
                selectedSM.editorGenerated = true;
            }

            if (GUILayout.Button("Save Settings For Runtime"))
            {
                SaveCurrentForRuntime();
            }

            if (GUILayout.Button("Clear Environment"))
            {
                ClearEnvironment();
                selectedSM.editorGenerated = false;
            }

            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Clear Clones"))
            {
                ClearClones();
            }

            if (GUILayout.Button("Delete File..."))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("saveData.dat"), false, DeleteSaveData);
                menu.AddItem(new GUIContent("PGP.dat"), false, DeleteStructureManPGP);
                menu.AddItem(new GUIContent("pgpPresets.dat"), false, DeletePGPPresets);

                menu.ShowAsContext();
            }

            EditorGUILayout.Space(30f);

            showPresets = EditorGUILayout.BeginFoldoutHeaderGroup(showPresets, "Presets");

            if (showPresets)
            {
                if (pgpPresets == null) { LoadEditorData(); }

                currentPresetName = EditorGUILayout.TextField("Preset Name: ", currentPresetName);

                if (GUILayout.Button("Load Preset..."))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (KeyValuePair<string, ProceduralGenerationParameters> entry in pgpPresets)
                    {
                        menu.AddItem(new GUIContent(entry.Key), currentPresetName == entry.Key, PresetSelected, entry.Key);
                    }

                    menu.ShowAsContext();
                }

                if (GUILayout.Button("Save Preset"))
                {
                    if (pgpPresets.ContainsKey(currentPresetName))
                    {
                        // overwrite
                        pgpPresets[currentPresetName] = currentPreset;
                    }
                    else
                    {
                        // add
                        pgpPresets.Add(currentPresetName, currentPreset);
                    }
                    SaveEditorData();
                }

                if (GUILayout.Button("Delete Preset..."))
                {
                    GenericMenu menu = new GenericMenu();

                    foreach (KeyValuePair<string, ProceduralGenerationParameters> entry in pgpPresets)
                    {
                        menu.AddItem(new GUIContent(entry.Key), false, PresetDeleted, entry.Key);
                    }

                    menu.ShowAsContext();
                    SaveEditorData();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }

    void PresetSelected(object _presetName)
    {
        currentPresetName = (string)_presetName;
        currentPreset = pgpPresets[(string)_presetName];
    }

    void PresetDeleted(object _presetName)
    {
        if (pgpPresets.ContainsKey((string)_presetName))
        {
            pgpPresets.Remove((string)_presetName);
        }
    }

    void SetSMProperties()
    {
        selectedSM.forestEnvironmentBounds = currentPreset.forestParameters;
        selectedSM.recursiveFGrowthChance = currentPreset.forestParameters.z;
        selectedSM.hillsEnvironmentBounds = currentPreset.hillsParameters;
        selectedSM.recursiveHGrowthChance = currentPreset.hillsParameters.z;
        selectedSM.plainsEnvironmentBounds = currentPreset.plainsParameters;
        selectedSM.recursivePGrowthChance = currentPreset.plainsParameters.z;
    }


    public static void ClearEnvironment()
    {
        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            if (structure.name.Contains("PG"))
            {
                DestroyImmediate(structure.gameObject);
            }
        }
    }

    public static void ClearClones(bool _avoidPGElements = false)
    {

        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            if (structure.name.Contains("(Clone)"))
            {
                if (_avoidPGElements)
                {
                    if (structure.name.Contains("PG"))
                    {
                        // skip this one
                        continue;
                    }
                }
                DestroyImmediate(structure.gameObject);
            }
        }
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (enemy.name.Contains("(Clone)"))
            {
                DestroyImmediate(enemy.gameObject);
            }
        }
    }

    public static void DeleteSaveData()
    {
        if (System.IO.File.Exists(StructureManager.GetSaveDataPath()))
        {
            System.IO.File.Delete(StructureManager.GetSaveDataPath());
        }
    }

    public void DeletePGPPresets()
    {
        if (System.IO.File.Exists(kPathPresets))
        {
            System.IO.File.Delete(kPathPresets);
        }
    }

    public static void DeleteStructureManPGP()
    {
        if (System.IO.File.Exists(StructureManager.GetPGPPath()))
        {
            System.IO.File.Delete(StructureManager.GetPGPPath());
        }
    }

    void SaveCurrentForRuntime()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (System.IO.File.Exists(StructureManager.GetPGPPath()))
        {
            System.IO.File.Delete(StructureManager.GetPGPPath());
        }
        System.IO.FileStream file = System.IO.File.Create(StructureManager.GetPGPPath());

        bf.Serialize(file, currentPreset);

        file.Close();
    }

    void SaveEditorData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (System.IO.File.Exists(kPathPresets))
        {
            System.IO.File.Delete(kPathPresets);
        }
        System.IO.FileStream file = System.IO.File.Create(kPathPresets);

        bf.Serialize(file, pgpPresets);

        file.Close();
    }

    void LoadEditorData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (System.IO.File.Exists(kPathPresets))
        {
            System.IO.FileStream file = System.IO.File.Open(kPathPresets, System.IO.FileMode.Open);
            pgpPresets = (Dictionary<string, ProceduralGenerationParameters>)bf.Deserialize(file);
            file.Close();
        }
        else
        {
            // File does not exist, load defaults and save
            currentPreset = StructureManager.GetPGPHardPreset(0);
            currentPresetName = "Default";
            pgpPresets = new Dictionary<string, ProceduralGenerationParameters>
            {
                { currentPresetName, currentPreset }
            };
            System.IO.FileStream file = System.IO.File.Create(kPathPresets);
            bf.Serialize(file, pgpPresets);
            file.Close();
        }
    }
}
#endif