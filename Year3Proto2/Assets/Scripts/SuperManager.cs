﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SuperManager : MonoBehaviour
{
    // CONSTANTS
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

    // FREEZE TOWER
    public const int FreezeTower = 0;
    public const int FreezeTowerRange = 1;
    public const int FreezeTowerStunDuration = 1;
    public const int FreezeTowerFortification = 3;
    public const int FreezeTowerEfficiency = 4;
    public const int FreezeTowerSuper = 5;

    // TODO REFACTOR NUMBERS

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

        // resource structures
        public bool wasPlacedOn;

        // environment structures
        public bool exploited;
        public int exploiterID;

        // defense structures
        public float timeTrained;
    }

    [Serializable]
    public struct MatchSaveData
    {
        public bool match;
        public int levelID;
        public bool matchWon;
        public PlayerResources playerResources;
        public Dictionary<string, ResourceBundle> structureCosts;
        public Dictionary<BuildPanel.Buildings, int> structureCounts;
        public List<StructureSaveData> structures;
        public List<InvaderSaveData> invaders;
        public List<HeavyInvaderSaveData> heavyInvaders;
        public List<SoldierSaveData> soldiers;
        public int enemiesKilled;
        public float spawnTime;
        public bool spawning;
        public float waveSystemWeightageScalar;
        public float waveSystemTokenIncrement;
        public float waveSystemTokenScalar;
        public float waveSystemTime;
        public SaveVector3 waveSystemTimeVariance;
        public float waveSystemTokens;
        public int wave;
        public bool tutorialDone;
        public bool repairMessage;
        public bool repairAll;
        public int nextStructureID;
        public int villagers;
        public int availableVillagers;
        public int starveTicks;
    }

    [Serializable]
    public struct GameSaveData
    {
        public Dictionary<int, bool> research;
        public Dictionary<int, bool> levelCompletion;
        public int researchPoints;
        public MatchSaveData currentMatch;
        public bool showTutorial;
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
        public int winCond;
        public List<int> modifiers;
        public int reward;

        public LevelDefinition(int _id, int _reqID, int _winCond, List<int> _modifiers, int _reward)
        {
            ID = _id;
            reqID = _reqID;
            winCond = _winCond;
            modifiers = _modifiers;
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
    public static List<ResearchElementDefinition> researchDefinitions;
    public static List<LevelDefinition> levelDefinitions;
    public static List<ModifierDefinition> modDefinitions;
    public static List<WinConditionDefinition> winConditionDefinitions;
    public int currentLevel;
    [SerializeField]
    private bool startMaxed;

    public static SuperManager GetInstance()
    {
        return instance;
    }

    public void PlayLevel(int _level)
    {
        currentLevel = _level;
        if (_level != saveData.currentMatch.levelID) { ClearCurrentMatch(); }
        FindObjectOfType<SceneSwitcher>().SceneSwitchLoad("SamDev");
    }

    // populates _levelData with the levels
    public void GetLevelData(ref List<MapScreen.Level> _levelData)
    {
        if (_levelData == null) { _levelData = new List<MapScreen.Level>(); }
        else { _levelData.Clear(); }

        for (int i = 0; i < levelDefinitions.Count; i++)
        {
            MapScreen.Level newLevelData = new MapScreen.Level
            {
                completed = saveData.levelCompletion[i],
                locked = levelDefinitions[i].reqID == -1 ? false : !saveData.levelCompletion[levelDefinitions[i].reqID],
                inProgress = saveData.currentMatch.match && saveData.currentMatch.levelID == i,
                victoryTitle = winConditionDefinitions[levelDefinitions[i].winCond].name,
                victoryDescription = winConditionDefinitions[levelDefinitions[i].winCond].description,
                victoryValue = levelDefinitions[i].reward,
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
        for (int i = 0; i < modDefinitions.Count; i++)
        {
            if (modDefinitions[i].name == _modifierName)
            {
                _modifierDefinition = modDefinitions[i];
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

        for (int i = 0; i < levelDefinitions[levelID].modifiers.Count; i++)
        {
            MapScreen.Modifier mod = new MapScreen.Modifier
            {
                title = modDefinitions[levelDefinitions[levelID].modifiers[i]].name,
                description = modDefinitions[levelDefinitions[levelID].modifiers[i]].description,
                modBonus = modDefinitions[levelDefinitions[levelID].modifiers[i]].coefficient
            };
            _modifierData.Add(mod);
        }
    }

    public int GetCurrentWinCondition()
    {
        return levelDefinitions[currentLevel].winCond;
    }

    void DataInitialization()
    {
        researchDefinitions = new List<ResearchElementDefinition>()
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

            new ResearchElementDefinition(Barracks, NoRequirement, "Barracks", "The Barracks spawns enemy soldiers, which automatically chase down enemies.", 300),
            new ResearchElementDefinition(BarracksSoldierDamage, Barracks, "Soldier Damage", "Damage improved by 30%.", 200),
            new ResearchElementDefinition(BarracksSoldierHealth, Barracks, "Soldier Health", "Health increased by 50%.", 200),
            new ResearchElementDefinition(BarracksSoldierSpeed, Barracks, "Soldier Speed", "Speed increased by 30%.", 200),
            new ResearchElementDefinition(BarracksFortification, Barracks, "Fortification", "Improves building durability by 50%.", 200),
            new ResearchElementDefinition(BarracksSuper, Barracks, "Rapid Courses", "Barracks spawn & heal soldiers faster.", 500, true),
        };
        levelDefinitions = new List<LevelDefinition>()
        {
            // ID, ID requirement, Win Condition, Modifiers, Base Reward
            new LevelDefinition(0, NoRequirement, Accumulate, new List<int>(), 500),
            new LevelDefinition(1, 0, Survive, new List<int>(){ SnoballPrices, SwiftFootwork }, 750),
            new LevelDefinition(2, 1, SurviveII, new List<int>(){ DryFields, PoorTimber }, 1000),
            new LevelDefinition(3, 2, AccumulateIII, new List<int>(){ SnoballPrices, DryFields, PoorTimber }, 1500)
        };
        modDefinitions = new List<ModifierDefinition>()
        { 
            // ID, Name, Description, Coefficient
            new ModifierDefinition(SnoballPrices, "Snowball Prices", "Structures cost more as you place them.", 0.5f),
            new ModifierDefinition(SwiftFootwork, "Swift Footwork", "Enemies are 40% faster.", 0.25f),
            new ModifierDefinition(DryFields, "Dry Fields", "Food production is halved.", 0.35f),
            new ModifierDefinition(PoorTimber, "Poor Timber", "Buildings have 50% of their standard durability.", 0.4f),
        };
        winConditionDefinitions = new List<WinConditionDefinition>()
        { 
            // ID, Name, Description
            new WinConditionDefinition(Accumulate, "Accumulate", "Gather 1500 of each resource."),
            new WinConditionDefinition(AccumulateII, "Accumulate II", "Gather 2500 of each resource."),
            new WinConditionDefinition(AccumulateIII, "Accumulate III", "Gather 7500 of each resource."),
            new WinConditionDefinition(Slaughter, "Slaughter", "Kill 300 Enemies."),
            new WinConditionDefinition(SlaughterII, "Slaughter II", "Kill 800 Enemies."),
            new WinConditionDefinition(SlaughterIII, "Slaughter III", "Kill 2000 Enemies."),
            new WinConditionDefinition(Survive, "Survive", "Defend against 25 waves."),
            new WinConditionDefinition(SurviveII, "Survive II", "Defend against 50 waves."),
            new WinConditionDefinition(SurviveIII, "Survive III", "Defend against 100 waves."),
        };

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
        DataInitialization();
        currentLevel = 0;
        if (startMaxed) { StartNewGame(); }
        else { ReadGameData(); }
    }

    private void Update()
    {
        // Hold control
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Press D
            if (Input.GetKeyDown(KeyCode.D))
            {
                WipeReloadScene();
            }
            // Press M
            if (Input.GetKeyDown(KeyCode.M))
            {
                if(GameManager.GetInstance())
                {
                    GameManager.GetInstance().playerResources.AddBatch(new ResourceBatch(500, ResourceType.Food));
                    GameManager.GetInstance().playerResources.AddBatch(new ResourceBatch(500, ResourceType.Wood));
                    GameManager.GetInstance().playerResources.AddBatch(new ResourceBatch(500, ResourceType.Metal));
                }
            }
        }
    }

    private void WipeReloadScene()
    {
        if (File.Exists(StructureManager.GetSaveDataPath()))
        {
            File.Delete(StructureManager.GetSaveDataPath());
        }
        ReadGameData();
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

        // easy stuff
        currentLevel = _matchData.levelID;
        GameManager.GetInstance().repairAll = _matchData.repairAll;
        GameManager.GetInstance().repairMessage = _matchData.repairMessage;
        GameManager.GetInstance().tutorialDone = _matchData.tutorialDone;
        StructureManager.GetInstance().structureCosts = _matchData.structureCosts;
        StructureManager.GetInstance().structureCounts = _matchData.structureCounts;
        StructureManager.GetInstance().SetNextStructureID(_matchData.nextStructureID);
        GameManager.GetInstance().playerResources = _matchData.playerResources;
        EnemyManager.GetInstance().SetSpawning(_matchData.spawning);
        EnemyManager.GetInstance().SetWave(_matchData.wave);
        EnemyManager.GetInstance().SetTime(_matchData.spawnTime);
        EnemyManager.GetInstance().SetEnemiesKilled(_matchData.enemiesKilled);
        GameManager.GetInstance().gameAlreadyWon = _matchData.matchWon;
        VillagerManager.GetInstance().SetVillagers(_matchData.villagers);
        VillagerManager.GetInstance().SetAvailable(_matchData.availableVillagers);
        EnemyManager.GetInstance().LoadSystemFromData(_matchData);
        VillagerManager.GetInstance().SetStarveTicks(_matchData.starveTicks);
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
            EnemyManager.GetInstance().LoadInvader(saveData);
        }

        // heavies
        foreach (HeavyInvaderSaveData saveData in _matchData.heavyInvaders)
        {
            EnemyManager.GetInstance().LoadHeavyInvader(saveData);
        }

        // soldiers
        Barracks[] allBarracks = FindObjectsOfType<Barracks>();
        foreach (SoldierSaveData saveData in _matchData.soldiers)
        {
            foreach (Barracks barr in allBarracks)
            {
                if (barr.GetID() == saveData.barracksID)
                {
                    //barr.LoadSoldier(saveData);
                    break;
                }
            }
        }

        // enemies are spawned, let the towers detect them
        foreach (AttackStructure attackStructure in FindObjectsOfType<AttackStructure>())
        {
            attackStructure.DetectEnemies();
        }

        return true;
    }

    public void ClearCurrentMatch()
    {
        saveData.currentMatch.match = false;
        WriteGameData();
    }

    private MatchSaveData SaveMatch()
    {
        // define a MatchSaveData with the current game state
        MatchSaveData save = new MatchSaveData
        {
            match = true,
            levelID = currentLevel,
            repairAll = GameManager.GetInstance().repairAll,
            repairMessage = GameManager.GetInstance().repairMessage,
            tutorialDone = GameManager.GetInstance().tutorialDone,
            structureCosts = StructureManager.GetInstance().structureCosts,
            structureCounts = StructureManager.GetInstance().structureCounts,
            playerResources = GameManager.GetInstance().playerResources,
            wave = EnemyManager.GetInstance().GetWaveCurrent(),
            invaders = new List<InvaderSaveData>(),
            heavyInvaders = new List<HeavyInvaderSaveData>(),
            soldiers = new List<SoldierSaveData>(),
            structures = new List<StructureSaveData>(),
            enemiesKilled = EnemyManager.GetInstance().GetEnemiesKilled(),
            matchWon = GameManager.GetInstance().WinConditionIsMet() || GameManager.GetInstance().gameAlreadyWon,
            nextStructureID = StructureManager.GetInstance().GetNextStructureID(),
            villagers = VillagerManager.GetInstance().GetVillagers(),
            availableVillagers = VillagerManager.GetInstance().GetAvailable(),
            spawnTime = EnemyManager.GetInstance().GetTime(),
            starveTicks = VillagerManager.GetInstance().GetStarveTicks()
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
                    health = invader.health,
                    position = new SaveVector3(invader.transform.position),
                    orientation = new SaveQuaternion(invader.transform.rotation),
                    targetPosition = new SaveVector3(invader.GetTarget().transform.position),
                    state = invader.GetState()
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
                    health = heavy.health,
                    position = new SaveVector3(heavy.transform.position),
                    orientation = new SaveQuaternion(heavy.transform.rotation),
                    targetPosition = new SaveVector3(heavy.GetTarget().transform.position),
                    state = heavy.GetState()
                },
                equipment = heavy.GetEquipment()
            };
            save.heavyInvaders.Add(saveData);
        }

        // soldiers
        foreach (Soldier soldier in FindObjectsOfType<Soldier>())
        {
            SoldierSaveData saveData = new SoldierSaveData
            {
                health = soldier.health,
                position = new SaveVector3(soldier.transform.position),
                orientation = new SaveQuaternion(soldier.transform.rotation),
                barracksID = soldier.barracksID,
                state = soldier.state,
                returnHome = soldier.returnHome
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
                    ID = structure.GetID()
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
                if (structure.IsStructure("Barracks"))
                {
                    //saveData.timeTrained = structure.gameObject.GetComponent<Barracks>().GetTimeTrained();
                }
                save.structures.Add(saveData);
            }
        }

        return save;
    }

    public bool GetResearchComplete(int _ID)
    {
        if (saveData.research == null) { RestoreSaveData(); }
        return saveData.research[_ID];
    }

    public Dictionary<int, bool> GetResearch()
    {
        if (saveData.research == null) { RestoreSaveData(); }
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
            WipeReloadScene();
        }
        Debug.Log("RestoreSaveData ends...");
    }

    public MatchSaveData GetSavedMatch()
    {
        return saveData.currentMatch;
    }

    public bool CanPlayLevel(int _ID)
    {
        int reqID = levelDefinitions[_ID].reqID;
        // if the level does not require any levels to be complete
        if (reqID == -1)
        { return true; }
        // if the level does need a level to be completed
        else
        { return GetLevelComplete(reqID); }
    }

    public bool CurrentLevelHasModifier(int _modifierID)
    {
        return levelDefinitions[currentLevel].modifiers.Contains(_modifierID);
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
            if (researchDefinitions[_ID].price <= GetResearchPoints())
            {
                saveData.researchPoints -= researchDefinitions[_ID].price;
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
            StartNewGame();
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

    private void StartNewGame()
    {
        if (!Application.isEditor)
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
        for (int i = 0; i < researchDefinitions.Count; i++)
        {
            if (i == 0) { saveData.research.Add(0, true); }
            else { saveData.research.Add(i, startMaxed); }
        }
        for (int i = 0; i < levelDefinitions.Count; i++)
        {
            saveData.levelCompletion.Add(i, startMaxed);
        }

        WriteGameData();
    }

    public void ResetSaveData()
    {
        StartNewGame();
    }

    public void SetShowTutorial(bool _showTutorial)
    {
        saveData.showTutorial = _showTutorial;
    }

    public bool GetShowTutorial()
    {
        return saveData.showTutorial;
    }
}
