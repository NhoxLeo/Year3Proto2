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
    Food,
    Wood,
    Metal
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

public static class StructureNames
{
    public const string Longhaus = "Longhaus";

    public const string Barracks = "Barracks";
    public const string Ballista = "Ballista";
    public const string Catapult = "Catapult";
    public const string FreezeTower = "Freeze Tower";
    public const string ShockwaveTower = "Shockwave Tower";
    public const string LightningTower = "Lightning Tower";

    public const string FoodEnvironment = "Field";
    public const string FoodResource = "Farm";
    public const string FoodStorage = "Granary";

    public const string LumberEnvironment = "Forest";
    public const string LumberResource = "Lumber Mill";
    public const string LumberStorage = "Lumber Pile";

    public const string MetalEnvironment = "Hill";
    public const string MetalResource = "Mine";
    public const string MetalStorage = "Metal Storage";

    public static string BuildPanelToString(BuildPanel.Buildings _buildingID)
    {
        switch (_buildingID)
        {
            case BuildPanel.Buildings.Ballista:
                return Ballista;
            case BuildPanel.Buildings.Catapult:
                return Catapult;
            case BuildPanel.Buildings.Barracks:
                return Barracks;
            case BuildPanel.Buildings.FreezeTower:
                return FreezeTower;
            case BuildPanel.Buildings.ShockwaveTower:
                return ShockwaveTower;
            case BuildPanel.Buildings.LightningTower:
                return LightningTower;
            case BuildPanel.Buildings.Farm:
                return FoodResource;
            case BuildPanel.Buildings.LumberMill:
                return LumberResource;
            case BuildPanel.Buildings.Mine:
                return MetalResource;
            case BuildPanel.Buildings.Granary:
                return FoodStorage;
            case BuildPanel.Buildings.LumberPile:
                return LumberStorage;
            default:
            case BuildPanel.Buildings.None:
            case BuildPanel.Buildings.MetalStorage:
                return MetalStorage;
        }
    }
}

[ExecuteInEditMode]
public class StructureManager : MonoBehaviour
{
    private static StructureManager instance = null;

    // constants
    public static string PathSaveData;
    public static string PathPGP;

    private StructManState structureState = StructManState.Selecting;
    private bool firstStructurePlaced = false;
    private bool structureFromStore = false;
    private Structure hoveroverStructure = null;
    private Structure selectedStructure = null;
    private Structure structure = null;
    private TileBehaviour structureOldTile = null;
    private float hoveroverTime = 0f;
    private int nextStructureID = 0;
    public Transform selectedTileHighlight = null;
    public Transform tileHighlight = null;
    [HideInInspector]
    public bool editorGenerated = false;
    [HideInInspector]
    public bool useSeed = false;
    [HideInInspector]
    public int seed = 0;

    public static Dictionary<BuildPanel.Buildings, string> StructureDescriptions = new Dictionary<BuildPanel.Buildings, string>
    {
        { BuildPanel.Buildings.Ballista, "Fires deadly bolts at individual targets." },
        { BuildPanel.Buildings.Catapult, "Fires a large flaming boulder. Damages enemies in a small area." },
        { BuildPanel.Buildings.Barracks, "Spawns soldiers, who automatically attack enemies." },
        { BuildPanel.Buildings.FreezeTower, "Sprays enemies with ice to slow them down." },
        { BuildPanel.Buildings.ShockwaveTower, "Creates a large shockwave to repulse enemies." },
        { BuildPanel.Buildings.LightningTower, "Casts lightning on targeted enemies." },
        { BuildPanel.Buildings.Farm, "Collects Food from nearby field tiles. Bonus if constructed on field." },
        { BuildPanel.Buildings.Granary, "Increases maximum Food storage capacity." },
        { BuildPanel.Buildings.LumberMill, "Collects Wood from nearby forest tiles. Bonus if constructed on a forest." },
        { BuildPanel.Buildings.LumberPile, "Increases maximum Wood storage capacity." },
        { BuildPanel.Buildings.Mine, "Collects Metal from nearby rocky hill tiles. Bonus if constructed on hills." },
        { BuildPanel.Buildings.MetalStorage, "Increases maximum Metal storage capacity." }
    };

