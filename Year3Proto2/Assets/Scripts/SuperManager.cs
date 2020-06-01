using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class SuperManager : MonoBehaviour
{
    // CONSTANTS
    public const int k_iNoRequirement = -1;

    // Modifiers
    public const int k_iSnoballPrices = 0;
    public const int k_iSwiftFootwork = 1;
    public const int k_iDryFields = 2;
    public const int k_iPoorTimber = 3;

    // Win Conditions
    public const int k_iAccumulate = 0;
    public const int k_iAccumulateII = 1;
    public const int k_iAccumulateIII = 2;
    public const int k_iSlaughter = 3;
    public const int k_iSlaughterII = 4;
    public const int k_iSlaughterIII = 5;
    public const int k_iSurvive = 6;
    public const int k_iSurviveII = 7;
    public const int k_iSurviveIII = 8;

    // Research Elements
    public const int k_iBallista = 0;
    public const int k_iBallistaRange = 1;
    public const int k_iBallistaPower = 2;
    public const int k_iBallistaFortification = 3;
    public const int k_iBallistaEfficiency = 4;
    public const int k_iBallistaSuper = 5;
    public const int k_iCatapult = 6;
    public const int k_iCatapultRange = 7;
    public const int k_iCatapultPower = 8;
    public const int k_iCatapultFortification = 9;
    public const int k_iCatapultEfficiency = 10;
    public const int k_iCatapultSuper = 11;

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
    public struct StructureSaveData
    {
        public string structure;
        public float health;
        public StructureType type;
        public SaveVector3 position;
        public int foodAllocation;
        public bool wasPlacedOn;
    }

    [Serializable]
    public struct EnemySaveData
    {
        public string enemy;
        public SaveVector3 position;
        public SaveQuaternion orientation;
        public SaveVector3 targetPosition;
        public EnemyState state;
        public float scale;
    }

    [Serializable]
    public struct MatchSaveData
    {
        public bool match;
        public int levelID;
        public PlayerResources playerResources;
        public Dictionary<string, ResourceBundle> structureCosts;
        public Dictionary<BuildPanel.Buildings, int> structureCounts;
        public List<StructureSaveData> structures;
        public List<EnemySaveData> enemies;
        public int enemyWaveSize;
        public int enemiesKilled;
        public float spawnerCooldown;
        public bool spawning;
        public int wave;
        public bool tutorialDone;
        public bool repairMessage;
        public bool repairAll;
    }

    [Serializable]
    public struct GameSaveData
    {
        public Dictionary<int, bool> research;
        public Dictionary<int, bool> levelCompletion;
        public int researchPoints;
        public MatchSaveData currentMatch;
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

    private GameManager gameMan;
    private StructureManager structMan;
    private EnemySpawner enemySpawner;

    public static SuperManager GetInstance()
    {
        return instance;
    }

    public void PlayLevel(int _level)
    {
        currentLevel = _level;
        if (_level != saveData.currentMatch.levelID) { ClearCurrentMatch(); }
        FindObjectOfType<SceneSwitcher>().SceneSwitch("SamDev");
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
            new ResearchElementDefinition(k_iBallista, k_iNoRequirement, "Ballista Tower", "The Ballista Tower is great for single target damage, firing bolts at deadly speeds.", 0),
            new ResearchElementDefinition(k_iBallistaRange, k_iBallista, "Range Boost", "Extends tower range by 25%.", 200),
            new ResearchElementDefinition(k_iBallistaPower, k_iBallista, "Power Shot", "Bolt velocity and damage improved by 30%.", 200),
            new ResearchElementDefinition(k_iBallistaFortification, k_iBallista, "Fortification", "Improves building durability by 50%.", 200),
            new ResearchElementDefinition(k_iBallistaEfficiency, k_iBallista, "Efficiency", "Bolt cost reduced by 50%.", 200),
            new ResearchElementDefinition(k_iBallistaSuper, k_iBallista, "Piercing Shot", "Bolts rip right through their targets.", 500, true),

            new ResearchElementDefinition(k_iCatapult, k_iNoRequirement, "Catapult Tower", "The Catapult Tower deals splash damage, making it the ideal choice for crowd control.", 300),
            new ResearchElementDefinition(k_iCatapultRange, k_iCatapult, "Range Boost", "Extends tower range by 25%.", 200),
            new ResearchElementDefinition(k_iCatapultPower, k_iCatapult, "Power Shot", "Boulder velocity and damage improved by 30%.", 200),
            new ResearchElementDefinition(k_iCatapultFortification, k_iCatapult, "Fortification", "Improves building durability by 50%.", 200),
            new ResearchElementDefinition(k_iCatapultEfficiency, k_iCatapult, "Efficiency", "Boulder cost reduced by 50%.", 200),
            new ResearchElementDefinition(k_iCatapultSuper, k_iCatapult, "Big Shockwave", "Boulders have a 50% larger damage radius.", 500, true),
        };
        levelDefinitions = new List<LevelDefinition>()
        {
            // ID, ID requirement, Win Condition, Modifiers, Base Reward
            new LevelDefinition(0, k_iNoRequirement, k_iAccumulate, new List<int>(){ k_iSnoballPrices }, 500),
            new LevelDefinition(1, 0, k_iSurvive, new List<int>(){ k_iSnoballPrices, k_iSwiftFootwork }, 750),
            new LevelDefinition(2, 1, k_iSurviveII, new List<int>(){ k_iDryFields, k_iPoorTimber }, 1000),
            new LevelDefinition(3, 2, k_iAccumulateIII, new List<int>(){ k_iSnoballPrices, k_iDryFields, k_iPoorTimber }, 1500)
        };
        modDefinitions = new List<ModifierDefinition>()
        { 
            // ID, Name, Description, Coefficient
            new ModifierDefinition(k_iSnoballPrices, "Snowball Prices", "Structure price acceleration hits twice as hard.", 0.5f),
            new ModifierDefinition(k_iSwiftFootwork, "Swift Footwork", "Enemies are 40% faster.", 0.25f),
            new ModifierDefinition(k_iDryFields, "Dry Fields", "Food production is halved.", 0.35f),
            new ModifierDefinition(k_iPoorTimber, "Poor Timber", "Buildings have 50% of their standard durability.", 0.4f),
        };
        winConditionDefinitions = new List<WinConditionDefinition>()
        { 
            // ID, Name, Description
            new WinConditionDefinition(k_iAccumulate, "Accumulate", "Gather 1500 of each resource."),
            new WinConditionDefinition(k_iAccumulateII, "Accumulate II", "Gather 2500 of each resource."),
            new WinConditionDefinition(k_iAccumulateIII, "Accumulate III", "Gather 7500 of each resource."),
            new WinConditionDefinition(k_iSlaughter, "Slaughter", "Kill 300 Enemies."),
            new WinConditionDefinition(k_iSlaughterII, "Slaughter II", "Kill 800 Enemies."),
            new WinConditionDefinition(k_iSlaughterIII, "Slaughter III", "Kill 2000 Enemies."),
            new WinConditionDefinition(k_iSurvive, "Survive", "Defend against 25 waves."),
            new WinConditionDefinition(k_iSurviveII, "Survive II", "Defend against 50 waves."),
            new WinConditionDefinition(k_iSurviveIII, "Survive III", "Defend against 100 waves."),
        };

    }

    void Awake()
    {
        if (instance)
        {
            instance.RefreshManagers();
            Destroy(gameObject);
            return;
        }
        instance = GetComponent<SuperManager>();
        DontDestroyOnLoad(gameObject);
        gameMan = FindObjectOfType<GameManager>();
        structMan = FindObjectOfType<StructureManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
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

    public void RefreshManagers()
    {
        gameMan = FindObjectOfType<GameManager>();
        structMan = FindObjectOfType<StructureManager>();
        enemySpawner = FindObjectOfType<EnemySpawner>();
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
        enemySpawner.enemiesPerWave = _matchData.enemyWaveSize;
        gameMan.repairAll = _matchData.repairAll;
        gameMan.repairMessage = _matchData.repairMessage;
        gameMan.tutorialDone = _matchData.tutorialDone;
        structMan.structureCosts = _matchData.structureCosts;
        structMan.structureCounts = _matchData.structureCounts;
        gameMan.playerResources = _matchData.playerResources;
        if (_matchData.spawning != enemySpawner.IsSpawning()) { enemySpawner.ToggleSpawning(); }
        enemySpawner.SetWaveCurrent(_matchData.wave);
        enemySpawner.cooldown = _matchData.spawnerCooldown;
        currentLevel = _matchData.levelID;
        enemySpawner.SetKillCount(_matchData.enemiesKilled);
        // not so easy stuff...
        
        // structures
        // first, environment structures
        // environment structures are done first so that resource structures can recalculate their tileBonus
        foreach (StructureSaveData saveData in _matchData.structures)
        {
            // check if the structure is environment
            if (saveData.type == StructureType.environment)
            {
                structMan.LoadBuilding(saveData);
            }
        }
        // second, non-environment structures
        foreach (StructureSaveData saveData in _matchData.structures)
        {
            // check if the structure isn't environment
            if (saveData.type != StructureType.environment)
            {
                structMan.LoadBuilding(saveData);
            }
        }

        // enemies
        foreach (EnemySaveData saveData in _matchData.enemies)
        {
            enemySpawner.LoadEnemy(saveData);
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
        RefreshManagers();
        // define a MatchSaveData with the current game state
        MatchSaveData save = new MatchSaveData
        {
            match = true,
            levelID = currentLevel,
            enemyWaveSize = enemySpawner.enemiesPerWave,
            repairAll = gameMan.repairAll,
            repairMessage = gameMan.repairMessage,
            tutorialDone = gameMan.tutorialDone,
            structureCosts = structMan.structureCosts,
            structureCounts = structMan.structureCounts,
            playerResources = gameMan.playerResources,
            spawning = enemySpawner.IsSpawning(),
            wave = enemySpawner.GetWaveCurrent(),
            enemies = new List<EnemySaveData>(),
            structures = new List<StructureSaveData>(),
            enemiesKilled = enemySpawner.GetKillCount(),
            spawnerCooldown = enemySpawner.cooldown            
        };

        // not so easy stuff...
        // enemies
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            EnemySaveData saveData = new EnemySaveData
            {
                enemy = "Invader", // TODO this needs to detect what kind of enemy it is
                position = new SaveVector3(enemy.transform.position),
                orientation = new SaveQuaternion(enemy.transform.rotation),
                scale = enemy.scale,
                targetPosition = new SaveVector3(enemy.GetTarget().transform.position),
                state = enemy.GetState()
            };
            save.enemies.Add(saveData);
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
                    foodAllocation = structure.GetFoodAllocation(),
                    health = structure.GetHealth()
                };
                if (structure.IsStructure("Farm")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<Farm>().wasPlacedOnPlains; }
                if (structure.IsStructure("Mine")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<Mine>().wasPlacedOnHills; }
                if (structure.IsStructure("Lumber Mill")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<LumberMill>().wasPlacedOnForest; }
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
        saveData.researchPoints += 30;
        saveData.levelCompletion[currentLevel] = true;
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
            researchPoints = 1000,
            currentMatch = new MatchSaveData()
        };
        saveData.currentMatch.match = false;
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
}
