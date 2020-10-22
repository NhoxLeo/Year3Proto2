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

#region Structs

[Serializable]
public struct ResourceBundle
{
    public float food;
    public float wood;
    public float metal;
    public ResourceBundle(float _fCost, float _wCost, float _mCost)
    {
        food = _fCost;
        wood = _wCost;
        metal = _mCost;
    }
    public static ResourceBundle operator *(ResourceBundle _kStructureDefinition, float _otherHS)
    {
        ResourceBundle newStructure = _kStructureDefinition;
        newStructure.food *= _otherHS;
        newStructure.wood *= _otherHS;
        newStructure.metal *= _otherHS;
        return newStructure;
    }

    public static implicit operator Vector3(ResourceBundle _rb)
    {
        Vector3 vec;
        vec.x = _rb.food;
        vec.y = _rb.wood;
        vec.z = _rb.metal;
        return vec;
    }

    public static ResourceBundle operator + (ResourceBundle _LHS, ResourceBundle _RHS)
    {
        ResourceBundle sum;
        sum.food = _LHS.food + _RHS.food;
        sum.wood = _LHS.wood + _RHS.wood;
        sum.metal = _LHS.metal + _RHS.metal;
        return sum;
    }

    public ResourceBundle(Vector3 _vec)
    {
        food = _vec.x;
        wood = _vec.y;
        metal = _vec.z;
    }