    public static Dictionary<string, BuildPanel.Buildings> StructureIDs = new Dictionary<string, BuildPanel.Buildings>
    {
        { StructureNames.Ballista, BuildPanel.Buildings.Ballista },
        { StructureNames.Catapult, BuildPanel.Buildings.Catapult },
        { StructureNames.Barracks, BuildPanel.Buildings.Barracks },
        { StructureNames.FreezeTower, BuildPanel.Buildings.FreezeTower },
        { StructureNames.ShockwaveTower, BuildPanel.Buildings.ShockwaveTower },
        { StructureNames.LightningTower, BuildPanel.Buildings.LightningTower },
        { StructureNames.FoodResource, BuildPanel.Buildings.Farm },
        { StructureNames.FoodStorage, BuildPanel.Buildings.Granary },
        { StructureNames.LumberResource, BuildPanel.Buildings.LumberMill },
        { StructureNames.LumberStorage, BuildPanel.Buildings.LumberPile },
        { StructureNames.MetalResource, BuildPanel.Buildings.Mine },
        { StructureNames.MetalStorage, BuildPanel.Buildings.MetalStorage }
    };
    public Dictionary<BuildPanel.Buildings, int> structureCounts = new Dictionary<BuildPanel.Buildings, int>
    {
        { BuildPanel.Buildings.Ballista, 0 },
        { BuildPanel.Buildings.Catapult, 0 },
        { BuildPanel.Buildings.Barracks, 0 },
        { BuildPanel.Buildings.FreezeTower, 0 },
        { BuildPanel.Buildings.ShockwaveTower, 0 },
        { BuildPanel.Buildings.LightningTower, 0 },
        { BuildPanel.Buildings.Farm, 0 },
        { BuildPanel.Buildings.Granary, 0 },
        { BuildPanel.Buildings.LumberMill, 0 },
        { BuildPanel.Buildings.LumberPile, 0 },
        { BuildPanel.Buildings.Mine, 0 },
        { BuildPanel.Buildings.MetalStorage, 0 }
    };
    public Dictionary<string, ResourceBundle> structureCosts = new Dictionary<string, ResourceBundle>
    {
        // NAME                                                wC       mC      fC
        { StructureNames.Ballista,          new ResourceBundle(150,     50,     0) },
        { StructureNames.Catapult,          new ResourceBundle(200,     250,    0) },
        { StructureNames.Barracks,          new ResourceBundle(200,     100,    0) },
        { StructureNames.FreezeTower,       new ResourceBundle(200,     50,     0) },
        { StructureNames.ShockwaveTower,    new ResourceBundle(200,     200,    0) },
        { StructureNames.LightningTower,    new ResourceBundle(200,     100,    0) },

        { StructureNames.FoodResource,      new ResourceBundle(40,      0,      0) },
        { StructureNames.LumberResource,    new ResourceBundle(60,      20,     0) },
        { StructureNames.MetalResource,     new ResourceBundle(100,     20,     0) },

        { StructureNames.FoodStorage,       new ResourceBundle(120,     0,      0) },
        { StructureNames.LumberStorage,     new ResourceBundle(120,     0,      0) },
        { StructureNames.MetalStorage,      new ResourceBundle(120,     80,     0) }
    };
    private Dictionary<int, Structure> playerStructureDict = new Dictionary<int, Structure>();
    [HideInInspector]
    public bool isOverUI = false;

    // defined at runtime
    public Dictionary<string, StructureDefinition> structureDict;
    private List<TileBehaviour> PGPlayableTiles;
    [HideInInspector]
    public Canvas canvas;
    [HideInInspector]
    public GameObject healthBarPrefab;
    [HideInInspector]
    public GameObject villagerWidgetPrefab;
    private BuildPanel panel;
    private GameObject buildingPuff;
    private BuildingInfo buildingInfo;
    private EnvInfo envInfo;
    private MessageBox messageBox;
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

