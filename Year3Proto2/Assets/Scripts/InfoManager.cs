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
    public List<(float, SuperManager.SaveVector3)> resourcesSpentRecently;
    public float timeSkipped;
    public float tileBonusAverage;
}

public class InfoManager : MonoBehaviour
{
    private const float InfoUpdateDelay = 0.33f;
    private static float InfoUpdateTimer = 0f;
    private static int InfoUpdateTarget = 0;
    private static List<(float, SuperManager.SaveVector3)> ResourcesSpentRecently;
    private static List<float> VillagersLost;
    private static List<float> StructuresLost;

    public static int ActionsTotal { get; private set; }
    public static int StructuresPlaced { get; private set; }
    public static float MatchDuration { get; private set; }
    public static float CalculatedRecentSpent { get; private set; }
    public static float TimeSkipped { get; private set; }
    public static float TileBonusAverage { get; private set; }
    public static float VillagersLostGradual { get; private set; }
    public static float StructuresLostGradual { get; private set; }


    public static float CurrentAPM 
    {
        get => (MatchDuration >= 60f) ? (ActionsTotal / (MatchDuration / 60f)) : -1f;
    }


    #region Unity Messages
    void Awake()
    {
        StructuresPlaced = 0;
        MatchDuration = 0;
        TimeSkipped = 0;
        ActionsTotal = 0;
        TileBonusAverage = 0;
        ResourcesSpentRecently = new List<(float, SuperManager.SaveVector3)>();
        VillagersLost = new List<float>();
        StructuresLost = new List<float>();
    }

    void Update()
    {
        MatchDuration += Time.deltaTime;
        InfoUpdateTimer -= Time.deltaTime;
        if (InfoUpdateTimer <= 0f)
        {
            InfoUpdateTimer = InfoUpdateDelay;
            switch (InfoUpdateTarget)
            {
                case 0:
                    ResourcesSpentRecently.RemoveAll(match => Time.time - match.Item1 >= 60f);
                    CalculatedRecentSpent = 0f;
                    foreach ((float, SuperManager.SaveVector3) element in ResourcesSpentRecently)
                    {
                        float timeMultiplier = (60f - (Time.time - element.Item1)) * 0.0167f;
                        CalculatedRecentSpent += element.Item2.x * timeMultiplier;
                        CalculatedRecentSpent += element.Item2.y * timeMultiplier;
                        CalculatedRecentSpent += element.Item2.z * timeMultiplier;
                    }
                    InfoUpdateTarget = 1;
                    break;
                case 1:
                    VillagersLost.RemoveAll(match => Time.time - match >= 60f);
                    VillagersLostGradual = 0f;
                    foreach (float villagerLost in VillagersLost)
                    {
                        VillagersLostGradual += (60f - (Time.time - villagerLost)) * 0.0167f;
                    }
                    InfoUpdateTarget = 2;
                    break;
                case 2:
                    StructuresLost.RemoveAll(match => Time.time - match >= 60f);
                    StructuresLostGradual = 0f;
                    foreach (float structureLost in StructuresLost)
                    {
                        StructuresLostGradual += (60f - (Time.time - structureLost)) * 0.0167f;
                    }
                    InfoUpdateTarget = 0;
                    break;
            }

        }
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

    public static void RecordResourcesSpent(Vector3 _resourcesSpent)
    {
        ResourcesSpentRecently.Add((Time.time, new SuperManager.SaveVector3(_resourcesSpent)));
    }

    public static void RecordTimeSkipped(float _time)
    {
        TimeSkipped += _time;
    }

    public static void RecordTileBonusAverage(float _average)
    {
        TileBonusAverage = _average;
    }

    public static void RecordVillagerDeath(int _villagerCount)
    {
        for (int i = 0; i < _villagerCount; i++)
        {
            VillagersLost.Add(Time.time);
        }
    }

    public static void RecordStructureDestroyed()
    {
        StructuresLost.Add(Time.time);
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
        ActionsTotal = _saveData.actionsTotal;
        StructuresPlaced = _saveData.structuresPlaced;
        MatchDuration = _saveData.matchDuration;
        ResourcesSpentRecently = _saveData.resourcesSpentRecently;
        TimeSkipped = _saveData.timeSkipped;
        TileBonusAverage = _saveData.tileBonusAverage;
    }

    public static InfoManagerSaveData GenerateSaveData()
    {
        return new InfoManagerSaveData()
        {
            actionsTotal = ActionsTotal,
            structuresPlaced = StructuresPlaced,
            matchDuration = MatchDuration,
            resourcesSpentRecently = ResourcesSpentRecently,
            timeSkipped = TimeSkipped,
            tileBonusAverage = TileBonusAverage
        };
    }
}
