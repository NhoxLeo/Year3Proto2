﻿using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuperManager : MonoBehaviour
{
    public static bool DevMode = false;
    public const string DevModePassword = "lollipop";
    public static string runningString = "";
    public static bool raining = false;
    public static bool TitleScreenAnimPlayed = false;
    // SETTINGS
    public static float AmbientVolume = 1.0f;
    public static float MusicVolume = 1.0f;
    public static float EffectsVolume = 1.0f;

    public static bool waveHornStart = false;
    public static bool messageBox = false;
    public static float CameraSensitivity = 4.0f;

    // CONSTANTS
    public const float ScalingFactor = 1.33f;
    public const float PoorTimberFactor = 0.75f;

    // Identifiers
    #region Identifiers
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

    // Structure Research
    // BARRACKS
    public const int Barracks = 0;
    public const int BarracksSoldierDamage = 1;
    public const int BarracksSoldierHealth = 2;
    public const int BarracksSoldierSpeed = 3;
    public const int BarracksFortification = 4;
    public const int BarracksSuper = 5;

    // BALLISTA
    public const int Ballista = 6;
    public const int BallistaRange = 7;
    public const int BallistaPower = 8;
    public const int BallistaFortification = 9;
    public const int BallistaEfficiency = 10;
    public const int BallistaSuper = 11;

    // CATAPULT
    public const int Catapult = 12;
    public const int CatapultRange = 13;
    public const int CatapultPower = 14;
    public const int CatapultFortification = 15;
    public const int CatapultEfficiency = 16;
    public const int CatapultSuper = 17;

    // FREEZE TOWER
    public const int FrostTower = 18;
    public const int FrostTowerRange = 19;
    public const int FrostTowerSlowEffect = 20;
    public const int FrostTowerFortification = 21;
    public const int FrostTowerSuper = 22;

    // LIGHTNING TOWER
    public const int LightningTower = 23;
    public const int LightningTowerRange = 24;
    public const int LightningTowerPower = 25;
    public const int LightningTowerFortification = 26;
    public const int LightningTowerSuper = 27;

    // SHOCKWAVE TOWER
    public const int ShockwaveTower = 28;
    public const int ShockwaveTowerRange = 29;
    public const int ShockwaveTowerStunDuration = 30;
    public const int ShockwaveTowerFortification = 31;
    public const int ShockwaveTowerSuper = 32;

    // MUSIC
    public const int CloudLine = 0;
    public const int FerryLanding = 1;
    public const int IdleWays = 2;
    public const int Stillness = 3;
    public const int StrangeDogWalk = 4;
    public const int VulcanStreet = 5;
    public const int Contention = 6;
    public const int GreatExpectations = 7;
    public const int PyrrhicVictory = 8;
    #endregion

    // Structs
    #region Structs

    #region SaveData

    #region Vector3 & Quaternion

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

    #endregion

    #region Enemies

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
    public struct AirshipSaveData
    {
        public List<string> enemies;
        public SaveVector3 position;
        public SaveVector3 targetPosition;
        public SaveQuaternion orientation;
        public SaveVector3 initialLocation;
        public AirshipState state;
        public int spawnWave;
    }

    #endregion

    #region Soldiers

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

    #endregion

    #region Structures

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

    #endregion

    #region Match

    [Serializable]
    public struct MatchSaveData
    {
        public bool match;
        public bool matchWon;
        public bool tutorialDone;
        public bool repairMessage;
        public bool repairAll;
        public bool spawning;
        public int levelID;
        public int objectivesCompleted;
        public int enemiesKilled;
        public int wave;
        public int nextStructureID;
        public int villagers;
        public int availableVillagers;
        public int manuallyAllocated;
        public int waveAtObjectiveStart;
        public float weightageScalar;
        public float tokenIncrement;
        public float tokenScalar;
        public float time;
        public float tokens;
        public float starveTicks;
        public float tempFood;
        public float tempLumber;
        public float tempMetal;
        public float longhausHealth;
        public float spawnTime;
        public SaveVector3 timeVariance;
        public PlayerResources playerResources;
        public InfoManagerSaveData infoData;
        public List<AirshipSaveData> airships;
        public List<StructureSaveData> structures;
        public List<InvaderSaveData> invaders;
        public List<HeavyInvaderSaveData> heavyInvaders;
        public List<EnemySaveData> flyingInvaders;
        public List<EnemySaveData> petards;
        public List<EnemySaveData> rams;
        public List<SoldierSaveData> soldiers;
        public List<Priority> priorities;
        public Dictionary<string, ResourceBundle> structureCosts;
        public Dictionary<BuildPanel.Buildings, int> structureCounts;
        public EnvironmentWeatherData environmentWeatherData;
        public EnvironmentAmbientData environmentAmbientData;
        public Dictionary<int, WaveData> waveEnemyCounts;
    }

    #endregion

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

    #endregion

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

    #endregion

    private static SuperManager instance = null;
    private GameSaveData saveData;
    public static List<ResearchElementDefinition> ResearchDefinitions = new List<ResearchElementDefinition>()
    {
        // ID, ID requirement, Name, Description, RP Cost, Special Upgrade (false by default)
        new ResearchElementDefinition(Barracks, NoRequirement, "Barracks", "The Barracks spawns Soldiers, which automatically chase down enemies.", 300),
        new ResearchElementDefinition(BarracksSoldierDamage, Barracks, "Soldier Damage", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(BarracksSoldierHealth, Barracks, "Soldier Health", "Health increased by 50%.", 200),
        new ResearchElementDefinition(BarracksSoldierSpeed, Barracks, "Soldier Speed", "Speed increased by 30%.", 200),
        new ResearchElementDefinition(BarracksFortification, Barracks, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(BarracksSuper, Barracks, "Advanced Training", "Soldiers train & heal much faster.", 500, true),

        new ResearchElementDefinition(Ballista, NoRequirement, "Ballista Tower", "The Ballista Tower is great for single target damage, firing bolts at deadly speeds.", 300),
        new ResearchElementDefinition(BallistaRange, Ballista, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(BallistaPower, Ballista, "Power Shot", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(BallistaFortification, Ballista, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(BallistaEfficiency, Ballista, "Efficiency", "Bolt cost reduced by 50%.", 200),
        new ResearchElementDefinition(BallistaSuper, Ballista, "Multishot", "Fires three piercing shots.", 500, true),

        new ResearchElementDefinition(Catapult, NoRequirement, "Catapult Tower", "The Catapult Tower deals splash damage, making it the ideal choice for crowd control.", 300),
        new ResearchElementDefinition(CatapultRange, Catapult, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(CatapultPower, Catapult, "Power Shot", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(CatapultFortification, Catapult, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(CatapultEfficiency, Catapult, "Efficiency", "Boulder cost reduced by 50%.", 200),
        new ResearchElementDefinition(CatapultSuper, Catapult, "Cluster Bomb", "Small bombs erupt from the explosion.", 500, true),

        new ResearchElementDefinition(FrostTower, NoRequirement, "Frost Tower", "The Frost Tower slows down enemies making it easier for other defences to hit them.", 300),
        new ResearchElementDefinition(FrostTowerRange, FrostTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(FrostTowerSlowEffect, FrostTower, "Slow Effect", "Slows Enemies by +30%.", 200),
        new ResearchElementDefinition(FrostTowerFortification, FrostTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(FrostTowerSuper, FrostTower, "Blizzard", "Frost makes enemies more vulnerable", 500, true),

        new ResearchElementDefinition(LightningTower, NoRequirement, "Lightning Tower", "The Lightning Tower shoots bolts at enemies dealing heavy shock damage.", 300),
        new ResearchElementDefinition(LightningTowerRange, LightningTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(LightningTowerPower, LightningTower, "Power", "Damage improved by 30%.", 200),
        new ResearchElementDefinition(LightningTowerFortification, LightningTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(LightningTowerSuper, LightningTower, "Chain Lightning", "Lightning jumps from enemy to enemy.", 500, true),

        new ResearchElementDefinition(ShockwaveTower, NoRequirement, "Shockwave Tower", "The Shockwave Tower releases high energy shockwaves that momentarily stun enemies.", 300),
        new ResearchElementDefinition(ShockwaveTowerRange, ShockwaveTower, "Range Boost", "Extends tower range by 25%.", 200),
        new ResearchElementDefinition(ShockwaveTowerStunDuration, ShockwaveTower, "Stun Duration", "Enemy stun duration increased by 25%", 200),
        new ResearchElementDefinition(ShockwaveTowerFortification, ShockwaveTower, "Fortification", "Improves building durability by 50%.", 200),
        new ResearchElementDefinition(ShockwaveTowerSuper, ShockwaveTower, "Bulldoze", "Damages close enemies.", 500, true),
    };
    public static List<LevelDefinition> LevelDefinitions = new List<LevelDefinition>()
    {
        // ID, ID requirement, Win Condition, Modifiers, Base Reward
        new LevelDefinition(0, NoRequirement,   new List<int>(){ Food, Villagers, Survive },                          new List<int>(),                                            1000),
        new LevelDefinition(1, 0,               new List<int>(){ Villagers, Accumulate, SlaughterII },                  new List<int>(){ SnoballPrices },            1250),
        new LevelDefinition(2, 1,               new List<int>(){ Slaughter, Lumber, VillagersII, AccumulateII },        new List<int>(){ DryFields, PoorTimber },                   1500),
        new LevelDefinition(3, 2,               new List<int>(){ FoodIII, SlaughterIII,  VillagersIII, AccumulateIII },  new List<int>(){ SnoballPrices, PoorTimber, SwiftFootwork },    1750)
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
        new WinConditionDefinition(SlaughterIII, "Slaughter III", "Kill 75 Enemies."),

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

        new WinConditionDefinition(Villagers, "Villagers", "Have a total of 20 villagers."),
        new WinConditionDefinition(VillagersII, "Villagers II", "Have a total of 40 villagers."),
        new WinConditionDefinition(VillagersIII, "Villagers III", "Have a total of 60 villagers."),
    };
    public static Dictionary<int, (Vector4, Vector2)> CameraSettings = new Dictionary<int, (Vector4, Vector2)>()
    {
        {0, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {1, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {2, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) },
        {3, (new Vector4(10, -18, 10, -18), new Vector2(-8, 10)) }
    };
    public static Dictionary<int, SpawnerData> SpawnerSettings = new Dictionary<int, SpawnerData>()
    {
        {0, new SpawnerData(0.2f, 0.0004f, new Vector2(60, 100)) },
        {1, new SpawnerData(0.25f, 0.00045f, new Vector2(60, 100)) },
        {2, new SpawnerData(0.3f, 0.0005f, new Vector2(60, 100)) },
        {3, new SpawnerData(0.4f, 0.0006f, new Vector2(60, 100)) }
    };
    private int currentLevel;
    [SerializeField]
    private bool startMaxed;

    // Music
    // Audio Clips
    private Dictionary<int, AudioClip> GameMusic = new Dictionary<int, AudioClip>();
    private Dictionary<int, string> GameMusicDetails = new Dictionary<int, string>();
    private AudioClip windAmbience = null;
    private AudioClip rainAmbience = null;
    private static AudioClip UIClick = null;

    // Audio sources
    private AudioSource musicAudio;
    private AudioSource windAmbienceAudio;
    private AudioSource rainAmbienceAudio;

    // Management
    private const float MusicDelayMinimum = 5f;
    private float nextSongTimer = 0f;
    private List<int> recentlyPlayedSongs = new List<int>();
    private List<int> songHistory = new List<int>();
    private bool moderateVolume = true;
    private bool playWindAmbience = false;
    private bool musicControls = false;

    #region Unity Messages

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

        LoadMusic();

        AudioSource[] sources = GetComponents<AudioSource>();

        musicAudio = sources[0];
        musicAudio.clip = GameMusic[GreatExpectations];
        musicAudio.volume = 0.4f;
        musicAudio.Play();
        nextSongTimer = musicAudio.clip.length + GetMusicDelayRandom();
        recentlyPlayedSongs.Add(GreatExpectations);

        windAmbienceAudio = sources[1];
        windAmbienceAudio.clip = windAmbience;
        windAmbienceAudio.loop = true;

        rainAmbienceAudio = sources[2];
        rainAmbienceAudio.clip = rainAmbience;
        rainAmbienceAudio.loop = true;
        rainAmbienceAudio.volume = 0.0f;

        if (saveData.gameVersion != Application.version)
        {
            ClearCurrentMatch();
            //saveData.showTutorial = true;
        }

        saveData.gameVersion = Application.version;
    }

    private void Update()
    {


        // Hold both mouse buttons
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (DevMode)
            {
                // Press D
                if (Input.GetKeyDown(KeyCode.D))
                {
                    WipeReloadScene(false);
                }
                // Press S
                if (Input.GetKeyDown(KeyCode.S))
                {
                    startMaxed = true;
                    WipeReloadScene(true);
                    PlayerPrefs.DeleteAll();
                }
            }
        }

        CheckDevModePassword();

        MusicPlayerUpdate();
    }

    public void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            GameManager gameMan = GameManager.GetInstance();
            if (gameMan && !Application.isEditor)
            {
                if (!gameMan.GetGameLost())
                {
                    gameMan.AttemptPause();
                }
            }
        }
    }

    #endregion

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
        if (GameManager.GetInstance().GetGameLost())
        {
            return;
        }
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
        EnvironmentSystem environmentSystem = EnvironmentSystem.GetInstance();

        // easy stuff

        environmentSystem.LoadData(_matchData);
        enemyMan.LoadData(_matchData);
        gameMan.repairAll = _matchData.repairAll;
        gameMan.repairMessage = _matchData.repairMessage;
        gameMan.playerResources = _matchData.playerResources;
        gameMan.gameAlreadyWon = _matchData.matchWon;
        gameMan.objectivesCompleted = _matchData.objectivesCompleted;
        gameMan.foodSinceObjective = _matchData.tempFood;
        gameMan.lumberSinceObjective = _matchData.tempLumber;
        gameMan.metalSinceObjective = _matchData.tempMetal;
        gameMan.waveAtObjectiveStart = _matchData.waveAtObjectiveStart;
        structMan.structureCosts = _matchData.structureCosts;
        structMan.structureCounts = _matchData.structureCounts;
        structMan.SetNextStructureID(_matchData.nextStructureID);
        villagerMan.SetVillagers(_matchData.villagers);
        villagerMan.SetAvailable(_matchData.availableVillagers);
        villagerMan.SetStarveTicks(_matchData.starveTicks);
        villagerMan.SetManuallyAllocated(_matchData.manuallyAllocated);
        villagerMan.LoadPriorities(_matchData.priorities);
        InfoManager.LoadSaveData(_matchData.infoData);
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
        foreach (Barracks barr in allBarracks)
        {
            barr.RefreshRestPositions();
        }
        foreach (SoldierSaveData saveData in _matchData.soldiers)
        {
            for (int i = 0; i < allBarracks.Length; i++)
            {
                if (allBarracks[i].GetID() == saveData.barracksID)
                {
                    allBarracks[i].LoadSoldier(saveData);
                    break;
                }
            }
        }

        enemyMan.LoadAirships(_matchData.airships);

        FindObjectOfType<Longhaus>().SetHealth(_matchData.longhausHealth);

        HUDManager.GetInstance().UpdateVillagerWidgetMode();

        villagerMan.RedistributeVillagers();
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
            structureCosts = structMan.structureCosts,
            structureCounts = structMan.structureCounts,
            playerResources = gameMan.playerResources,
            airships = new List<AirshipSaveData>(),
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
            manuallyAllocated = villMan.GetManuallyAllocated(),
            priorities = villMan.GetPriorities(),
            waveAtObjectiveStart = gameMan.waveAtObjectiveStart,
            infoData = InfoManager.GenerateSaveData()
        };

        EnemyManager.GetInstance().SaveSystemToData(ref save);
        EnvironmentSystem.GetInstance().SaveSystemToData(ref save);

        raining = false;
        rainAmbienceAudio.DOFade(0.0f, 3.0f);


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

        foreach (Airship airship in FindObjectsOfType<Airship>())
        {
            save.airships.Add(airship.GenerateSaveData());
        }

        // structures
        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            // don't save the longhaus
            if (structure.GetStructureName() == StructureNames.Longhaus)
            {
                continue;
            }
            // don't save structures that haven't been placed
            if (!structure.isPlaced)
            {
                continue;
            }

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

    public bool GetResearchComplete(BuildPanel.Buildings _building)
    {
        switch (_building)
        {
            case BuildPanel.Buildings.Ballista:
                return GetResearchComplete(Ballista);
            case BuildPanel.Buildings.Catapult:
                return GetResearchComplete(Catapult);
            case BuildPanel.Buildings.Barracks:
                return GetResearchComplete(Barracks);
            case BuildPanel.Buildings.FrostTower:
                return GetResearchComplete(FrostTower);
            case BuildPanel.Buildings.ShockwaveTower:
                return GetResearchComplete(ShockwaveTower);
            case BuildPanel.Buildings.LightningTower:
                return GetResearchComplete(LightningTower);
            default:
                return true;
        }
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
        saveData.showPriority = true;
        saveData.showWidgets = false;
        for (int i = 0; i < ResearchDefinitions.Count; i++)
        {
            if (i == Barracks) { saveData.research.Add(Barracks, true); }
            else if (i == Ballista) { saveData.research.Add(Ballista, true); }
            else { saveData.research.Add(i, startMaxed); }
        }
        for (int i = 0; i < LevelDefinitions.Count; i++)
        {
            saveData.levelCompletion.Add(i, startMaxed);
        }

        WriteGameData();
    }

    public bool GetShowWidgets()
    {
        return saveData.showWidgets;
    }

    public void SetShowWidgets(bool _showWidgets)
    {
        saveData.showWidgets = _showWidgets;
    }

    public bool GetShowPriority()
    {
        return saveData.showPriority;
    }

    public void SetShowPriority(bool _showPriority)
    {
        saveData.showPriority = _showPriority;
    }

    public void ToggleShowPriority()
    {
        saveData.showPriority = !saveData.showPriority;
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

    public bool GetSnow()
    {
        return currentLevel == 1 || currentLevel == 3;
    }

    public SpawnerData GetCurrentLevelSpawnerData()
    {
        return SpawnerSettings[currentLevel];
    }

    private void LoadMusic()
    {
        GameMusic.Add(CloudLine, Resources.Load("Audio/Music/Blue Dot Sessions - Cloud Line") as AudioClip);
        GameMusic.Add(FerryLanding, Resources.Load("Audio/Music/Blue Dot Sessions - Ferry Landing") as AudioClip);
        GameMusic.Add(IdleWays, Resources.Load("Audio/Music/Blue Dot Sessions - Idle Ways") as AudioClip);
        GameMusic.Add(Stillness, Resources.Load("Audio/Music/Blue Dot Sessions - Stillness") as AudioClip);
        GameMusic.Add(StrangeDogWalk, Resources.Load("Audio/Music/Blue Dot Sessions - Strange Dog Walk") as AudioClip);
        GameMusic.Add(VulcanStreet, Resources.Load("Audio/Music/Blue Dot Sessions - Vulcan Street") as AudioClip);
        GameMusic.Add(Contention, Resources.Load("Audio/Music/Kai Engel - Contention") as AudioClip);
        GameMusic.Add(GreatExpectations, Resources.Load("Audio/Music/Kai Engel - Great Expectations") as AudioClip);
        GameMusic.Add(PyrrhicVictory, Resources.Load("Audio/Music/Lobo Loco - Pyrrhic Victory") as AudioClip);

        GameMusicDetails.Add(CloudLine, "Cloud Line - Blue Dot Sessions");
        GameMusicDetails.Add(FerryLanding, "Ferry Landing - Blue Dot Sessions");
        GameMusicDetails.Add(IdleWays, "Idle Ways - Blue Dot Sessions");
        GameMusicDetails.Add(Stillness, "Stillness - Blue Dot Sessions");
        GameMusicDetails.Add(StrangeDogWalk, "Strange Dog Walk - Blue Dot Sessions");
        GameMusicDetails.Add(VulcanStreet, "Vulcan Street - Blue Dot Sessions");
        GameMusicDetails.Add(Contention, "Contention - Kai Engel");
        GameMusicDetails.Add(GreatExpectations, "Great Expectations - Kai Engel");
        GameMusicDetails.Add(PyrrhicVictory, "Pyrrhic Victory - Lobo Loco");

        windAmbience = Resources.Load("Audio/SFX/sfxWindAmbience") as AudioClip;
        rainAmbience = Resources.Load("Audio/SFX/sfxRain") as AudioClip;
        UIClick = Resources.Load("Audio/SFX/sfxUIClick2") as AudioClip;
    }

    private void MusicPlayerUpdate()
    {
        if (musicControls)
        {
            if (Input.GetKeyDown(KeyCode.Period))
            {
                nextSongTimer = 0f;
            }
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                if (musicAudio.time < 10f && songHistory.Count > 1)
                {
                    musicAudio.clip = GameMusic[songHistory[songHistory.Count - 2]];
                    //Debug.Log("Now Playing " + GameMusicDetails[songHistory[songHistory.Count - 2]]);
                    musicAudio.Play();
                    nextSongTimer = musicAudio.clip.length + GetMusicDelayRandom();
                    recentlyPlayedSongs.Add(songHistory[songHistory.Count - 2]);
                    if (recentlyPlayedSongs.Count > 3)
                    {
                        recentlyPlayedSongs.RemoveAt(0);
                    }
                    songHistory.RemoveAt(songHistory.Count - 1);
                }
                musicAudio.time = 0f;
            }
        }

        nextSongTimer -= Time.deltaTime;
        if (nextSongTimer <= 0f)
        {
            if (GetMenuMusic())
            {
                musicAudio.clip = GameMusic[GreatExpectations];
            }
            else
            {
                songHistory.Add(PickNewRandomTrack());
            }
            nextSongTimer = musicAudio.clip.length + GetMusicDelayRandom();
            musicAudio.Play();
        }
        if (moderateVolume)
        {
            musicAudio.volume = 0.4f * MusicVolume;
            windAmbienceAudio.volume = playWindAmbience ? 0.15f * AmbientVolume : 0f;

            if(raining)
            {
                rainAmbienceAudio.volume = AmbientVolume;
            } 
        }
    }

    private int PickNewRandomTrack()
    {
        // pick a new track to play
        List<int> validSongs = new List<int>();
        for (int i = 0; i < GameMusic.Count; i++)
        {
            validSongs.Add(i);
        }

        if (recentlyPlayedSongs.Count > 0)
        {
            for (int i = 0; i < recentlyPlayedSongs.Count; i++)
            {
                validSongs.Remove(recentlyPlayedSongs[i]);
            }
        }

        if (validSongs.Contains(GreatExpectations))
        {
            validSongs.Remove(GreatExpectations);
        }

        int song = validSongs[UnityEngine.Random.Range(0, validSongs.Count)];
        //Debug.Log("Now Playing " + GameMusicDetails[song]);
        musicAudio.clip = GameMusic[song];
        recentlyPlayedSongs.Add(song);
        if (recentlyPlayedSongs.Count > 3)
        {
            recentlyPlayedSongs.RemoveAt(0);
        }
        return song;
    }

    private bool GetMenuMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "SamDev":
                return false;
            case "Level 2":
                return false;
            case "Level 3":
                return false;
            case "Level 4":
                return false;
            default:
                return true;
        }
    }

    private float GetMusicDelayRandom()
    {
        return MusicDelayMinimum * UnityEngine.Random.Range(1f, 2f);
    }

    public void OnMatchStart()
    {
        nextSongTimer = 0f;
        windAmbienceAudio.Play();
        rainAmbienceAudio.Play();
        windAmbienceAudio.DOFade(0.1f * AmbientVolume, 0.5f);
        playWindAmbience = true;
        moderateVolume = false;
        Invoke("ModerateVolume", 0.5f);
        musicControls = true;
        songHistory.Clear();
    }

    public void OnBackToMenus()
    {
        windAmbienceAudio.DOFade(0.0f, 0.5f);
        rainAmbienceAudio.DOFade(0.0f, 0.5f);
        musicAudio.DOFade(0f, 0.5f);
        Invoke("PlayTitleScreenMusic", 0.5f);
        moderateVolume = false;
        playWindAmbience = false;
        Invoke("ModerateVolume", 0.5f);
        musicControls = false;
        songHistory.Clear();
    }

    public void PlayGameoverMusic(bool _victory)
    {
        // fade out music and play victory/loss music, then fade the music back in
        windAmbienceAudio.DOFade(0f, 0.2f);
        rainAmbienceAudio.DOFade(0.0f, 0.2f);
        musicAudio.DOFade(0f, 0.2f);
        moderateVolume = false;
        string clipName = _victory ? "win" : "lose";
        float delay = GameManager.GetClipLength(clipName);
        GameManager.CreateAudioEffect(clipName, Vector3.zero, SoundType.Music, 1f, false);
        if (_victory)
        {
            Invoke("FadeMusicBackIn", delay);
        }
        else
        {
            Invoke("TurnMusicOff", 0.2f);
            Invoke("FadeWindBackIn", 0.2f);
            Invoke("ModerateVolume", 0.25f);
        }
    }

    public void PlayTitleScreenMusic()
    {
        musicAudio.clip = GameMusic[GreatExpectations];
        musicAudio.Play();
        nextSongTimer = musicAudio.clip.length + GetMusicDelayRandom();
    }

    public void ModerateVolume()
    {
        moderateVolume = true;
    }

    public void FadeMusicBackIn()
    {
        FadeWindBackIn();
        musicAudio.DOFade(0.4f * MusicVolume, 0.5f);
        Invoke("ModerateVolume", 0.5f);
    }

    public void TurnMusicOff()
    {
        musicAudio.Stop();
    }

    public void FadeWindBackIn()
    {
        windAmbienceAudio.DOFade(0.1f * AmbientVolume, 0.5f);
    }

    public void OnPause()
    {
        if (musicAudio.isPlaying)
        {
            musicAudio.Pause();
        }
        if (windAmbienceAudio.isPlaying)
        {
            windAmbienceAudio.Pause();
        }

        if (rainAmbienceAudio.isPlaying)
        {
            rainAmbienceAudio.Pause();
        }
    }

    public void OnResume()
    {
        if (musicAudio.clip)
        {
            if (musicAudio.clip.length != musicAudio.time)
            {
                musicAudio.Play();
            }
        }
        windAmbienceAudio.Play();
        rainAmbienceAudio.Play();
    }

    public static void UIClickSound()
    {
        GameObject spawnAudio = new GameObject("TemporarySoundObject");
        AudioSource spawnAudioComp = spawnAudio.AddComponent<AudioSource>();
        DestroyMe spawnAudioDestroy = spawnAudio.AddComponent<DestroyMe>();
        spawnAudioDestroy.SetLifetime(UIClick.length);
        spawnAudioDestroy.SetUnscaledTime(true);
        spawnAudioComp.clip = UIClick;
        spawnAudioComp.volume = 0.825f * EffectsVolume;
        spawnAudioComp.Play();
    }

    public static void SetBonusHighlightHeight(Transform _bonusHighlight, float _height)
    {
        _bonusHighlight.GetComponent<MeshRenderer>().material.SetFloat("_Height", _height);
    }

    public AudioSource GetRainAudio()
    {
        return rainAmbienceAudio;
    }

    private void CheckDevModePassword()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            runningString += "l";
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            runningString += "o";
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            runningString += "p";
            if (runningString == DevModePassword)
            {
                DevMode = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            runningString += "i";
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            runningString = "";
            DevMode = false;
        }
    }
}