    public static StructureManager GetInstance()
    {
        return instance;
    }

    public bool StructureIsSelected(Structure _structure)
    {
        return selectedStructure == _structure;
    }

    private void DefineDictionaries()
    {
        structureDict = new Dictionary<string, StructureDefinition>
        {
            // NAME                                                                    NAME                                                                        wC       mC      fC
            { StructureNames.Longhaus,          new StructureDefinition(Resources.Load("Structures/Longhaus")                   as GameObject,  new ResourceBundle(600,     200,    0)) },

            { StructureNames.Ballista,          new StructureDefinition(Resources.Load("Structures/Defense/Ballista Tower")     as GameObject,  new ResourceBundle(150,     50,     0)) },
            { StructureNames.Catapult,          new StructureDefinition(Resources.Load("Structures/Defense/Catapult Tower")     as GameObject,  new ResourceBundle(200,     250,    0)) },
            { StructureNames.Barracks,          new StructureDefinition(Resources.Load("Structures/Defense/Barracks")           as GameObject,  new ResourceBundle(200,     250,    0)) },
            { StructureNames.FreezeTower,       new StructureDefinition(Resources.Load("Structures/Defense/Freeze Tower")       as GameObject,  new ResourceBundle(200,     200,    0)) },
            { StructureNames.ShockwaveTower,    new StructureDefinition(Resources.Load("Structures/Defense/Shockwave Tower")    as GameObject,  new ResourceBundle(200,     200,    0)) },
            { StructureNames.LightningTower,    new StructureDefinition(Resources.Load("Structures/Defense/Lightning Tower")    as GameObject,  new ResourceBundle(200,     200,    0)) },


            { StructureNames.FoodResource,      new StructureDefinition(Resources.Load("Structures/Resource/Farm")              as GameObject,  new ResourceBundle(40,      0,      0)) },
            { StructureNames.LumberResource,    new StructureDefinition(Resources.Load("Structures/Resource/Lumber Mill")       as GameObject,  new ResourceBundle(60,      20,     0)) },
            { StructureNames.MetalResource,     new StructureDefinition(Resources.Load("Structures/Resource/Mine")              as GameObject,  new ResourceBundle(100,     20,     0)) },

            { StructureNames.FoodStorage,       new StructureDefinition(Resources.Load("Structures/Storage/Granary")            as GameObject,  new ResourceBundle(120,     0,      0)) },
            { StructureNames.LumberStorage,     new StructureDefinition(Resources.Load("Structures/Storage/Lumber Pile")        as GameObject,  new ResourceBundle(120,     0,      0)) },
            { StructureNames.MetalStorage,      new StructureDefinition(Resources.Load("Structures/Storage/Metal Storage")      as GameObject,  new ResourceBundle(120,     80,     0)) },

            { StructureNames.LumberEnvironment, new StructureDefinition(Resources.Load("Structures/Environment/Forest")         as GameObject,  new ResourceBundle(0,       0,      0)) },
            { StructureNames.MetalEnvironment,  new StructureDefinition(Resources.Load("Structures/Environment/Hills")          as GameObject,  new ResourceBundle(0,       0,      0)) },
            { StructureNames.FoodEnvironment,   new StructureDefinition(Resources.Load("Structures/Environment/Plains")         as GameObject,  new ResourceBundle(0,       0,      0)) },
        };
    }

    private void Awake()
    {
        instance = this;
        PathSaveData = GetSaveDataPath();
        PathPGP = GetPGPPath();
        DefineDictionaries();
        panel = FindObjectOfType<BuildPanel>();
        buildingInfo = FindObjectOfType<BuildingInfo>();
        canvas = FindObjectOfType<Canvas>();
        messageBox = FindObjectOfType<MessageBox>();
        envInfo = FindObjectOfType<EnvInfo>();
        healthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        villagerWidgetPrefab = Resources.Load("VillagerAllocationWidget") as GameObject;
        buildingPuff = Resources.Load("BuildEffect") as GameObject;
        GlobalData.longhausDead = false;
    }

