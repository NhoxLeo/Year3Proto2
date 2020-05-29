using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System;
using System.IO;

public class SuperManager : MonoBehaviour
{
    public enum Modifiers
    {
        CostIncrementing = 0
    }

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
    public GameSaveData saveData;
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

    public static void PlayLevel(int _level)
    {
        GetInstance().currentLevel = _level;
        if (_level != instance.saveData.currentMatch.levelID)
        { instance.ClearCurrentMatch(); }
        FindObjectOfType<SceneSwitcher>().SceneSwitch("SamDev");
    }

    // populates _levelData with the levels
    public static void GetLevelData(ref List<MapScreen.Level> _levelData)
    {
        GetInstance();
        if (_levelData == null) { _levelData = new List<MapScreen.Level>(); }
        else { _levelData.Clear(); }

        for (int i = 0; i < levelDefinitions.Count; i++)
        {
            MapScreen.Level newLevelData = new MapScreen.Level();
            newLevelData.completed = instance.saveData.levelCompletion[i];
            newLevelData.locked = levelDefinitions[i].reqID == -1 ? false : instance.saveData.levelCompletion[levelDefinitions[i].reqID];
            newLevelData.inProgress = instance.saveData.currentMatch.match && instance.saveData.currentMatch.levelID == i;
            newLevelData.victoryTitle = winConditionDefinitions[levelDefinitions[i].winCond].name;
            newLevelData.victoryDescription = winConditionDefinitions[levelDefinitions[i].winCond].description;
            newLevelData.victoryValue = levelDefinitions[i].reward;
            newLevelData.modifiers = new List<MapScreen.Modifier>();
            GetModifierData(i, ref newLevelData.modifiers);
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
            MapScreen.Modifier mod = new MapScreen.Modifier();
            mod.title = modDefinitions[levelDefinitions[levelID].modifiers[i]].name;
            mod.description = modDefinitions[levelDefinitions[levelID].modifiers[i]].description;
            mod.modBonus = modDefinitions[levelDefinitions[levelID].modifiers[i]].coefficient;
            _modifierData.Add(mod);
        }
    }

    void DataInitialization()
    {
        researchDefinitions = new List<ResearchElementDefinition>()
        {
            new ResearchElementDefinition(0, -1, "Archer Tower", "Archer Tower Building Description", 200),
            new ResearchElementDefinition(1, 0, "Range Boost", "Extends defence range by 15%", 100),
            new ResearchElementDefinition(2, 0, "Power Shot", "Extends defence range by 15%", 100),
            new ResearchElementDefinition(3, 0, "Fortification", "Extends defence range by 15%", 100),
            new ResearchElementDefinition(4, 0, "Efficiency", "Extends defence range by 15%", 100),
            new ResearchElementDefinition(5, 0, "Piercing Shot", "Extends defence range by 15%", 100, true),
        };
        levelDefinitions = new List<LevelDefinition>()
        {
            new LevelDefinition(0, -1, 0, new List<int>(){ 0 }, 500),
            new LevelDefinition(1, 0, 0, new List<int>(), 750),
            new LevelDefinition(2, 1, 0, new List<int>(){ 1 }, 750),
            new LevelDefinition(3, 2, 1, new List<int>(), 1000)
        };
        modDefinitions = new List<ModifierDefinition>()
        { 
            new ModifierDefinition(0, "Snowball Prices", "The cost of building a structure goes up as you build more of the same kind.", 0.3f),
            new ModifierDefinition(1, "Hermes Boots", "Enemies are 40% faster.", 0.4f)
        };
        winConditionDefinitions = new List<WinConditionDefinition>()
        { 
            new WinConditionDefinition(0, "Accumulate", "Gather the target total of each resource."),
            new WinConditionDefinition(1, "Slaughter", "Gather the target total of each resource.")
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
        // define a MatchSaveData with the current game state
        MatchSaveData save = new MatchSaveData();
        save.match = true;

        RefreshManagers();

        // easy stuff
        save.levelID = currentLevel;
        save.enemyWaveSize = enemySpawner.enemiesPerWave;
        save.repairAll = gameMan.repairAll;
        save.repairMessage = gameMan.repairMessage;
        save.tutorialDone = gameMan.tutorialDone;
        save.structureCosts = structMan.structureCosts;
        save.structureCounts = structMan.structureCounts;
        save.playerResources = gameMan.playerResources;
        save.spawning = enemySpawner.IsSpawning();
        save.wave = enemySpawner.GetWaveCurrent();

        // not so easy stuff...
        // enemies
        save.enemies = new List<EnemySaveData>();
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            EnemySaveData saveData = new EnemySaveData();
            saveData.enemy = "Invader"; // TODO this needs to detect what kind of enemy it is
            saveData.position = new SaveVector3(enemy.transform.position);
            saveData.orientation = new SaveQuaternion(enemy.transform.rotation);
            saveData.scale = enemy.scale;
            saveData.targetPosition = new SaveVector3(enemy.GetTarget().transform.position);
            saveData.state = enemy.GetState();
            save.enemies.Add(saveData);
        }

        // structures
        save.structures = new List<StructureSaveData>();
        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            // structures placed by the structureManager don't have a parent, and need to be saved
            if (structure.transform.parent == null)
            {
                StructureSaveData saveData = new StructureSaveData();
                saveData.structure = structure.GetStructureName();
                saveData.type = structure.GetStructureType();
                if (structure.IsStructure("Farm")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<Farm>().wasPlacedOnPlains; }
                if (structure.IsStructure("Mine")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<Mine>().wasPlacedOnHills; }
                if (structure.IsStructure("Lumber Mill")) { saveData.wasPlacedOn = structure.gameObject.GetComponent<LumberMill>().wasPlacedOnForest; }
                saveData.position = new SaveVector3(structure.transform.position);
                saveData.foodAllocation = structure.GetFoodAllocation();
                saveData.health = structure.GetHealth();
                save.structures.Add(saveData);
            }
        }

        return save;
    }

    public bool GetResearchComplete(int _ID)
    {
        return saveData.research[_ID];
    }

    public bool GetLevelComplete(int _ID)
    {
        return saveData.levelCompletion[_ID];
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
        saveData = new GameSaveData();
        saveData.research = new Dictionary<int, bool>();
        for (int i = 0; i < researchDefinitions.Count; i++)
        {
            saveData.research.Add(i, startMaxed);
        }
        saveData.levelCompletion = new Dictionary<int, bool>();
        for (int i = 0; i < levelDefinitions.Count; i++)
        {
            saveData.levelCompletion.Add(i, startMaxed);
        }
        saveData.researchPoints = 1000;
        saveData.currentMatch = new MatchSaveData();
        saveData.currentMatch.match = false;

        WriteGameData();
    }

    public void ResetSaveData()
    {
        StartNewGame();
    }
}
