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
        public int RPCost;

        public ResearchElementDefinition(int _id, int _rpCost, int _reqID)
        {
            ID = _id;
            RPCost = _rpCost;
            reqID = _reqID;
        }
    }

    [Serializable]
    public struct LevelDefinition
    {
        public int ID;
        public int reqID;
        public Dictionary<int, bool> modifiers;
        public int maxWaves;

        public LevelDefinition(int _id, int _reqID, Dictionary<int, bool> _modifiers, int _maxWaves = 0)
        {
            ID = _id;
            reqID = _reqID;
            modifiers = _modifiers;
            maxWaves = _maxWaves;
        }
    }

    private static SuperManager instance = null;
    public GameSaveData saveData;
    public List<ResearchElementDefinition> researchDefinitions;
    public static List<LevelDefinition> levels;
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

        researchDefinitions = new List<ResearchElementDefinition>()
        {
            new ResearchElementDefinition(0, 50, -1),
            new ResearchElementDefinition(1, 50, 0)
        };
        levels = new List<LevelDefinition>()
        {
            new LevelDefinition(0, -1, new Dictionary<int, bool>(){ {0, true}, {1, false} }, 5),
            new LevelDefinition(1, 0, new Dictionary<int, bool>(){ {0, false}, {1, false} }),
            new LevelDefinition(2, 0, new Dictionary<int, bool>(){ {0, false}, {1, true} })
        };
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
        int reqID = levels[_ID].reqID;
        // if the level does not require any levels to be complete
        if (reqID == -1)
        { return true; }
        // if the level does need a level to be completed
        else
        { return GetLevelComplete(reqID); }
    }

    public bool CurrentLevelHasModifier(Modifiers _modifier)
    {
        return levels[currentLevel].modifiers[(int)_modifier];
    }

    public int GetResearchPoints()
    {
        return saveData.researchPoints;
    }

    public bool AttemptResearch(int _ID)
    {
        if (!saveData.research[_ID])
        {
            if (researchDefinitions[_ID].RPCost <= GetResearchPoints())
            {
                saveData.researchPoints -= researchDefinitions[_ID].RPCost;
                saveData.research[_ID] = true;
                WriteGameData();
                return true;
            }
        }
        return false;
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
        if (!startMaxed)
        {
            saveData.research = new Dictionary<int, bool>()
            {
                { 0, false },
                { 1, false }
            };
            saveData.levelCompletion = new Dictionary<int, bool>()
            {
                { 1, false },
                { 2, false },
                { 3, false }
            };
            saveData.researchPoints = 0;
        }
        else
        {
            saveData.research = new Dictionary<int, bool>()
            {
                { 0, true },
                { 1, true }
            };
            saveData.levelCompletion = new Dictionary<int, bool>()
            {
                { 1, true },
                { 2, true },
                { 3, true }
            };
            saveData.researchPoints = 500;
        }

        saveData.currentMatch = new MatchSaveData();
        saveData.currentMatch.match = false;

        WriteGameData();
    }

    public void ResetSaveData()
    {
        StartNewGame();
    }
}