    private void OnApplicationQuit()
    {
        SuperManager.GetInstance().SaveCurrentMatch();
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
        SuperManager superMan = SuperManager.GetInstance();
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
        if (!Application.isPlaying)
        {
            return;
        }

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //envInfo.SetVisibility(false);
        if (!isOverUI)
        {
            if (!tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(true); }
            switch (structureState)
            {
                case StructManState.Selecting:
                    UpdateSelecting(mouseRay);
                    break;
                case StructManState.Selected:
                    UpdateSelected(mouseRay);
                    break;
                case StructManState.Moving:
                    UpdateMoving(mouseRay);
                    break;
            }
        }
        if (isOverUI)
        {
            if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
            HideBuilding();
        }

    }

    private void UpdateSelecting(Ray _mouseRay)
    {
        // if the right mouse button isn't being held down...
        if (!Input.GetMouseButton(1))
        {
            // if the player is hovering over a structure...
            if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitStructure, Mathf.Infinity, LayerMask.GetMask("Structure")))
            {
                Structure hitStructureComponent = hitStructure.transform.GetComponent<Structure>();

                // If the structure is attached to a playable tile...
                if (hitStructureComponent.attachedTile.GetPlayable())
                {
                    // turn on the tile highlight.
                    tileHighlight.gameObject.SetActive(true);

                    // move the highlight to the position the player is hovering over.
                    Vector3 highlightpos = hitStructure.transform.position;
                    highlightpos.y = 0.501f;
                    tileHighlight.position = highlightpos;

                    // Respond to the fact that the player's hovering over the structure.
                    PlayerMouseOver(hitStructureComponent);

                    // If the player clicks the LMB...
                    if (Input.GetMouseButtonDown(0))
                    {
                        // Select that structure.
                        SelectStructure(hitStructureComponent);
                    }
                }
                else // the structure isn't attached to a playable tile...
                {
                    // turn off the tile highlight.
                    tileHighlight.gameObject.SetActive(false);
                    // hide the environment info panel.
                    //envInfo.SetVisibility(false);
                }
            }
            // if the player is hovering over a tile...
            else if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitGround, Mathf.Infinity, LayerMask.GetMask("Ground")))
            {
                // if the tile they hit is playable...
                if (hitGround.transform.GetComponent<TileBehaviour>().GetPlayable())
                {
                    // turn on the tile highlight.
                    tileHighlight.gameObject.SetActive(true);

                    // move the highlight to the position the player is hovering over.
                    Vector3 highlightpos = hitGround.transform.position;
                    highlightpos.y = 0.501f;
                    tileHighlight.position = highlightpos;
                }
                else
                {
                    // turn off the tile highlight.
                    tileHighlight.gameObject.SetActive(false);
                }
                // report no hover-over-ing
                PlayerMouseOver(null);
            }
            else
            {
                // turn off the tile highlight.
                tileHighlight.gameObject.SetActive(false);

                // report no hover-over-ing
                PlayerMouseOver(null);
            }
        }
        else
        {
            // turn off the tile highlight.
            tileHighlight.gameObject.SetActive(false);

            // report no hover-over-ing
            PlayerMouseOver(null);
        }
    }

    private void UpdateSelected(Ray _mouseRay)
    {
        // if there isn't a structure selected...
        if (!selectedStructure)
        {
            // formally switch to another state.
            DeselectStructure();
            // stop short
            return;
        }

        // If the player presses the delete key...
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            StructureType selectedStructType = selectedStructure.GetStructureType();

            // if the structure type can be deleted...
            if (selectedStructType != StructureType.Environment && selectedStructType != StructureType.Longhaus)
            {
                // destroy the building
                DestroySelectedBuilding();
                return;
            }
        }

        Vector3 highlightpos = selectedStructure.transform.position;
        highlightpos.y = 0.501f;
        selectedTileHighlight.position = highlightpos;

        if (!Input.GetMouseButton(1))
        {
            if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitStructure, Mathf.Infinity, LayerMask.GetMask("Structure")))
            {
                Structure hitStructureComponent = hitStructure.transform.GetComponent<Structure>();
                if (hitStructureComponent.attachedTile.GetPlayable())
                {
                    tileHighlight.gameObject.SetActive(true);

                    highlightpos = hitStructure.transform.position;
                    highlightpos.y = 0.501f;
                    tileHighlight.position = highlightpos;

                    // If the player clicks the LMB...
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (hitStructureComponent != selectedStructure)
                        {
                            if (hitStructureComponent.attachedTile.GetPlayable())
                            {
                                SelectStructure(hitStructureComponent);
                            }
                        }
                    }
                    else
                    {
                        PlayerMouseOver(hitStructureComponent);
                    }
                }
                else
                {
                    if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                }
            }
            else
            {
                PlayerMouseOver(null);
                // If the player clicks the LMB...
                if (Input.GetMouseButtonDown(0))
                {
                    DeselectStructure();
                    return;
                }
                if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitGround, Mathf.Infinity, LayerMask.GetMask("Ground")))
                {
                    if (hitGround.transform.GetComponent<TileBehaviour>().GetPlayable())
                    {
                        tileHighlight.gameObject.SetActive(true);
                        highlightpos = hitGround.transform.position;
                        highlightpos.y = 0.501f;
                        tileHighlight.position = highlightpos;
                    }
                    else
                    {
                        tileHighlight.gameObject.SetActive(false);
                    }
                }
                else
                {
                    tileHighlight.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            tileHighlight.gameObject.SetActive(false);
            PlayerMouseOver(null);
        }
    }

    private void UpdateMoving(Ray _mouseRay)
    {
        if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
        if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitGround, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            TileBehaviour tile = hitGround.transform.GetComponent<TileBehaviour>();
            if (tile.GetPlayable())
            {
                bool canPlaceHere = true;
                // If the tile we hit has an attached object...
                Structure attached = tile.GetAttached();
                StructureType newStructureType = structure.GetStructureType();
                if (attached)
                {
                    canPlaceHere = false;
                    Vector3 hitPos = hitGround.point;
                    hitPos.y = structure.sitHeight;
                    structure.transform.position = hitPos;

                    SetStructureColour(Color.red);

                    if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                    if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }

                    if (attached.GetStructureType() == StructureType.Environment)
                    {
                        canPlaceHere = true;
                    }
                }
                // if the structure can be placed here...
                if (canPlaceHere)
                {
                    if (newStructureType == StructureType.Defense)
                    {
                        structure.ShowRangeDisplay(true);
                    }

                    if (attached)
                    {
                        if (attached.GetStructureType() == StructureType.Environment)
                        {
                            string attachedName = attached.GetStructureName();
                            string structureName = structure.GetStructureName();
                            // determine if the structure is in synergy with attached structure
                            bool resourceGain = (attachedName == StructureNames.FoodEnvironment && structureName == StructureNames.FoodResource)
                                || (attachedName == StructureNames.LumberEnvironment && structureName == StructureNames.LumberResource)
                                || (attachedName == StructureNames.MetalEnvironment && structureName == StructureNames.MetalResource);
                            if (resourceGain)
                            {
                                SetStructureColour(Color.green);
                            }
                            else
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
                    if (!GameManager.GetInstance().playerResources.CanAfford(structureCosts[structure.GetStructureName()]))
                    {
                        SetStructureColour(Color.red);
                    }

                    Vector3 structPos = hitGround.transform.position;
                    structPos.y = structure.sitHeight;
                    structure.transform.position = structPos;

                    Vector3 highlightPos = structPos;
                    highlightPos.y = 0.501f;
                    tileHighlight.position = highlightPos;
                    selectedTileHighlight.position = highlightPos;

                    tileHighlight.gameObject.SetActive(true);

                    // If the user clicked the LMB...
                    if (Input.GetMouseButtonDown(0))
                    {
                        AttemptPlaceStructure(tile);
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
            }
            else
            {
                SelectStructure(structure);
            }
            messageBox.HideMessage();
        }
    }

    public void AttemptPlaceStructure(TileBehaviour _tile)
    {
        Structure attached = _tile.GetAttached();
        StructureType structType = structure.GetStructureType();
        if ((structureFromStore && BuyBuilding()) || !structureFromStore)
        {
            GameManager.CreateAudioEffect("build", structure.transform.position, 0.6f);
            SetStructureColour(Color.white);
            // Attach the structure to the tile and vica versa
            if (attached) { attached.attachedTile.Detach(); }
            _tile.Attach(structure);
            structure.SetID(GetNewID());
            structure.OnPlace();
            Instantiate(buildingPuff, structure.transform.position, Quaternion.Euler(-90f, 0f, 0f));
            if (attached)
            {
                string attachedName = attached.GetStructureName();
                string structName = structure.GetStructureName();
                if (attachedName == StructureNames.FoodEnvironment && structName == StructureNames.FoodResource)
                {
                    GameManager.GetInstance().AddBatch(new ResourceBatch(50, ResourceType.Food));
                    structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                }
                else if (attachedName == StructureNames.LumberEnvironment && structName == StructureNames.LumberResource)
                {
                    GameManager.GetInstance().AddBatch(new ResourceBatch(50, ResourceType.Wood));
                    structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                }
                else if (attachedName == StructureNames.MetalEnvironment && structName == StructureNames.MetalResource)
                {
                    GameManager.GetInstance().AddBatch(new ResourceBatch(50, ResourceType.Metal));
                    structure.GetComponent<Mine>().wasPlacedOnHills = true;
                }
                Destroy(attached.gameObject);
            }
            if (structureFromStore)
            {
                panel.UINoneSelected();
                panel.ResetBuildingSelected();
            }
            if (!firstStructurePlaced)
            {
                if (!EnemyManager.GetInstance().GetSpawning())
                {
                    EnemyManager.GetInstance().SetSpawning(true);
                }
                firstStructurePlaced = true;
            }
            bool villWidget = structType == StructureType.Resource || structType == StructureType.Defense;
            if (structure.IsStructure(StructureNames.Barracks) || structure.IsStructure(StructureNames.FreezeTower))
            {
                villWidget = false;
            }
            if (villWidget)
            {
                VillagerAllocation villagerAllocation = Instantiate(villagerWidgetPrefab, canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
                villagerAllocation.SetTarget(structure);
                structure.SetAllocationWidget(villagerAllocation);
            }
            playerStructureDict.Add(structure.GetID(), structure);
            GameManager.GetInstance().OnStructurePlaced();
            PathManager.GetInstance().ClearPaths();
            VillagerManager.GetInstance().RedistributeVillagers();
            SelectStructure(structure);
            if (villWidget)
            {
                structure.RefreshWidget();
                structure.SetWidgetVisibility(true);
            }
        }
    }

    public void DestroySelectedBuilding()
    {
        selectedStructure.DeallocateAll();
        float health = selectedStructure.GetHealth();
        ResourceBundle compensation = QuoteCompensationFor(selectedStructure);
        selectedStructure.Damage(health);
        GameManager.GetInstance().playerResources.AddResourceBundle(compensation);
        HUDManager.GetInstance().ShowResourceDelta(compensation, false);
        DeselectStructure();
    }

    public ResourceBundle QuoteCompensationFor(Structure _structure)
    {
        float health = _structure.GetHealth();
        float maxHealth = _structure.GetTrueMaxHealth();
        return new ResourceBundle(0.5f * (health / maxHealth) * (Vector3)structureCosts[_structure.GetStructureName()]);
    }

    public ResourceBundle QuoteUpgradeCostFor(DefenseStructure _structure)
    {
        int level = _structure.GetLevel();
        if (level == 3)
        {
            return new ResourceBundle(0, 0, 0);
        }
        int cost = 200 * level;
        ResourceBundle result = new ResourceBundle(cost, cost, 0);
        return result;
    }

    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            ResourceBundle cost = structureCosts[structure.GetStructureName()];
            if (GameManager.GetInstance().playerResources.AttemptPurchase(cost))
            {
                IncreaseStructureCost(structure.GetStructureName());
                HUDManager.GetInstance().ShowResourceDelta(cost, true);
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
        if (SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SnoballPrices))
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
        structureState = StructManState.Selecting;
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
        structureState = StructManState.Selected;
        selectedStructure.OnSelected();

        selectedTileHighlight.gameObject.SetActive(true);

        Vector3 highlightpos = selectedStructure.attachedTile.transform.position;
        highlightpos.y = 0.501f;
        selectedTileHighlight.position = highlightpos;

        buildingInfo.SetTargetBuilding(selectedStructure.gameObject);
        buildingInfo.showPanel = true;
    }

    public bool SetBuilding(string _building)
    {
        if (structureState != StructManState.Moving)
        {
            DeselectStructure();
            // Put the manager back into moving mode.
            structureState = StructManState.Moving;
            structureFromStore = true;
            GameObject structureInstance = Instantiate(structureDict[_building].structurePrefab);
            structureInstance.transform.position = Vector3.down * 10f;
            structure = structureInstance.GetComponent<Structure>();
            if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
            selectedStructure = null;
            buildingInfo.showPanel = false;
            return true;
        }
        return false;
    }

    public bool SetBuilding(BuildPanel.Buildings _buildingID)
    {
        return SetBuilding(StructureNames.BuildPanelToString(_buildingID));
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
            PGRecursiveWander(StructureNames.MetalEnvironment, tile, ref hillsPlaced, hillsTotal, recursiveHGrowthChance);
        }

        int forestPlaced = 0;
        while (forestPlaced < forestTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander(StructureNames.LumberEnvironment, tile, ref forestPlaced, forestTotal, recursiveFGrowthChance);
        }

        int plainsPlaced = 0;
        while (plainsPlaced < plainsTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander(StructureNames.FoodEnvironment, tile, ref plainsPlaced, plainsTotal, recursivePGrowthChance);
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
        structure.SetID(GetNewID());
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
        // disabled
        /*
        if (!_structure)
        {
            hoveroverStructure = null;
            hoveroverTime = 0f;
        }
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
            envInfo.SetInfoByStructure(_structure);
        }
        */
    }

    private void ShowMessage(string _message, float _duration)
    {
        if (GameManager.GetInstance().tutorialDone)
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
        DefenseStructure defense = newStructure.gameObject.GetComponent<DefenseStructure>();
        if (defense)
        {
            defense.SetLevel(_saveData.level);
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
        if (newStructure.GetStructureType() != StructureType.Environment)
        {
            playerStructureDict.Add(_saveData.ID, newStructure);
            StructureType structType = newStructure.GetStructureType();
            bool villWidget = structType == StructureType.Resource || structType == StructureType.Defense;
            if (newStructure.IsStructure(StructureNames.Barracks) || newStructure.IsStructure(StructureNames.FreezeTower))
            {
                villWidget = false;
            }
            if (villWidget)
            {
                VillagerAllocation villagerAllocation = Instantiate(villagerWidgetPrefab, canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
                villagerAllocation.SetTarget(newStructure);
                newStructure.SetAllocationWidget(villagerAllocation);
            }
        }
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

    public int GetPlayerStructureCount()
    {
        return playerStructureDict.Count;
    }

    public Dictionary<int, Structure>.ValueCollection GetPlayerStructures()
    {
        return playerStructureDict.Values;
    }

    public void OnStructureDestroyed(Structure _structure)
    {
        DecreaseStructureCost(_structure.GetStructureName());
        if (playerStructureDict.ContainsKey(_structure.GetID()))
        {
            playerStructureDict.Remove(_structure.GetID());
        }
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
