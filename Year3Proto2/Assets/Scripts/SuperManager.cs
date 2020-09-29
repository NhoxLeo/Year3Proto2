using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SuperManager : MonoBehaviour
{
    // AUDIO
    public static float AmbientVolume = 1.0f;
    public static float MusicVolume = 1.0f;
    public static float EffectsVolume = 1.0f;

    // CONSTANTS
    public static bool DevMode = true;
    public static float CameraSensitivity = 4.0f;

    public const float ScalingFactor = 1.33f;
    public const float PoorTimberFactor = 0.75f;
    public static bool ShowPriortyPanel = false;
    public static bool ShowVillagerWidgets = false;


    public const int NoRequirement = -1;

    // Modifiers
    public const int SnoballPrices = 0;
    public const int SwiftFootwork = 1;
    public const int DryFields = 2;
    public const int PoorTimber = 3;

    // Win Conditions
    public const int Accumulate = 0;
    public const int AccumulateII = 1;
    public const int AccumulateIII = 2;

    public const int Slaughter = 3;
    public const int SlaughterII = 4;
    public const int SlaughterIII = 5;

    public const int Survive = 6;
    public const int SurviveII = 7;
    public const int SurviveIII = 8;

    public const int Food = 9;
    public const int FoodII = 10;
    public const int FoodIII = 11;

    public const int Lumber = 12;
    public const int LumberII = 13;
    public const int LumberIII = 14;

    public const int Metal = 15;
    public const int MetalII = 16;
    public const int MetalIII = 17;

    public const int Villagers = 18;
    public const int VillagersII = 19;
    public const int VillagersIII = 20;

    // BALLISTA
    public const int Ballista = 0;
    public const int BallistaRange = 1;
    public const int BallistaPower = 2;
    public const int BallistaFortification = 3;
    public const int BallistaEfficiency = 4;
    public const int BallistaSuper = 5;

    // CATAPULT
    public const int Catapult = 6;
    public const int CatapultRange = 7;
    public const int CatapultPower = 8;
    public const int CatapultFortification = 9;
    public const int CatapultEfficiency = 10;
    public const int CatapultSuper = 11;

    // BARRACKS
    public const int Barracks = 12;
    public const int BarracksSoldierDamage = 13;
    public const int BarracksSoldierHealth = 14;
    public const int BarracksSoldierSpeed = 15;
    public const int BarracksFortification = 16;
    public const int BarracksSuper = 17;

    // FREEZE TOWER
    public const int FreezeTower = 18;
    public const int FreezeTowerRange = 19;
    public const int FreezeTowerSlowEffect = 20;
    public const int FreezeTowerFortification = 21;
    public const int FreezeTowerEfficiency = 22;
    public const int FreezeTowerSuper = 23;

    // LIGHTNING TOWER
    public const int LightningTower = 24;
    public const int LightningTowerRange = 25;
    public const int LightningTowerPower = 26;
    public const int LightningTowerFortification = 27;
    public const int LightningTowerEfficiency = 28;
    public const int LightningTowerSuper = 29;

    // SHOCKWAVE TOWER
    public const int ShockwaveTower = 30;
    public const int ShockwaveTowerRange = 31;
    public const int ShockwaveTowerStunDuration = 32;
    public const int ShockwaveTowerFortification = 33;
    public const int ShockwaveTowerEfficiency = 34;
    public const int ShockwaveTowerSuper = 35;

    [Serializable]
    public struct SaveQuaternion
    {
        public float w;
        public float x;
        public float y;
        public float z;

        public SaveQuaternion(Quaternion _quat)
        {
            w = _quat.w;
            x = _quat.x;
            y = _quat.y;
            z = _quat.z;
        }

        public static implicit operator Quaternion(SaveQuaternion _saveQuat)
        {
            return new Quaternion(_saveQuat.x, _saveQuat.y, _saveQuat.z, _saveQuat.w);
        }
    }

    [Serializable]
    public struct SaveVector3
    {
        public float x;
        public float y;
        public float z;

        public SaveVector3(Vector2 _vec)
        {
            x = _vec.x;
            y = _vec.y;
            z = 0f;
        }

        public SaveVector3(Vector3 _vec)
        {
            x = _vec.x;
            y = _vec.y;
            z = _vec.z;
        }

        public SaveVector3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public static implicit operator Vector3(SaveVector3 _saveVec3)
        {
            return new Vector3(_saveVec3.x, _saveVec3.y, _saveVec3.z);
        }

        public static implicit operator Vector2(SaveVector3 _saveVec3)
        {
            return new Vector2(_saveVec3.x, _saveVec3.y);
        }

        public static implicit operator Vector2Int(SaveVector3 _saveVec3)
        {
            return new Vector2Int((int)_saveVec3.x, (int)_saveVec3.y);
        }
    }

    [Serializable]
    public struct EnemySaveData
    {
        public float health;
        public SaveVector3 position;
        public SaveQuaternion orientation;
        public SaveVector3 targetPosition;
        public EnemyState state;
        public int enemyWave;
        public int level;
    }

    [Serializable]
    public struct SoldierSaveData
    {
        public float health;
        public SaveVector3 position;
        public SaveQuaternion orientation;
        public int barracksID;
        public int state;
        public bool returnHome;
    }

    [Serializable]
    public struct InvaderSaveData
    {
        public EnemySaveData enemyData;
        public float scale;
    }

    [Serializable]
    public struct HeavyInvaderSaveData
    {
        public EnemySaveData enemyData;
        public bool[] equipment;
    }

    [Serializable]
    public struct StructureSaveData
    {
        // all structures
        public string structure;
        public int ID;
        public float health;
        public StructureType type;
        public SaveVector3 position;
        public int villagers;
        public bool manualAllocation;

        // resource structures
        public bool wasPlacedOn;

        // environment structures
        public bool exploited;
        public int exploiterID;

        // defense structures
        public int level;
    }

    [Serializable]
    public struct MatchSaveData
    {
        public bool match;
        public int levelID;
        public int objectivesCompleted;
        public bool matchWon;
        public PlayerResources playerResources;
        public Dictionary<string, ResourceBundle> structureCosts;
        public Dictionary<BuildPanel.Buildings, int> structureCounts;
        public List<StructureSaveData> structures;
        public List<InvaderSaveData> invaders;
        public List<HeavyInvaderSaveData> heavyInvaders;
        public List<EnemySaveData> flyingInvaders;
        public List<EnemySaveData> petards;
        public List<EnemySaveData> rams;
        public List<SoldierSaveData> soldiers;
        public int enemiesKilled;
        public float spawnTime;
        public bool spawning;
        public float weightageScalar;
        public float tokenIncrement;
        public float tokenScalar;
        public float time;
        public SaveVector3 timeVariance;
        public float tokens;
        public int wave;
        public bool tutorialDone;
        public bool repairMessage;
        public bool repairAll;
        public int nextStructureID;
        public int villagers;
        public int availableVillagers;
        public int manuallyAllocated;
        public float starveTicks;
        public float tempFood;
        public float tempLumber;
        public float tempMetal;
        public float longhausHealth;
    }

    [Serializable]
    public struct GameSaveData
    {
        public Dictionary<int, bool> research;
        public Dictionary<int, bool> levelCompletion;
        public int researchPoints;
        public MatchSaveData currentMatch;
        public bool showTutorial;
        public string gameVersion;
        public bool showWidgets;
        public bool showPriority;
    }

    [Serializable]
    public struct ResearchElementDefinition
    {
        public int ID;
        public int reqID;
        public string name;
        public string description;
        public int price;
        public bool isSpecialUpgrade;

        public ResearchElementDefinition(int _id, int _reqID, string _name, string _description, int _price, bool _isSpecial = false)
        {
            ID = _id;
            reqID = _reqID;
            name = _name;
            description = _description;
            price = _price;
            isSpecialUpgrade = _isSpecial;
        }
    }

    [Serializable]
    public struct LevelDefinition
    {
        public int ID;
        public int reqID;
        public List<int> objectives;
        public List<int> modifiers;
        public int reward;

        public LevelDefinition(int _id, int _reqID, List<int> _objectives, List<int> _modifiers, int _reward)
        {
            ID = _id;
            reqID = _reqID;
            modifiers = _modifiers;
            objectives = _objectives;
            reward = _reward;
        }
    }

    [Serializable]
    public struct ModifierDefinition
    {
        public int ID;
        public string name;
        public string description;
        public float coefficient;

        public ModifierDefinition(int _ID, string _name, string _description, float _coefficient)
        {
            ID = _ID;
            name = _name;
            description = _description;
            coefficient = _coefficient;
        }
    }

    [Serializable]
    public struct WinConditionDefinition
    {
        public int ID;
        public string name;
        public string description;

        public WinConditionDefinition(int _ID, string _name, string _description)
        {
            ID = _ID;
            name = _name;
            description = _description;
        }
    }

    private static SuperManager instance = null;
    private GameSaveData saveData;
    public static List<ResearchElementDefinition> ResearchDefinitions = new List<ResearchElementDefinition>()
    {
        // ID, ID requirement, Name, Description, RP Cost, Special Upgrade (false by default)
        new ResearchElementDefinition(Ballista, NoRequirement, "Ballista Tower", "The Ballista Tower is great for single target damage, firing bolts at deadly speeds.", 0),
        new ResearchElementDefinition(BallistaRange, Ballista, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(BallistaPower, Ballista, "Power Shot", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(BallistaFortification, Ballista, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(BallistaEfficiency, Ballista, "Efficiency", "Bolt cost reduced by 50%.", 200),
        new ResearchElementDefinition(BallistaSuper, Ballista, "Piercing Shot", "Bolts rip right through their targets.", 500, true),

        new ResearchElementDefinition(Catapult, NoRequirement, "Catapult Tower", "The Catapult Tower deals splash damage, making it the ideal choice for crowd control.", 300),
        new ResearchElementDefinition(CatapultRange, Catapult, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(CatapultPower, Catapult, "Power Shot", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(CatapultFortification, Catapult, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(CatapultEfficiency, Catapult, "Efficiency", "Boulder cost reduced by 50%.", 200),
        new ResearchElementDefinition(CatapultSuper, Catapult, "Big Shockwave", "Boulders have a 50% larger damage radius.", 500, true),

        new ResearchElementDefinition(Barracks, NoRequirement, "Barracks", "The Barracks spawns soldiers, which automatically chase down enemies.", 300),
        new ResearchElementDefinition(BarracksSoldierDamage, Barracks, "Soldier Damage", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(BarracksSoldierHealth, Barracks, "Soldier Health", "Health increased by 50%.", 200),
        new ResearchElementDefinition(BarracksSoldierSpeed, Barracks, "Soldier Speed", "Speed increased by 30%.", 200),
        new ResearchElementDefinition(BarracksFortification, Barracks, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(BarracksSuper, Barracks, "Rapid Courses", "Barracks spawn & heal soldiers faster.", 500, true),

        new ResearchElementDefinition(FreezeTower, NoRequirement, "Freeze Tower", "The Freeze Tower slows down enemies making it easier for other defenses to hit them.", 300),
        new ResearchElementDefinition(FreezeTowerRange, FreezeTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(FreezeTowerSlowEffect, FreezeTower, "Slow Effect", "Slows Enemies by +30%.", 200),
        new ResearchElementDefinition(FreezeTowerFortification, FreezeTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(FreezeTowerEfficiency, FreezeTower, "Efficiency", "Freezing cost reduced by 50%.", 200),
        new ResearchElementDefinition(FreezeTowerSuper, FreezeTower, "Blizzard", "Frost effect damages enemies.", 500, true),

        new ResearchElementDefinition(LightningTower, NoRequirement, "Lightning Tower", "The Lightning Tower shoots bolts at enemies dealing heavy shock damage.", 300),
        new ResearchElementDefinition(LightningTowerRange, LightningTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(LightningTowerPower, LightningTower, "Power", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(LightningTowerFortification, LightningTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(LightningTowerEfficiency, LightningTower, "Efficiency", "Lightning bolt cost reduced by 50%.", 200),
        new ResearchElementDefinition(LightningTowerSuper, LightningTower, "Thunder Wave", "Sparks deal damage to surrounding enemies.", 500, true),

        new ResearchElementDefinition(ShockwaveTower, NoRequirement, "Shockwave Tower", "The Shockwave Tower releases high energy shockwaves that momentarily stun enemies.", 300),
        new ResearchElementDefinition(ShockwaveTowerRange, ShockwaveTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(ShockwaveTowerStunDuration, ShockwaveTower, "Stun Duration", "Enemy stun duration increased by 25%", 200),
        new ResearchElementDefinition(ShockwaveTowerFortification, ShockwaveTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(ShockwaveTowerEfficiency, ShockwaveTower, "Efficiency", "Shockwave cost reduced by 50%.", 200),
        new ResearchElementDefinition(ShockwaveTowerSuper, ShockwaveTower, "Bulldoze", "Shockwaves deal some damage.", 500, true),
    };
    public static List<LevelDefinition> LevelDefinitions = new List<LevelDefinition>()
    {
        // ID, ID requirement, Win Condition, Modifiers, Base Reward
        new LevelDefinition(0, NoRequirement,   new List<int>(){ Survive, Villagers, FoodII },                          new List<int>(),                                            1000),
        new LevelDefinition(1, 0,               new List<int>(){ Villagers, Accumulate, SlaughterII },                  new List<int>(){ SnoballPrices, SwiftFootwork },            1250),
        new LevelDefinition(2, 1,               new List<int>(){ Slaughter, Lumber, VillagersII, AccumulateII },        new List<int>(){ DryFields, PoorTimber },                   1500),
        new LevelDefinition(3, 2,               new List<int>(){ FoodII, SlaughterIII,  VillagersIII, AccumulateIII },  new List<int>(){ SnoballPrices, DryFields, PoorTimber },    1750)
    };
    public static List<ModifierDefinition> ModDefinitions = new List<ModifierDefinition>()
    { 
        // ID, Name, Description, Coefficient
        new ModifierDefinition(SnoballPrices, "Snowball Prices", "Structure Cost Acceleration hits harder.", 0.5f),
        new ModifierDefinition(SwiftFootwork, "Swift Footwork", "Enemies are 40% faster.", 0.25f),
        new ModifierDefinition(DryFields, "Dry Fields", "Food production is halved.", 0.35f),
        new ModifierDefinition(PoorTimber, "Poor Timber", "Buildings have 75% of their standard durability.", 0.4f),
    };
    public static List<WinConditionDefinition> WinConditionDefinitions = new List<WinConditionDefinition>()
    { 
        // ID, Name, Description
        new WinConditionDefinition(Accumulate, "Accumulate", "Have 1500 of each resource."),
        new WinConditionDefinition(AccumulateII, "Accumulate II", "Have 2500 of each resource."),
        new WinConditionDefinition(AccumulateIII, "Accumulate III", "Have 5000 of each resource."),

        new WinConditionDefinition(Slaughter, "Slaughter", "Kill 20 Enemies."),
        new WinConditionDefinition(SlaughterII, "Slaughter II", "Kill 50 Enemies."),
        new WinConditionDefinition(SlaughterIII, "Slaughter III", "Kill 100 Enemies."),

        new WinConditionDefinition(Survive, "Survive", "Defend against 5 waves."),
        new WinConditionDefinition(SurviveII, "Survive II", "Defend against 10 waves."),
        new WinConditionDefinition(SurviveIII, "Survive III", "Defend against 15 waves."),

        new WinConditionDefinition(Food, "Food", "Collect a total of 1000 food."),
        new WinConditionDefinition(FoodII, "Food II", "Collect a total of 2000 food."),
        new WinConditionDefinition(FoodIII, "Food III", "Collect a total of 3000 food."),

        new WinConditionDefinition(Lumber, "Lumber", "Collect a total of 1000 lumber."),
        new WinConditionDefinition(LumberII, "Lumber II", "Collect a total of 2000 lumber."),
        new WinConditionDefinition(LumberIII, "Lumber III", "Collect a total of 3000 lumber."),

        new WinConditionDefinition(Metal, "Metal", "Collect a total of 1000 metal."),
        new WinConditionDefinition(MetalII, "Metal II", "Collect a total of 2000 metal."),
        new WinConditionDefinition(MetalIII, "Metal III", "Collect a total of 3000 metal."),

        new WinConditionDefinition(Villagers, "Villagers", "Train a total of 15 villagers."),
        new WinConditionDefinition(VillagersII, "Villagers II", "Train a total of 35 villagers."),
        new WinConditionDefinition(VillagersIII, "Villagers III", "Train a total of 65 villagers."),
    };
    public static Dictionary<int, (Vector4, Vector2)> CameraSettings = new Dictionary<int, (Vector4, Vector2)>()
    {
        {0, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {1, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {2, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {3, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) }
    };
    private int currentLevel;
    [SerializeField]
    private bool startMaxed;

    public static SuperManager GetInstance()
    {
        return instance;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void PlayLevel(int _level)
    {
        currentLevel = _level;
        if (_level != saveData.currentMatch.levelID) { ClearCurrentMatch(); }
        switch (_level)
        {
            case 0:
                FindObjectOfType<SceneSwitcher>().SceneSwitchLoad("SamDev");
                break;
            case 1:
                FindObjectOfType<SceneSwitcher>().SceneSwitchLoad("Level 2");
                break;
            case 2:
                FindObjectOfType<SceneSwitcher>().SceneSwitchLoad("Level 3");
                break;
            case 3:
                FindObjectOfType<SceneSwitcher>().SceneSwitchLoad("Level 4");
                break;
            default:
                break;
        }
    }

    // populates _levelData with the levels
    public void GetLevelData(ref List<MapScreen.Level> _levelData)
    {
        if (_levelData == null) { _levelData = new List<MapScreen.Level>(); }
        else { _levelData.Clear(); }

        for (int i = 0; i < LevelDefinitions.Count; i++)
        {
            MapScreen.Level newLevelData = new MapScreen.Level
            {
                completed = saveData.levelCompletion[i],
                locked = LevelDefinitions[i].reqID == -1 ? false : !saveData.levelCompletion[LevelDefinitions[i].reqID],
                inProgress = saveData.currentMatch.match && saveData.currentMatch.levelID == i,
                victoryTitle = "Objectives: " + LevelDefinitions[i].objectives.Count.ToString(),
                victoryDescription = "First objective: " + WinConditionDefinitions[LevelDefinitions[i].objectives[0]].description,
                victoryValue = LevelDefinitions[i].reward,
                modifiers = new List<MapScreen.Modifier>()
            };
            GetModifierData(i, ref newLevelData.modifiers);
            // this has to be done after GetModifierData is called
            newLevelData.reward = Mathf.RoundToInt(newLevelData.victoryValue * (1f + newLevelData.GetTotalCoefficient()));
            _levelData.Add(newLevelData);
        }
    }

    public static bool GetModifier(string _modifierName, ref ModifierDefinition _modifierDefinition)
    {
        for (int i = 0; i < ModDefinitions.Count; i++)
        {
            if (ModDefinitions[i].name == _modifierName)
            {
                _modifierDefinition = ModDefinitions[i];
                return true;
            }
        }
        return false;
    }

    // populates _modifierData with the modifiers
    public static void GetModifierData(int levelID, ref List<MapScreen.Modifier> _modifierData)
    {
        if (_modifierData == null) { _modifierData = new List<MapScreen.Modifier>(); }
        else { _modifierData.Clear(); }

        for (int i = 0; i < LevelDefinitions[levelID].modifiers.Count; i++)
        {
            MapScreen.Modifier mod = new MapScreen.Modifier
            {
                title = ModDefinitions[LevelDefinitions[levelID].modifiers[i]].name,
                description = ModDefinitions[LevelDefinitions[levelID].modifiers[i]].description,
                modBonus = ModDefinitions[LevelDefinitions[levelID].modifiers[i]].coefficient
            };
            _modifierData.Add(mod);
        }
    }

    public List<int> GetCurrentWinConditions()
    {
        return LevelDefinitions[currentLevel].objectives;
    }

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = GetComponent<SuperManager>();
        DontDestroyOnLoad(gameObject);
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "Level 2":
                currentLevel = 1;
                break;
            case "Level 3":
                currentLevel = 2;
                break;
            case "Level 4":
                currentLevel = 3;
                break;
            default:
                currentLevel = 0;
                break;
        }
        if (startMaxed) { StartNewGame(false); }
        else { ReadGameData(); }

        if (saveData.gameVersion != Application.version)
        {
            ClearCurrentMatch();
        }

        saveData.gameVersion = Application.version;
    }

    private void Update()
    {
        // Hold both mouse buttons
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            // Press D
            if (Input.GetKeyDown(KeyCode.D))
            {
                WipeReloadScene(false);
            }
            if (DevMode)
            {
                // Press S
                if (Input.GetKeyDown(KeyCode.S))
                {
                    startMaxed = true;
                    WipeReloadScene(true);
                }
            }
        }
    }

    private void WipeReloadScene(bool _override)
    {
        if (File.Exists(StructureManager.GetSaveDataPath()))
        {
            File.Delete(StructureManager.GetSaveDataPath());
        }
        StartNewGame(_override);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public bool LoadCurrentMatch()
    {
        return LoadMatch(saveData.currentMatch);
    }

    public void SaveCurrentMatch()
    {
        saveData.currentMatch = SaveMatch();
        WriteGameData();
    }

    private bool LoadMatch(MatchSaveData _matchData)
    {
        if (_matchData.match == false)
        {
            return false;
        }
        if (currentLevel != _matchData.levelID)
        {
            return false;
        }

        GameManager gameMan = GameManager.GetInstance();
        StructureManager structMan = StructureManager.GetInstance();
        VillagerManager villagerMan = VillagerManager.GetInstance();
        EnemyManager enemyMan = EnemyManager.GetInstance();

        // easy stuff
        enemyMan.LoadData(_matchData);
        gameMan.repairAll = _matchData.repairAll;
        gameMan.repairMessage = _matchData.repairMessage;
        gameMan.tutorialDone = _matchData.tutorialDone;
        gameMan.playerResources = _matchData.playerResources;
        gameMan.gameAlreadyWon = _matchData.matchWon;
        gameMan.objectivesCompleted = _matchData.objectivesCompleted;
        gameMan.foodSinceObjective = _matchData.tempFood;
        gameMan.lumberSinceObjective = _matchData.tempLumber;
        gameMan.metalSinceObjective = _matchData.tempMetal;
        structMan.structureCosts = _matchData.structureCosts;
        structMan.structureCounts = _matchData.structureCounts;
        structMan.SetNextStructureID(_matchData.nextStructureID);
        villagerMan.SetVillagers(_matchData.villagers);
        villagerMan.SetAvailable(_matchData.availableVillagers);
        villagerMan.SetStarveTicks(_matchData.starveTicks);
        villagerMan.SetManuallyAllocated(_matchData.manuallyAllocated);
        // not so easy stuff...

        // structures
        // first, environment structures
        // environment structures are done first so that resource structures can recalculate their tileBonus
        foreach (StructureSaveData saveData in _matchData.structures)
        {
            // check if the structure is environment
            if (saveData.type == StructureType.Environment)
            {
                StructureManager.GetInstance().LoadBuilding(saveData);
            }
        }
        // second, non-environment structures
        foreach (StructureSaveData saveData in _matchData.structures)
        {
            // check if the structure isn't environment
            if (saveData.type != StructureType.Environment)
            {
                StructureManager.GetInstance().LoadBuilding(saveData);
            }
        }

        // invaders
        foreach (InvaderSaveData saveData in _matchData.invaders)
        {
            enemyMan.LoadInvader(saveData);
        }

        // heavies
        foreach (HeavyInvaderSaveData saveData in _matchData.heavyInvaders)
        {
            enemyMan.LoadHeavyInvader(saveData);
        }

        // flying invaders
        foreach (EnemySaveData saveData in _matchData.flyingInvaders)
        {
            enemyMan.LoadFlyingInvader(saveData);
        }

        // petards
        foreach (EnemySaveData saveData in _matchData.petards)
        {
            enemyMan.LoadPetard(saveData);
        }

        // rams
        foreach (EnemySaveData saveData in _matchData.rams)
        {
            enemyMan.LoadRam(saveData);
        }

        // soldiers
        Barracks[] allBarracks = FindObjectsOfType<Barracks>();
        foreach (SoldierSaveData saveData in _matchData.soldiers)
        {
            foreach (Barracks barr in allBarracks)
            {
                if (barr.GetID() == saveData.barracksID)
                {
                    barr.LoadSoldier(saveData);
                    break;
                }
            }
        }

        FindObjectOfType<Longhaus>().SetHealth(_matchData.longhausHealth);

        return true;
    }

    public void ClearCurrentMatch()
    {
        saveData.currentMatch.match = false;
        WriteGameData();
    }

    private MatchSaveData SaveMatch()
    {
        GameManager gameMan = GameManager.GetInstance();
        StructureManager structMan = StructureManager.GetInstance();
        VillagerManager villMan = VillagerManager.GetInstance();

        // define a MatchSaveData with the current game state
        MatchSaveData save = new MatchSaveData
        {
            match = true,
            levelID = currentLevel,
            repairAll = gameMan.repairAll,
            repairMessage = gameMan.repairMessage,
            tutorialDone = gameMan.tutorialDone,
            structureCosts = structMan.structureCosts,
            structureCounts = structMan.structureCounts,
            playerResources = gameMan.playerResources,
            invaders = new List<InvaderSaveData>(),
            heavyInvaders = new List<HeavyInvaderSaveData>(),
            flyingInvaders = new List<EnemySaveData>(),
            petards = new List<EnemySaveData>(),
            rams = new List<EnemySaveData>(),
            soldiers = new List<SoldierSaveData>(),
            structures = new List<StructureSaveData>(),
            matchWon = gameMan.AllObjectivesCompleted() || gameMan.gameAlreadyWon,
            objectivesCompleted = gameMan.objectivesCompleted,
            nextStructureID = structMan.GetNextStructureID(),
            villagers = villMan.GetVillagers(),
            availableVillagers = villMan.GetAvailable(),
            starveTicks = villMan.GetStarveTicks(),
            tempFood = gameMan.foodSinceObjective,
            tempLumber = gameMan.lumberSinceObjective,
            tempMetal = gameMan.metalSinceObjective,
            longhausHealth = FindObjectOfType<Longhaus>().GetHealth(),
            manuallyAllocated = villMan.GetManuallyAllocated()
        };

        EnemyManager.GetInstance().SaveSystemToData(ref save);
        

        // not so easy stuff...
        // invaders
        foreach (Invader invader in FindObjectsOfType<Invader>())
        {
            InvaderSaveData saveData = new InvaderSaveData
            {
                enemyData = new EnemySaveData
                {
                    health = invader.GetHealth(),
                    position = new SaveVector3(invader.transform.position),
                    orientation = new SaveQuaternion(invader.transform.rotation),
                    targetPosition = new SaveVector3(invader.GetTarget().transform.position),
                    state = invader.GetState(),
                    enemyWave = invader.GetSpawnWave(),
                    level = invader.GetLevel()
                },
                scale = invader.scale,
            };
            save.invaders.Add(saveData);
        }

        // heavys
        foreach (HeavyInvader heavy in FindObjectsOfType<HeavyInvader>())
        {
            HeavyInvaderSaveData saveData = new HeavyInvaderSaveData
            {
                enemyData = new EnemySaveData
                {
                    health = heavy.GetHealth(),
                    position = new SaveVector3(heavy.transform.position),
                    orientation = new SaveQuaternion(heavy.transform.rotation),
                    targetPosition = new SaveVector3(heavy.GetTarget().transform.position),
                    state = heavy.GetState(),
                    enemyWave = heavy.GetSpawnWave(),
                    level = heavy.GetLevel()
                },
                equipment = heavy.GetEquipment()
            };
            save.heavyInvaders.Add(saveData);
        }
        
        // flying
        foreach (FlyingInvader flying in FindObjectsOfType<FlyingInvader>())
        {
            EnemySaveData saveData = new EnemySaveData
            {
                health = flying.GetHealth(),
                position = new SaveVector3(flying.transform.position),
                orientation = new SaveQuaternion(flying.transform.rotation),
                targetPosition = new SaveVector3(flying.GetTarget().transform.position),
                state = flying.GetState(),
                enemyWave = flying.GetSpawnWave(),
                level = flying.GetLevel()
            };
            save.flyingInvaders.Add(saveData);
        }

        // petards
        foreach (Petard petard in FindObjectsOfType<Petard>())
        {
            EnemySaveData saveData = new EnemySaveData
            {
                health = petard.GetHealth(),
                position = new SaveVector3(petard.transform.position),
                orientation = new SaveQuaternion(petard.transform.rotation),
                targetPosition = new SaveVector3(petard.GetTarget().transform.position),
                state = petard.GetState(),
                enemyWave = petard.GetSpawnWave(),
                level = petard.GetLevel()
            };
            save.petards.Add(saveData);
        }

        // rams
        foreach (BatteringRam ram in FindObjectsOfType<BatteringRam>())
        {
            EnemySaveData saveData = new EnemySaveData
            {
                health = ram.GetHealth(),
                position = new SaveVector3(ram.transform.position),
                orientation = new SaveQuaternion(ram.transform.rotation),
                targetPosition = new SaveVector3(ram.GetTarget().transform.position),
                state = ram.GetState(),
                enemyWave = ram.GetSpawnWave(),
                level = ram.GetLevel()
            };
            save.rams.Add(saveData);
        }

        // soldiers
        foreach (Soldier soldier in FindObjectsOfType<Soldier>())
        {
            SoldierSaveData saveData = new SoldierSaveData
            {
                health = soldier.GetHealth(),
                position = new SaveVector3(soldier.transform.position),
                orientation = new SaveQuaternion(soldier.transform.rotation),
                barracksID = soldier.GetBarracksID(),
                state = soldier.GetState(),
                returnHome = soldier.GetReturnHome()
            };
            save.soldiers.Add(saveData);
        }

        // structures
        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            // structures placed by the structureManager don't have a parent, and need to be saved
            if (structure.transform.parent == null)
            {
                StructureSaveData saveData = new StructureSaveData
                {
                    structure = structure.GetStructureName(),
                    type = structure.GetStructureType(),
                    position = new SaveVector3(structure.transform.position),
                    villagers = structure.GetAllocated(),
                    health = structure.GetHealth(),
                    ID = structure.GetID(),
                    manualAllocation = structure.GetManualAllocation()
                };
                if (saveData.type == StructureType.Environment)
                {
                    EnvironmentStructure envStructure = structure.gameObject.GetComponent<EnvironmentStructure>();
                    if (envStructure.GetExploited())
                    {
                        saveData.exploited = true;
                        saveData.exploiterID = envStructure.GetExploiterID();
                    }
                }
                if (structure.IsStructure("Farm"))
                {
                    saveData.wasPlacedOn = structure.gameObject.GetComponent<Farm>().wasPlacedOnPlains;
                }
                if (structure.IsStructure("Mine"))
                {
                    saveData.wasPlacedOn = structure.gameObject.GetComponent<Mine>().wasPlacedOnHills;
                }
                if (structure.IsStructure("Lumber Mill"))
                {
                    saveData.wasPlacedOn = structure.gameObject.GetComponent<LumberMill>().wasPlacedOnForest;
                }
                if (structure.GetStructureType() == StructureType.Defense)
                {
                    DefenseStructure defense = structure.GetComponent<DefenseStructure>();
                    saveData.level = defense.GetLevel();
                }
                save.structures.Add(saveData);
            }
        }

        return save;
    }

    public bool GetResearchComplete(int _ID)
    {
        if (saveData.research == null) 
        { 
            RestoreSaveData(); 
        }
        else
        {
            if (!saveData.research.ContainsKey(_ID))
            {
                WipeReloadScene(false);
            }
        }
        return saveData.research[_ID];
    }

    public Dictionary<int, bool> GetResearch()
    {
        if (saveData.research == null) 
        { 
            RestoreSaveData(); 
        }
        return saveData.research;
    }

    public bool GetLevelComplete(int _ID)
    {
        if (saveData.levelCompletion == null) { RestoreSaveData(); }
        return saveData.levelCompletion[_ID];
    }

    public bool CheckData()
    {
        return saveData.research != null && saveData.levelCompletion != null;
    }

    public void RestoreSaveData()
    {
        Debug.LogWarning("RestoreSaveData begins...");
        Debug.Log("Attempting ReadGameData...");
        ReadGameData();
        if (CheckData())
        {
            Debug.Log("CheckData returns true, returning...");
        }
        else
        {
            Debug.LogWarning("CheckData returns false, saveData appears corrupt. Calling WipeReloadScene...");
            WipeReloadScene(false);
        }
        Debug.Log("RestoreSaveData ends...");
    }

    public MatchSaveData GetSavedMatch()
    {
        return saveData.currentMatch;
    }

    public bool CanPlayLevel(int _ID)
    {
        int reqID = LevelDefinitions[_ID].reqID;
        // if the level does not require any levels to be complete
        if (reqID == -1)
        { return true; }
        // if the level does need a level to be completed
        else
        { return GetLevelComplete(reqID); }
    }

    public bool CurrentLevelHasModifier(int _modifierID)
    {
        return LevelDefinitions[currentLevel].modifiers.Contains(_modifierID);
    }

    public int GetResearchPoints()
    {
        return saveData.researchPoints;
    }
    public void AddResearchPoints(int _researchPoints)
    {
        saveData.researchPoints += _researchPoints;
    }

    public void SetResearchPoints(int _newResearchPoints)
    {
        saveData.researchPoints = _newResearchPoints;
    }

    public bool AttemptResearch(int _ID)
    {
        if (!saveData.research[_ID])
        {
            if (ResearchDefinitions[_ID].price <= GetResearchPoints())
            {
                saveData.researchPoints -= ResearchDefinitions[_ID].price;
                saveData.research[_ID] = true;
                WriteGameData();
                return true;
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public void WriteGameData()
    {
        saveData.showPriority = ShowPriortyPanel;
        saveData.showWidgets = ShowVillagerWidgets;
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(StructureManager.GetSaveDataPath()))
        {
            File.Delete(StructureManager.GetSaveDataPath());
        }
        FileStream file = File.Create(StructureManager.GetSaveDataPath());

        bf.Serialize(file, saveData);

        file.Close();
    }

    private void ReadGameData()
    {
        if (File.Exists(StructureManager.GetSaveDataPath()))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(StructureManager.GetSaveDataPath(), FileMode.Open);
            GameSaveData data = (GameSaveData)bf.Deserialize(file);
            file.Close();

            saveData = data;
            ShowPriortyPanel = saveData.showPriority;
            ShowVillagerWidgets = saveData.showWidgets;
        }
        else
        {
            // File does not exist, start new game
            StartNewGame(false);
        }
    }

    public void OnLevelComplete()
    {
        // calculate and reward research points
        // set level as completed
        List<MapScreen.Level> levels = new List<MapScreen.Level>();
        GetLevelData(ref levels);
        saveData.researchPoints += levels[currentLevel].reward;
        saveData.levelCompletion[currentLevel] = true;
        saveData.currentMatch.matchWon = true;
        WriteGameData();
    }

    private void StartNewGame(bool _override)
    {
        if (!Application.isEditor && !_override)
        {
            startMaxed = false;
        }
        saveData = new GameSaveData
        {
            research = new Dictionary<int, bool>(),
            levelCompletion = new Dictionary<int, bool>(),
            researchPoints = 0,
            currentMatch = new MatchSaveData()
        };
        saveData.currentMatch.match = false;
        saveData.currentMatch.matchWon = false;
        saveData.showTutorial = true;
        for (int i = 0; i < ResearchDefinitions.Count; i++)
        {
            if (i == 0) { saveData.research.Add(0, true); }
            else { saveData.research.Add(i, startMaxed); }
        }
        for (int i = 0; i < LevelDefinitions.Count; i++)
        {
            saveData.levelCompletion.Add(i, startMaxed);
        }

        WriteGameData();
    }

    public void ResetSaveData()
    {
        StartNewGame(false);
    }

    public void SetShowTutorial(bool _showTutorial)
    {
        saveData.showTutorial = _showTutorial;
    }

    public bool GetShowTutorial()
    {
        return saveData.showTutorial;
    }

    public (Vector4, Vector2) GetCurrentCamSettings()
    {
        return CameraSettings[currentLevel];
    }

    public float GetPoorTimberFactor()
    {
        bool poorTimber = CurrentLevelHasModifier(PoorTimber);
        return poorTimber ? PoorTimberFactor : 1.0f;
    }
}
