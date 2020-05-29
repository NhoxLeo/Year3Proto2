﻿using System.Collections;
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
    selecting,
    selected,
    moving
};

[Serializable]
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
    public static string kPathSaveData;
    public static string kPathPGP;

    // PRE-DEFINED
    private StructManState structureState = StructManState.selecting;
    private bool towerPlaced = false;
    private bool structureFromStore = false;
    private Structure hoveroverStructure = null;
    private Structure selectedStructure = null;
    private Structure structure = null;
    private TileBehaviour structureOldTile = null;
    private float hoveroverTime = 0f;

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
    public Dictionary<BuildPanel.Buildings, int> structureCounts = new Dictionary<BuildPanel.Buildings, int>
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

    private SuperManager superMan;
    private HUDManager HUDman;
    private GameManager gameMan;
    private BuildPanel panel;
    private GameObject buildingPuff;
    private EnemySpawner enemySpawner;
    private BuildingInfo buildingInfo;
    private EnvInfo envInfo;
    private MessageBox messageBox;

    private void Awake()
    {
        kPathSaveData = Application.persistentDataPath + "/saveData.dat";
        kPathPGP = Application.persistentDataPath + "/PGP.dat";
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
        superMan = SuperManager.GetInstance();
        healthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        buildingPuff = Resources.Load("BuildEffect") as GameObject;
        // read from disk PGPs. if they cannot be found, create & save them
    }

    private void OnApplicationQuit()
    {
        superMan.SaveCurrentMatch();
    }

    public static string GetSaveDataPath()
    {
        return kPathSaveData == null ? kPathSaveData = Application.persistentDataPath + "/saveData.dat" : kPathSaveData;
    }

    public static string GetPGPPath()
    {
        return kPathPGP == null ? kPathPGP = Application.persistentDataPath + "/PGP.dat" : kPathPGP;
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
        if (System.IO.File.Exists(kPathPGP))
        {
            System.IO.FileStream file = System.IO.File.Open(kPathPGP, System.IO.FileMode.Open);
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
            System.IO.FileStream file = System.IO.File.Create(kPathPGP);
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
                        if (!selectedStructure)
                        {
                            DeselectStructure();
                            structureState = StructManState.selecting;
                            break;
                        }

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
                                                else if (attached.IsStructure("Hills Environment") && structure.IsStructure("Mine")) { canPlaceHere = true; }
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
                                                if (!gameMan.playerResources.CanAfford(structureCosts[structure.GetStructureName()]))
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
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.wood));
                                                                        structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                                                                        break;
                                                                    case "Farm":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.food));
                                                                        structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                                                                        break;
                                                                    case "Mine":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.metal));
                                                                        structure.GetComponent<Mine>().wasPlacedOnHills = true;
                                                                        break;
                                                                }
                                                            }
                                                            else if (attachedStructType == StructureType.environment && (structType == StructureType.attack || structType == StructureType.storage))
                                                            {
                                                                switch (attached.GetStructureName())
                                                                {
                                                                    case "Forest Environment":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.wood));
                                                                        break;
                                                                    case "Hill Environment":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.metal));
                                                                        break;
                                                                    case "Plains Environment":
                                                                        gameMan.playerResources.AddBatch(new ResourceBatch(50, ResourceType.food));
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
                                                            if (!enemySpawner.IsSpawning())
                                                            {
                                                                enemySpawner.ToggleSpawning();
                                                            }
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

    public void IncreaseStructureCost(string _structureName)
    {
        if (superMan.CurrentLevelHasModifier(0))
        {
            if (!structureCosts.ContainsKey(_structureName))
            {
                return;
            }
            int buildingCount = ++structureCounts[StructureIDs[_structureName]];
            Vector3 newCost = (2f + buildingCount) / 2f * (Vector3)structureDict[_structureName].originalCost;
            structureCosts[_structureName] = new ResourceBundle(newCost);
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = newCost;
        }
    }

    public void DecreaseStructureCost(string _structureName)
    {
        if (superMan.CurrentLevelHasModifier(0))
        {
            if (!structureCosts.ContainsKey(_structureName))
            {
                return;
            }
            int buildingCount = --structureCounts[StructureIDs[_structureName]];
            if (buildingCount < 0) { buildingCount = structureCounts[StructureIDs[_structureName]] = 0; }
            Vector3 newCost = (2f + buildingCount) / 2f * (Vector3)structureDict[_structureName].originalCost;
            structureCosts[_structureName] = new ResourceBundle(newCost);
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = newCost;
        }
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
                TileBehaviour tileI = tile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
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
                TileBehaviour tileI = tile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
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
                TileBehaviour tileI = tile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];
                if (PGPlayableTiles.Count == 0) { Debug.LogError("PG: Ran out of tiles, try lower values for \"Plains Environemnt Bounds\" and the other environment types."); }
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
            TileBehaviour tileI = _tile.GetAdjacentTiles()[(TileBehaviour.TileCode)i];
            if (PGPlayableTiles.Contains(tileI))
            {
                if (UnityEngine.Random.Range(0f, 100f) <= _recursiveChance * 100f)
                {
                    PGRecursiveWander(_environmentType, tileI, ref _placed, _max, _recursiveChance);
                }
            }
        }
    }

    private void PGInstatiateEnvironment(string _environmentType, TileBehaviour _tile)
    {
        // define the structures when in editor mode
        if (!Application.isPlaying)
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
        newStructure.SetFoodAllocation(_saveData.foodAllocation);
        ResourceStructure resourceStructComp = newStructure.gameObject.GetComponent<ResourceStructure>();
        if (resourceStructComp != null)
        {
            if (_saveData.structure == "Farm") { newStructure.gameObject.GetComponent<Farm>().wasPlacedOnPlains = _saveData.wasPlacedOn; }
            if (_saveData.structure == "Mine") { newStructure.gameObject.GetComponent<Mine>().wasPlacedOnHills = _saveData.wasPlacedOn; }
            if (_saveData.structure == "Lumber Mill") { newStructure.gameObject.GetComponent<LumberMill>().wasPlacedOnForest = _saveData.wasPlacedOn; }
        }
        newStructure.isPlaced = true;
        newStructure.SetHealth(_saveData.health);
        newStructure.fromSaveData = true;
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
                pgp.hillsParameters = new SuperManager.SaveVector3(60f, 80f, 0.5f);
                pgp.forestParameters = new SuperManager.SaveVector3(60f, 80f, 0.3f);
                pgp.plainsParameters = new SuperManager.SaveVector3(60f, 80f, 0.3f);
                pgp.seed = 0;
                pgp.useSeed = false;
                break;
            default:
                break;
        }
        return pgp;
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
        selectedSM.recursiveFGrowthChance = currentPreset.hillsParameters.z;
        selectedSM.plainsEnvironmentBounds = currentPreset.plainsParameters;
        selectedSM.recursiveFGrowthChance = currentPreset.plainsParameters.z;
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
            pgpPresets = new Dictionary<string, ProceduralGenerationParameters>();
            pgpPresets.Add(currentPresetName, currentPreset);
            System.IO.FileStream file = System.IO.File.Create(kPathPresets);
            bf.Serialize(file, pgpPresets);
            file.Close();
        }
    }
}
#endif