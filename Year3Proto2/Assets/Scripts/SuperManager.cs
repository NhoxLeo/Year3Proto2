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
    public struct SaveData
    {
        public Dictionary<int, bool> research;
        public Dictionary<int, bool> levelCompletion;
        public int researchPoints;
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
    public SaveData saveData;
    public List<ResearchElementDefinition> researchDefinitions;
    public static List<LevelDefinition> levels;
    public int currentLevel;
    [SerializeField]
    private bool startMaxed;

    public static SuperManager GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        instance = GetComponent<SuperManager>();
        DontDestroyOnLoad(gameObject);
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
        else { Load(); }
    }

    // Update is called once per frame
    void Update()
    {

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
                Save();
                return true;
            }
        }
        return false;
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/saveData.dat"))
        {
            File.Delete(Application.persistentDataPath + "/saveData.dat");
        }
        FileStream file = File.Create(Application.persistentDataPath + "/saveData.dat");

        bf.Serialize(file, saveData);

        file.Close();
    }

    private void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/saveData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/saveData.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);
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
        Save();
    }

    private void StartNewGame()
    {
        saveData = new SaveData();
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
        
        Save();
    }

    public void ResetSaveData()
    {
        StartNewGame();
    }
}
