using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct InfoManagerSaveData
{
    public int structuresPlaced;
    public int actionsTotal;
    public float matchDuration;
}

public class InfoManager : MonoBehaviour
{
    public static int ActionsTotal { get; private set; }
    public static int StructuresPlaced { get; private set; }
    public static float MatchDuration { get; private set; }
    public static float CurrentAPM 
    {
        get => (MatchDuration >= 60f) ? (ActionsTotal / (MatchDuration / 60f)) : -1f;
    }


    #region Unity Messages
    void Awake()
    {
        StructuresPlaced = 0;
        MatchDuration = 0;
    }

    void Update()
    {
        MatchDuration += Time.deltaTime;
    }
    #endregion

    public static void RecordNewAction()
    {
        ActionsTotal++;
    }

    public static void RecordNewStructurePlaced()
    {
        StructuresPlaced++;
    }

    public static string GetStatsDebugInfo()
    {
        string heading = "Player Stats:";
        string actionsTotal = "\nActions Count: " + ActionsTotal.ToString();
        string matchDuration = "\nMatch Duration: " + ((int)(MatchDuration * 100f) / 100f).ToString();
        string apm = "\nAPM: " + ((int)(CurrentAPM * 100f) / 100f).ToString();
        string structuresPlaced = "\nStructures Placed: " + StructuresPlaced.ToString();
        return heading + actionsTotal + matchDuration + apm + structuresPlaced;
    }

    public static void LoadSaveData(InfoManagerSaveData _saveData)
    {
        StructuresPlaced = _saveData.structuresPlaced;
        ActionsTotal = _saveData.actionsTotal;
        MatchDuration = _saveData.matchDuration;
    }

    public static InfoManagerSaveData GenerateSaveData()
    {
        return new InfoManagerSaveData()
        {
            structuresPlaced = StructuresPlaced,
            actionsTotal = ActionsTotal,
            matchDuration = MatchDuration
        };
    }
}