    public bool IsEmpty()
    {
        return wood == 0 && metal == 0 && food == 0;
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

#endregion

public static class StructureMaterials
{
    private static Dictionary<(string, bool), List<Material>> materials = new Dictionary<(string, bool), List<Material>>();

    private static List<string> GetPathsFromKey((string, bool) _key)
    {
        List<string> paths = new List<string>();
        switch (_key.Item1)
        {
            case StructureNames.Longhaus:
                paths.Add("Materials/Structures/Longhaus/mLonghaus" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Longhaus/mLonghausRoof" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.FoodStorage:
                paths.Add("Materials/Structures/Storage/mGranary" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.LumberStorage:
                paths.Add("Materials/Structures/Storage/mLumberStorage" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.MetalStorage:
                paths.Add("Materials/Structures/Storage/mMetalStorage" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.MetalEnvironment:
                paths.Add("Materials/Structures/Environment/mHillRocks" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Environment/mHillGrass" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.LumberEnvironment:
                paths.Add("Materials/Structures/Environment/mForestGround" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Environment/mForest" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Environment/mForestGrass" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.LumberEnvironment + StructureNames.Alt:
                paths.Add("Materials/Structures/Environment/mForestTile" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Environment/mForestFence" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.FoodResource:
                paths.Add("Materials/Structures/Resource/mFarmGrazingField" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Environment/mFields");
                paths.Add("Materials/Structures/Resource/mFarm" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.FoodResource + StructureNames.Alt:
                paths.Add("Materials/Structures/Resource/mFarmFence" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.LumberResource:
                paths.Add("Materials/Structures/Resource/mLumberMill" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.MetalResource:
                paths.Add("Materials/Structures/Resource/mMine" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.MetalResource + StructureNames.Alt:
                paths.Add("Materials/Structures/Resource/mMinePlatform" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.Barracks:
                paths.Add("Materials/Structures/Defense/mBarracks" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Defense/mBarracksGround" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.Ballista:
                paths.Add("Materials/Structures/Defense/mBallista" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Defense/mBallistaGround" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.Catapult:
                paths.Add("Materials/Structures/Defense/mCatapult" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.LightningTower:
                paths.Add("Materials/Structures/Defense/mLightningCrystal");
                paths.Add("Materials/Structures/Defense/mLightningTower" + (_key.Item2 ? "_Snow" : ""));
                break;
            case StructureNames.FrostTower:
                paths.Add("Materials/Structures/Defense/mFreezeTower" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Defense/mFreezeGround" + (_key.Item2 ? "_Snow" : ""));
                paths.Add("Materials/Structures/Defense/mFreezeFaceTwo");
                paths.Add("Materials/Structures/Defense/mFreezeFaceOne");
                break;
            case StructureNames.ShockwaveTower:
                paths.Add("Materials/Structures/Defense/mShockwaveTower" + (_key.Item2 ? "_Snow" : ""));
                break;
            default:
                break;
        }
        return paths;
    }

    public static List<Material> Fetch(string _name, bool _snow)
    {
        (string, bool) key = (_name, _snow);
        if (!materials.ContainsKey(key))
        {
            List<Material> materialList = new List<Material>();
            List<string> paths = GetPathsFromKey(key);
            foreach (string path in paths)
            {
                materialList.Add(Resources.Load(path) as Material);
            }
            materials.Add(key, materialList);
        }
        return materials[key];
    }
}

public static class StructureNames
{
    public const string Longhaus = "Longhaus";

    public const string Barracks = "Barracks";
    public const string Ballista = "Ballista";
    public const string Catapult = "Catapult";
    public const string FrostTower = "Frost Tower";
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

    public const string Alt = "_Alt";

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
            case BuildPanel.Buildings.FrostTower:
                return FrostTower;
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

public class StructureManager : MonoBehaviour
{
    private static StructureManager instance = null;

    // constants
    public static string PathSaveData;
    public static string PathPGP;

    private StructManState structureState = StructManState.Selecting;
    private bool firstStructurePlaced = false;
    private bool structureFromStore = false;
    private Structure selectedStructure = null;
    private Structure structure = null;
    private TileBehaviour structureOldTile = null;
    private int nextStructureID = 0;
    protected static GameObject TileHighlight = null;
    protected static GameObject BonusHighlight = null;
    public Transform selectedTileHighlight = null;
    public Transform tileHighlight = null;
    [HideInInspector]
    public bool editorGenerated = false;
    [HideInInspector]
    public bool useSeed = false;
    [HideInInspector]
    public int seed = 0;
    private Vector3 mpAtRightDown = new Vector2();
    private List<Transform> resourceHighlights;
    private EnvironmentStructure hoverEnvironment = null;
    public const float HighlightSitHeight = 0.54f;
    public const float BonusHighlightSitHeight = 1f;
    private bool opacityAscending = false;
    private const float OpacitySpeed = 0.6f;
    private const float OpacityMinimum = 0.2f;
    private const float OpacityMaximum = 0.7f;
    private float opacity = OpacityMaximum;
    private const float ColourLerpAmount = 0.4f;
    private static GameObject ProceduralGenerationParent = null;
    private static GameObject LoadedStructuresParent = null;
    private static GameObject NewStructuresParent = null;

    public static Dictionary<BuildPanel.Buildings, string> StructureDescriptions = new Dictionary<BuildPanel.Buildings, string>
    {
        { BuildPanel.Buildings.Ballista, "Fires deadly bolts at individual targets." },
        { BuildPanel.Buildings.Catapult, "Fires a large flaming boulder. Damages enemies in a small area." },
        { BuildPanel.Buildings.Barracks, "Spawns soldiers, who automatically attack enemies." },
        { BuildPanel.Buildings.FrostTower, "Sprays enemies with ice to slow them down." },
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
        { StructureNames.FrostTower, BuildPanel.Buildings.FrostTower },
        { StructureNames.ShockwaveTower, BuildPanel.Buildings.ShockwaveTower },
        { StructureNames.LightningTower, BuildPanel.Buildings.LightningTower },
        { StructureNames.FoodResource, BuildPanel.Buildings.Farm },
        { StructureNames.LumberResource, BuildPanel.Buildings.LumberMill },
        { StructureNames.MetalResource, BuildPanel.Buildings.Mine },
        { StructureNames.FoodStorage, BuildPanel.Buildings.Granary },
        { StructureNames.LumberStorage, BuildPanel.Buildings.LumberPile },
        { StructureNames.MetalStorage, BuildPanel.Buildings.MetalStorage }
    };

    public Dictionary<BuildPanel.Buildings, int> structureCounts = new Dictionary<BuildPanel.Buildings, int>
    {
        { BuildPanel.Buildings.Ballista, 0 },
        { BuildPanel.Buildings.Catapult, 0 },
        { BuildPanel.Buildings.Barracks, 0 },
        { BuildPanel.Buildings.FrostTower, 0 },
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
        // NAME                                                fC       wC       mC    
        { StructureNames.Barracks,          new ResourceBundle(0,       150,     25) },
        { StructureNames.Ballista,          new ResourceBundle(0,       200,     125) },
        { StructureNames.Catapult,          new ResourceBundle(0,       50,      250) },
        { StructureNames.FrostTower,       new ResourceBundle(0,       100,     150) },
        { StructureNames.ShockwaveTower,    new ResourceBundle(0,       100,     200) },
        { StructureNames.LightningTower,    new ResourceBundle(0,       50,      200) },

        { StructureNames.FoodResource,      new ResourceBundle(0,       50,      0) },
        { StructureNames.LumberResource,    new ResourceBundle(0,       50,      0) },
        { StructureNames.MetalResource,     new ResourceBundle(0,       100,     0) },

        { StructureNames.FoodStorage,       new ResourceBundle(0,       100,     0) },
        { StructureNames.LumberStorage,     new ResourceBundle(0,       120,     40) },
        { StructureNames.MetalStorage,      new ResourceBundle(0,       160,     40) }
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
    public static GameObject HealthBarPrefab;
    [HideInInspector]
    public GameObject villagerWidgetPrefab;
    private BuildPanel panel;
    private GameObject buildingPuff;
    private BuildingInfo buildingInfo;
    private EnvInfo envInfo;
    private MessageBox messageBox;

    private const int PlainsInStartingArea = 24;
    private const int ForestsInStartingArea = 18;
    private const int HillsInStartingArea = 12;

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

    #region Unity Messages

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
        HealthBarPrefab = Resources.Load("BuildingHP") as GameObject;
        villagerWidgetPrefab = Resources.Load("VillagerAllocationWidget") as GameObject;
        buildingPuff = Resources.Load("BuildEffect") as GameObject;
        GlobalData.longhausDead = false;
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
            // add the Longhaus to the player structure dictionary
            playerStructureDict.Add(GetNewID(), FindObjectOfType<Longhaus>());
            resourceHighlights = new List<Transform>()
            {
                Instantiate(GetBonusHighlight()).transform,
                Instantiate(GetBonusHighlight()).transform,
                Instantiate(GetBonusHighlight()).transform,
                Instantiate(GetBonusHighlight()).transform,
                Instantiate(GetBonusHighlight()).transform
            };
            for (int i = 0; i < resourceHighlights.Count; i++)
            {
                resourceHighlights[i].gameObject.SetActive(false);
            }
            for (int i = 1; i <= 12; i++)
            {
                Vector3 cost = CalculateStructureCost(StructureNames.BuildPanelToString((BuildPanel.Buildings)i));
                panel.GetToolInfo().cost[i] = new Vector2(cost.y, cost.z);
            }
        }

    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (GameManager.GetInstance().GetGameLost())
        {
            return;
        }

        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

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

        if (Input.GetMouseButtonDown(1))
        {
            mpAtRightDown = Input.mousePosition;
        }
    }

    private void OnApplicationQuit()
    {
        SuperManager.GetInstance().SaveCurrentMatch();
    }

    #endregion

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
            // NAME                                                                    NAME                                                                        fC       wC       mC      
            { StructureNames.Longhaus,          new StructureDefinition(Resources.Load("Structures/Longhaus")                   as GameObject,  new ResourceBundle(0,       200,     50)) },

            { StructureNames.Barracks,          new StructureDefinition(Resources.Load("Structures/Defense/Barracks")           as GameObject,  new ResourceBundle(0,       150,     25)) },
            { StructureNames.Ballista,          new StructureDefinition(Resources.Load("Structures/Defense/Ballista Tower")     as GameObject,  new ResourceBundle(0,       200,     125)) },
            { StructureNames.Catapult,          new StructureDefinition(Resources.Load("Structures/Defense/Catapult Tower")     as GameObject,  new ResourceBundle(0,       50,      250)) },
            { StructureNames.FrostTower,        new StructureDefinition(Resources.Load("Structures/Defense/Frost Tower")        as GameObject,  new ResourceBundle(0,       100,     150)) },
            { StructureNames.ShockwaveTower,    new StructureDefinition(Resources.Load("Structures/Defense/Shockwave Tower")    as GameObject,  new ResourceBundle(0,       100,     200)) },
            { StructureNames.LightningTower,    new StructureDefinition(Resources.Load("Structures/Defense/Lightning Tower")    as GameObject,  new ResourceBundle(0,       50,      200)) },


            { StructureNames.FoodResource,      new StructureDefinition(Resources.Load("Structures/Resource/Farm")              as GameObject,  new ResourceBundle(0,       50,      0)) },
            { StructureNames.LumberResource,    new StructureDefinition(Resources.Load("Structures/Resource/Lumber Mill")       as GameObject,  new ResourceBundle(0,       50,      0)) },
            { StructureNames.MetalResource,     new StructureDefinition(Resources.Load("Structures/Resource/Mine")              as GameObject,  new ResourceBundle(0,       100,     0)) },

            { StructureNames.FoodStorage,       new StructureDefinition(Resources.Load("Structures/Storage/Granary")            as GameObject,  new ResourceBundle(0,       100,     0)) },
            { StructureNames.LumberStorage,     new StructureDefinition(Resources.Load("Structures/Storage/Lumber Pile")        as GameObject,  new ResourceBundle(0,       120,     40)) },
            { StructureNames.MetalStorage,      new StructureDefinition(Resources.Load("Structures/Storage/Metal Storage")      as GameObject,  new ResourceBundle(0,       160,     40)) },

            { StructureNames.LumberEnvironment, new StructureDefinition(Resources.Load("Structures/Environment/Forest")         as GameObject,  new ResourceBundle(0,       0,       0)) },
            { StructureNames.MetalEnvironment,  new StructureDefinition(Resources.Load("Structures/Environment/Hills")          as GameObject,  new ResourceBundle(0,       0,       0)) },
            { StructureNames.FoodEnvironment,   new StructureDefinition(Resources.Load("Structures/Environment/Plains")         as GameObject,  new ResourceBundle(0,       0,       0)) },
        };
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
                    highlightpos.y = HighlightSitHeight;
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
                    highlightpos.y = HighlightSitHeight;
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
        /*
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
        */

        Vector3 highlightpos = selectedStructure.transform.position;
        highlightpos.y = HighlightSitHeight;
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
                    highlightpos.y = HighlightSitHeight;
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
                        highlightpos.y = HighlightSitHeight;
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
        if (selectedTileHighlight.gameObject.activeSelf) 
        { 
            selectedTileHighlight.gameObject.SetActive(false); 
        }
        if (Physics.Raycast(_mouseRay.origin, _mouseRay.direction, out RaycastHit hitGround, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            TileBehaviour tile = hitGround.transform.GetComponent<TileBehaviour>();
            bool canPlaceHere = true;
            Structure attached = tile.GetAttached();
            StructureType newStructureType = structure.GetStructureType();
            if (tile.GetPlayable())
            {
                // If the tile we hit has an attached object...
                if (attached)
                {
                    canPlaceHere = false;
                    if (attached.GetStructureType() == StructureType.Environment)
                    {
                        canPlaceHere = true;
                    }
                }
            }
            else
            {
                canPlaceHere = false;
            }
            // if the structure can be placed here...
            if (canPlaceHere)
            {
                if (newStructureType == StructureType.Defense)
                {
                    structure.ShowRangeDisplay(true);
                }
                else if (newStructureType == StructureType.Resource)
                {
                    SetPreview(tile);
                }

                if (attached)
                {
                    EnvironmentStructure environment = attached.GetComponent<EnvironmentStructure>();
                    if (environment)
                    {
                        string attachedName = attached.GetStructureName();
                        string structureName = structure.GetStructureName();
                        // determine if the structure is in synergy with attached structure
                        bool resourceGain = (attachedName == StructureNames.FoodEnvironment && structureName == StructureNames.FoodResource)
                            || (attachedName == StructureNames.LumberEnvironment && structureName == StructureNames.LumberResource)
                            || (attachedName == StructureNames.MetalEnvironment && structureName == StructureNames.MetalResource);
                        if (resourceGain)
                        {
                            SetStructureColour(Color.Lerp(Color.white, Color.green, ColourLerpAmount));
                        }
                        else
                        {
                            SetStructureColour(Color.Lerp(Color.white, Color.yellow, ColourLerpAmount));
                        }
                        if (!environment.GetExploited())
                        {
                            if (hoverEnvironment)
                            {
                                if (hoverEnvironment != environment)
                                {
                                    ResetEnvironmentTransparency();
                                }
                            }
                            hoverEnvironment = environment;
                            UpdateEnvironmentTransparency();
                        }
                        else
                        {
                            ResetEnvironmentTransparency();
                        }
                    }
                }
                else // the tile can be placed on, and has no attached structure
                {
                    ResetEnvironmentTransparency();
                    if (structure.GetStructureType() == StructureType.Resource)
                    {
                        SetStructureColour(Color.Lerp(Color.white, Color.yellow, ColourLerpAmount));
                    }
                    else
                    {
                        SetStructureColour(Color.Lerp(Color.white, Color.green, ColourLerpAmount));
                    }
                }

                // If player cannot afford the structure, set to red.
                if (!GameManager.GetInstance().playerResources.CanAfford(structureCosts[structure.GetStructureName()]))
                {
                    SetStructureColour(Color.Lerp(Color.white, Color.red, ColourLerpAmount));
                }

                Vector3 structPos = hitGround.transform.position;
                structPos.y = structure.sitHeight;
                structure.transform.position = structPos;

                Vector3 highlightPos = structPos;
                highlightPos.y = HighlightSitHeight;
                tileHighlight.position = highlightPos;
                selectedTileHighlight.position = highlightPos;

                tileHighlight.gameObject.SetActive(true);

                // If the user clicked the LMB...
                if (Input.GetMouseButtonDown(0))
                {
                    AttemptPlaceStructure(tile);
                }
            }
            else
            {
                Vector3 hitPos = hitGround.point;
                hitPos.y = structure.sitHeight;
                structure.transform.position = hitPos;

                SetStructureColour(Color.Lerp(Color.white, Color.red, ColourLerpAmount));

                if (tileHighlight.gameObject.activeSelf) { tileHighlight.gameObject.SetActive(false); }
                if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }

                ResetEnvironmentTransparency();
                TurnOffPreview();
            }
        }
        else
        {
            tileHighlight.gameObject.SetActive(false);
            ResetEnvironmentTransparency();
            TurnOffPreview();
            HideBuilding();
        }
        if (Input.GetMouseButtonUp(1))
        {
            if ((Input.mousePosition - mpAtRightDown).magnitude < 20)
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
    }

    public void AttemptPlaceStructure(TileBehaviour _tile)
    {
        GameManager gameMan = GameManager.GetInstance();
        Structure attached = _tile.GetAttached();
        StructureType structType = structure.GetStructureType();
        if ((structureFromStore && BuyBuilding()) || !structureFromStore)
        {
            InfoManager.RecordNewAction();
            InfoManager.RecordNewStructurePlaced();
            GameManager.CreateAudioEffect("build", structure.transform.position, SoundType.SoundEffect, 0.6f);
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
                    gameMan.playerResources.AddResourceBundle(new ResourceBundle(50f, 0f, 0f));
                    structure.GetComponent<Farm>().wasPlacedOnPlains = true;
                }
                else if (attachedName == StructureNames.LumberEnvironment && structName == StructureNames.LumberResource)
                {
                    gameMan.playerResources.AddResourceBundle(new ResourceBundle(0f, 50f, 0f));
                    structure.GetComponent<LumberMill>().wasPlacedOnForest = true;
                }
                else if (attachedName == StructureNames.MetalEnvironment && structName == StructureNames.MetalResource)
                {
                    gameMan.playerResources.AddResourceBundle(new ResourceBundle(0f, 0f, 50f));
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
            if (structure.IsStructure(StructureNames.FrostTower))
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
            gameMan.OnStructurePlaced();
            PathManager.GetInstance().ClearPaths();
            VillagerManager.GetInstance().RedistributeVillagers();
            SelectStructure(structure);
            if (villWidget)
            {
                structure.RefreshWidget();
                structure.SetWidgetVisibility(true);
                if (structType == StructureType.Defense)
                {
                    structure.ManuallyAllocate(0);
                }
            }
            CalculateAverageTileBonus();
            TurnOffPreview();
            NotifyTutorial(structure.GetStructureName(), true);
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
        ResourceBundle result = new ResourceBundle(0, cost, cost);
        return result;
    }

    public bool BuyBuilding()
    {
        if (structure && structureFromStore)
        {
            ResourceBundle cost = structureCosts[structure.GetStructureName()];
            if (GameManager.GetInstance().playerResources.AttemptPurchase(cost))
            {
                InfoManager.RecordResourcesSpent(cost);
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
            Vector3 cost = CalculateStructureCost(_structureName);
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = new Vector2(cost.y, cost.z);
        }
    }

    public void DecreaseStructureCost(string _structureName)
    {
        if (structureCosts.ContainsKey(_structureName))
        {
            structureCounts[StructureIDs[_structureName]]--;
            Vector3 cost = CalculateStructureCost(_structureName);
            panel.GetToolInfo().cost[(int)StructureIDs[_structureName]] = new Vector2(cost.y, cost.z);
        }
    }

    private Vector3 CalculateStructureCost(string _structureName)
    {
        float increaseCoefficient = SuperManager.GetInstance().CurrentLevelHasModifier(SuperManager.SnoballPrices) ? 4f : 10f;
        Vector3 newCost = (increaseCoefficient + structureCounts[StructureIDs[_structureName]]) / increaseCoefficient * (Vector3)structureDict[_structureName].originalCost;
        structureCosts[_structureName] = new ResourceBundle(newCost);
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
            TurnOffPreview();
            ResetEnvironmentTransparency();
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
        highlightpos.y = HighlightSitHeight;
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
            GameObject structureInstance = Instantiate(structureDict[_building].structurePrefab, GetNewStructuresParent().transform);
            structureInstance.transform.position = Vector3.down * 10f;
            structure = structureInstance.GetComponent<Structure>();
            if (selectedTileHighlight.gameObject.activeSelf) { selectedTileHighlight.gameObject.SetActive(false); }
            selectedStructure = null;
            buildingInfo.showPanel = false;
            NotifyTutorial(structure.GetStructureName(), false);
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
        List<TileBehaviour> PGStartingTiles = new List<TileBehaviour>();

        Vector3 longhausTilePos = FindObjectOfType<Longhaus>().attachedTile.transform.position;
        float startingAreaRadius = 6f;

        for (int i = 0; i < tiles.Length; i++)
        {
            if (Application.isEditor)
            {
                tiles[i].DetectStructure(); 
            }
            // if the tile is playable and it doesn't have a structure already
            if (tiles[i].GetPlayable() && tiles[i].GetAttached() == null)
            {
                PGPlayableTiles.Add(tiles[i]);
                // if the tile is also within currentRadius units from the longhaus (if the tile is within the starting area)
                if ((tiles[i].transform.position - longhausTilePos).magnitude <= startingAreaRadius)
                {
                    PGStartingTiles.Add(tiles[i]);
                }
            }
        }

        // in a small radius around the Longhaus, make sure that a certain quota of fields, forests and hills have been generated.
        // 12 hills, then
        // 18 forests, then
        // 24 fields
        // STARTING AREA
        int hillsPlaced = 0;
        int forestPlaced = 0;
        int plainsPlaced = 0;
        bool continueStartingAreaGeneration = true;
        while (continueStartingAreaGeneration)
        {
            if (hillsPlaced < HillsInStartingArea && PGStartingTiles.Count > 0)
            {
                TileBehaviour tile = PGStartingTiles[UnityEngine.Random.Range(0, PGStartingTiles.Count)];
                List<TileBehaviour> toBeRemoved = PGRecursiveWander(StructureNames.MetalEnvironment, tile, ref hillsPlaced, HillsInStartingArea, recursiveHGrowthChance);
                foreach (TileBehaviour removalTarget in toBeRemoved)
                {
                    if (PGStartingTiles.Contains(removalTarget))
                    {
                        PGStartingTiles.Remove(removalTarget);
                    }
                }
            }

            if (forestPlaced < ForestsInStartingArea && PGStartingTiles.Count > 0)
            {
                TileBehaviour tile = PGStartingTiles[UnityEngine.Random.Range(0, PGStartingTiles.Count)];
                List<TileBehaviour> toBeRemoved = PGRecursiveWander(StructureNames.LumberEnvironment, tile, ref forestPlaced, ForestsInStartingArea, recursiveFGrowthChance);
                foreach (TileBehaviour removalTarget in toBeRemoved)
                {
                    if (PGStartingTiles.Contains(removalTarget))
                    {
                        PGStartingTiles.Remove(removalTarget);
                    }
                }
            }

            if (plainsPlaced < PlainsInStartingArea && PGStartingTiles.Count > 0)
            {
                TileBehaviour tile = PGStartingTiles[UnityEngine.Random.Range(0, PGStartingTiles.Count)];
                List<TileBehaviour> toBeRemoved = PGRecursiveWander(StructureNames.FoodEnvironment, tile, ref plainsPlaced, PlainsInStartingArea, recursivePGrowthChance);
                foreach (TileBehaviour removalTarget in toBeRemoved)
                {
                    if (PGStartingTiles.Contains(removalTarget))
                    {
                        PGStartingTiles.Remove(removalTarget);
                    }
                }
            }

            // if we've finished placing all the tiles
            if (hillsPlaced == HillsInStartingArea && forestPlaced == ForestsInStartingArea && plainsPlaced == PlainsInStartingArea)
            {
                break;
            }

            // if we're out of starting tiles
            if (PGStartingTiles.Count == 0)
            {
                startingAreaRadius += 3f;
                foreach (TileBehaviour tile in PGPlayableTiles)
                {
                    if ((tile.transform.position - longhausTilePos).magnitude <= startingAreaRadius)
                    {
                        PGStartingTiles.Add(tile);
                    }
                }
            }
        }


        // REMAINING GENERATION

        while (hillsPlaced < hillsTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander(StructureNames.MetalEnvironment, tile, ref hillsPlaced, hillsTotal, recursiveHGrowthChance);
        }

        while (forestPlaced < forestTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander(StructureNames.LumberEnvironment, tile, ref forestPlaced, forestTotal, recursiveFGrowthChance);
        }

        while (plainsPlaced < plainsTotal)
        {
            TileBehaviour tile = PGPlayableTiles[UnityEngine.Random.Range(0, PGPlayableTiles.Count)];
            PGRecursiveWander(StructureNames.FoodEnvironment, tile, ref plainsPlaced, plainsTotal, recursivePGrowthChance);
        }
    }

    private void SetStructureColour(Color _colour)
    {
        structure.SetColour(_colour);
    }

    private List<TileBehaviour> PGRecursiveWander(string _environmentType, TileBehaviour _tile, ref int _placed, int _max, float _recursiveChance)
    {
        List<TileBehaviour> removedTiles = new List<TileBehaviour>();
        if (_placed == _max)
        {
            return removedTiles;
        }
        // plant the environment on the tile,
        // remove the tile from PGPlayableTiles
        // for each face of the tile, if that face is in PGPlayableTiles, roll the dice on PGRecursiveWander
        if (PGPlayableTiles.Contains(_tile))
        {
            _placed++;
            PGPlayableTiles.Remove(_tile);
            PGInstatiateEnvironment(_environmentType, _tile);
            removedTiles.Add(_tile);
        }

        Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacentsToTile = _tile.GetAdjacentTiles();
        // now try the tiles around it
        for (int i = 0; i < 4; i++)
        {
            if (_placed == _max) { break; }

            if (adjacentsToTile.ContainsKey((TileBehaviour.TileCode)i))
            {
                TileBehaviour tileI = adjacentsToTile[(TileBehaviour.TileCode)i];
                if (PGPlayableTiles.Contains(tileI))
                {
                    if (UnityEngine.Random.Range(0f, 1f) <= _recursiveChance)
                    {
                        removedTiles.AddRange(PGRecursiveWander(_environmentType, tileI, ref _placed, _max, _recursiveChance));
                    }
                }
            }
        }

        return removedTiles;
    }

    private void PGInstatiateEnvironment(string _environmentType, TileBehaviour _tile)
    {
        // define the structures when in editor mode
        if (!Application.isPlaying)
        {
            if (structureDict == null) { DefineDictionaries(); }
        }
        // create the structure
        Structure structure = Instantiate(structureDict[_environmentType].structurePrefab, GetProcGenParent().transform).GetComponent<Structure>();
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
            structure.transform.position = Vector3.down * 1000f;
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
        messageBox.ShowMessage(_message, _duration);
    }

    public void LoadBuilding(SuperManager.StructureSaveData _saveData)
    {
        Structure newStructure = Instantiate(structureDict[_saveData.structure].structurePrefab, GetLoadedStructuresParent().transform).GetComponent<Structure>();
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
            if (newStructure.IsStructure(StructureNames.FrostTower))
            {
                villWidget = false;
            }
            if (villWidget)
            {
                VillagerAllocation villagerAllocation = Instantiate(villagerWidgetPrefab, canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
                villagerAllocation.SetTarget(newStructure);
                newStructure.SetAllocationWidget(villagerAllocation);
                newStructure.SetAllocated(_saveData.villagers);
                newStructure.SetManualAllocation(_saveData.manualAllocation);
            }
        }
    }

    private static bool FindTileAtXZ(float _x, float _z, out TileBehaviour _tile)
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

    public static GameObject GetTileHighlight()
    {
        if (!TileHighlight)
        {
            TileHighlight = Resources.Load("TileHighlight") as GameObject;
        }
        return TileHighlight;
    }

    public static GameObject GetBonusHighlight()
    {
        if (!BonusHighlight)
        {
            BonusHighlight = Resources.Load("BonusHighlight") as GameObject;
        }
        return BonusHighlight;
    }

    private void SetPreview(TileBehaviour _hitTile)
    {
        ResourceStructure resStruct = structure.GetComponent<ResourceStructure>();
        // if the structure is a ResourceStructure...
        if (resStruct)
        {
            ResourceType resourceType = resStruct.GetResourceType();
            Structure attachedToTile = _hitTile.GetAttached();
            resourceHighlights[4].gameObject.SetActive(false);
            if (attachedToTile)
            {
                if (attachedToTile.GetStructureType() == StructureType.Environment)
                {
                    EnvironmentStructure environmentAttachedToTile = attachedToTile.GetComponent<EnvironmentStructure>();
                    if (environmentAttachedToTile.GetResourceType() == resStruct.GetResourceType())
                    {
                        Vector3 highlightPos = _hitTile.transform.position;
                        highlightPos.y = BonusHighlightSitHeight;
                        resourceHighlights[4].position = highlightPos;
                        // enable it
                        resourceHighlights[4].gameObject.SetActive(true);
                        SuperManager.SetBonusHighlightHeight(resourceHighlights[4], environmentAttachedToTile.GetBonusHighlightSitHeight());
                    }
                }
            }

            Dictionary<TileBehaviour.TileCode, TileBehaviour> adjacents = _hitTile.GetAdjacentTiles();
            
            // for each tilecode
            for (int i = 0; i < 4; i++)
            {
                resourceHighlights[i].gameObject.SetActive(false);
                TileBehaviour.TileCode tileCode = (TileBehaviour.TileCode)i;

                // if _hitTile has a tile in that direction...
                if (adjacents.ContainsKey(tileCode))
                {
                    // set it to the right colour
                    Structure attached = adjacents[tileCode].GetAttached();
                    if (attached)
                    {
                        EnvironmentStructure attachedEnvironment = attached.GetComponent<EnvironmentStructure>();
                        if (attachedEnvironment)
                        {
                            // if the ResourceType of the structure being placed and the environment match
                            if (resourceType == attachedEnvironment.GetResourceType())
                            {
                                if (!attachedEnvironment.GetExploited())
                                {
                                    // enable it
                                    resourceHighlights[i].gameObject.SetActive(true);
                                    // move the corresponding tilehighligh to the tile's position
                                    Vector3 highlightPos = adjacents[tileCode].transform.position;
                                    highlightPos.y = BonusHighlightSitHeight;
                                    resourceHighlights[i].position = highlightPos;
                                    // set the shader height value
                                    SuperManager.SetBonusHighlightHeight(resourceHighlights[i], attachedEnvironment.GetBonusHighlightSitHeight());
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void TurnOffPreview()
    {
        // turn off all the resourceHighlights
        foreach (Transform highlight in resourceHighlights)
        {
            if (highlight.gameObject.activeSelf)
            {
                highlight.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateEnvironmentTransparency()
    {
        if (hoverEnvironment)
        {
            if (opacityAscending)
            {
                opacity += Time.deltaTime * OpacitySpeed;
                if (opacity > OpacityMaximum)
                {
                    opacity = OpacityMaximum;
                    opacityAscending = !opacityAscending;
                }
            }
            else
            {
                opacity -= Time.deltaTime * OpacitySpeed;
                if (opacity < OpacityMinimum)
                {
                    opacity = OpacityMinimum;
                    opacityAscending = !opacityAscending;
                }
            }
            hoverEnvironment.SetOpacity(opacity);
        }
        else
        {
            opacity = OpacityMaximum;
            opacityAscending = false;
        }
    }

    private void ResetEnvironmentTransparency()
    {
        if (hoverEnvironment)
        {
            hoverEnvironment.SetOpacity(1.0f);
            hoverEnvironment = null;
        }
    }

    public static GameObject GetProcGenParent()
    {
        if (!ProceduralGenerationParent)
        {
            ProceduralGenerationParent = new GameObject("Procedural Generation");
        }
        return ProceduralGenerationParent;
    }

    public static GameObject GetLoadedStructuresParent()
    {
        if (!LoadedStructuresParent)
        {
            LoadedStructuresParent = new GameObject("Loaded Structures");
        }
        return LoadedStructuresParent;
    }

    public static GameObject GetNewStructuresParent()
    {
        if (!NewStructuresParent)
        {
            NewStructuresParent = new GameObject("Purchased Structures");
        }
        return NewStructuresParent;
    }

    private void CalculateAverageTileBonus()
    {
        int runningTotal = 0;
        int count = 0;
        foreach (Structure structure in playerStructureDict.Values)
        {
            if (structure.GetStructureType() == StructureType.Resource)
            {
                ResourceStructure resourceStructure = structure.GetComponent<ResourceStructure>();
                runningTotal += resourceStructure.GetTileBonus();
                count++;
            }
        }
        if (count != 0)
        {
            InfoManager.RecordTileBonusAverage(runningTotal / (float)count);
        }
    }

    private void NotifyTutorial(string _structureName, bool _structureWasPlaced)
    {
        TutorialManager tutMan = TutorialManager.GetInstance();
        switch (_structureName)
        {
            case StructureNames.FoodResource:
                if (_structureWasPlaced)
                {
                    if (tutMan.State == TutorialManager.TutorialState.PlaceFarm)
                    {
                        tutMan.GoToNext();
                    }
                }
                else
                {
                    if (tutMan.State == TutorialManager.TutorialState.SelectFarm)
                    {
                        tutMan.GoToNext();
                    }
                }
                break;
            case StructureNames.LumberResource:
                if (_structureWasPlaced)
                {
                    if (tutMan.State == TutorialManager.TutorialState.PlaceLumberMill)
                    {
                        tutMan.GoToNext();
                    }
                }
                break;
            case StructureNames.MetalResource:
                if (_structureWasPlaced)
                {
                    if (tutMan.State == TutorialManager.TutorialState.PlaceMine)
                    {
                        tutMan.GoToNext();
                    }
                }
                break;
            case StructureNames.FoodStorage:
            case StructureNames.LumberStorage:
            case StructureNames.MetalStorage:
                if (_structureWasPlaced)
                {
                    if (tutMan.State == TutorialManager.TutorialState.Storages)
                    {
                        tutMan.GoToNext();
                    }
                }
                break;
            case StructureNames.Ballista:
            case StructureNames.Barracks:
            case StructureNames.Catapult:
            case StructureNames.LightningTower:
            case StructureNames.ShockwaveTower:
            case StructureNames.FrostTower:
                if (_structureWasPlaced)
                {
                    if (tutMan.State == TutorialManager.TutorialState.SelectDefence)
                    {
                        tutMan.GoToNext();
                    }
                }
                break;
            default:
                break;
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
