using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public struct PlayerResources
{
    private int food;
    private int foodMax;
    private int metal;
    private int metalMax;
    private int wood;
    private int woodMax;
    public PlayerResources(int _startAmount, int _maxAmount)
    {
        wood = _startAmount;
        metal = _startAmount;
        food = _startAmount;

        woodMax = _maxAmount;
        metalMax = _maxAmount;
        foodMax = _maxAmount;
    }

    public void AddBatch(ResourceBatch _batch)
    {
        switch (_batch.type)
        {
            case ResourceType.Wood:
                wood += _batch.amount;
                if (wood > woodMax) { wood = woodMax; }
                break;
            case ResourceType.Metal:
                metal += _batch.amount;
                if (metal > metalMax) { metal = metalMax; }
                break;
            case ResourceType.Food:
                food += _batch.amount;
                if (food > foodMax) { food = foodMax; }
                if (food < 0)
                {
                    VillagerManager.GetInstance().AddStarveTicks(-food);
                    food = 0;
                }
                break;
        }
    }

    public bool AttemptPurchase(ResourceBundle _cost)
    {
        if (CanAfford(_cost))
        {
            wood -= _cost.woodCost;
            metal -= _cost.metalCost;
            food -= _cost.foodCost;
            return true;
        }
        else return false;
    }

    public bool CanAfford(ResourceBundle _cost)
    {
        return (wood >= _cost.woodCost || _cost.woodCost <= 0) && (metal >= _cost.metalCost || _cost.metalCost <= 0) && (food >= _cost.foodCost || _cost.foodCost <= 0);
    }

    public void DeductResource(ResourceType _type, int _deduction)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                wood -= _deduction;
                break;
            case ResourceType.Metal:
                metal -= _deduction;
                break;
            case ResourceType.Food:
                food -= _deduction;
                break;
        }
    }

    public void DeductResourceBundle(ResourceBundle _bundle)
    {
        wood -= _bundle.woodCost;
        metal -= _bundle.metalCost;
        food -= _bundle.foodCost;
    }

    public void AddResourceBundle(ResourceBundle _bundle)
    {
        wood += _bundle.woodCost;
        metal += _bundle.metalCost;
        food += _bundle.foodCost;
    }

    public int Get(ResourceType _type)
    {
        int value = 0;
        switch (_type)
        {
            case ResourceType.Wood:
                value = wood;
                break;
            case ResourceType.Metal:
                value = metal;
                break;
            case ResourceType.Food:
                value = food;
                break;
        }
        return value;
    }

    public int GetResourceMax(ResourceType _type)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                return woodMax;
            case ResourceType.Metal:
                return metalMax;
            case ResourceType.Food:
                return foodMax;
            default:
                break;
        }
        return 0;
    }

    public bool ResourceIsFull(ResourceType _type)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                return (wood == woodMax);
            case ResourceType.Metal:
                return (metal == metalMax);
            case ResourceType.Food:
                return (food == foodMax);
            default:
                break;
        }
        return false;
    }

    public void SetMaximum(ResourceType _type, int _newMax)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                woodMax = _newMax;
                break;
            case ResourceType.Metal:
                metalMax = _newMax;
                break;
            case ResourceType.Food:
                foodMax = _newMax;
                break;
        }
    }

    public bool AllGreaterOrEqualTo(int _amount)
    {
        return wood >= _amount && metal >= _amount && food >= _amount;
    }
}

public class ResourceBatch
{
    public float age;
    public int amount;
    public ResourceType type;

    public ResourceBatch(int _amount, ResourceType _type)
    {
        age = 0f;
        amount = _amount;
        type = _type;
    }

    public void AddTime(float _time)
    {
        age += _time;
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [HideInInspector]
    public PlayerResources playerResources;
    [HideInInspector]
    public bool tutorialDone = false;
    private static Dictionary<string, AudioClip> audioClips;
    private float batchMaxAge = 3.0f;
    private bool gameover = false;
    [HideInInspector]
    public bool victory = false;
    private float gameoverTimer = 5.0f;
    private float tutorialAMessageTimer = 5.0f;
    private float tutorialBMessageTimer = 3.0f;
    private float tutorialDelay = 2.0f;
    private bool musicBackOn = false;
    private bool switchingScene = false;
    private float musicDelay = 3.0f;
    private static int repairCount = 0;
    private MessageBox messageBox;
    private BuildPanel buildPanel;
    [HideInInspector]
    public bool repairMessage = false;
    [HideInInspector]
    public bool longhausDead;
    [HideInInspector]
    public bool repairAll = false;
    private float volumeFull;
    private float panelRefreshTimer = 0.0f;
    private float panelRefreshCooldown = 0.5f;
    [HideInInspector]
    public bool gameAlreadyWon = false;
    public int objectivesCompleted = 0;
    public List<int> objectives;
    public int foodSinceObjective = 0;
    public int lumberSinceObjective = 0;
    public int metalSinceObjective = 0;

    public static GameManager GetInstance()
    {
        return instance;
    }

    public static void CreateAudioEffect(string _sfxName, Vector3 _positon, float _volume = 1.0f, bool _spatial = true, float _dopplerLevel = 0f)
    {
        GameObject spawnAudio = new GameObject("TemporarySoundObject");
        spawnAudio.transform.position = _positon;
        AudioSource spawnAudioComp = spawnAudio.AddComponent<AudioSource>();
        DestroyMe spawnAudioDestroy = spawnAudio.AddComponent<DestroyMe>();
        spawnAudioDestroy.SetLifetime(audioClips[_sfxName].length);
        spawnAudioComp.spatialBlend = _spatial ? 1.0f : 0.0f;
        spawnAudioComp.dopplerLevel = _dopplerLevel;
        spawnAudioComp.rolloffMode = AudioRolloffMode.Linear;
        spawnAudioComp.maxDistance = 100f;
        spawnAudioComp.clip = audioClips[_sfxName];
        spawnAudioComp.Play();
        spawnAudioComp.volume = _volume;
    }

    public static void IncrementRepairCount()
    {
        repairCount++;
    }

    public void AddBatch(ResourceBatch _newBatch)
    {
        playerResources.AddBatch(_newBatch);
        switch (_newBatch.type)
        {
            case ResourceType.Wood:
                lumberSinceObjective += _newBatch.amount;
                break;
            case ResourceType.Metal:
                metalSinceObjective += _newBatch.amount;
                break;
            case ResourceType.Food:
                foodSinceObjective += _newBatch.amount;
                break;
            default:
                break;
        }
    }

    public void CalculateStorageMaximum()
    {
        int newWoodMax = Longhaus.woodStorage;
        int newMetalMax = Longhaus.metalStorage;
        int newFoodMax = Longhaus.foodStorage;

        // gets every structure and adds to find totals.
        StorageStructure[] storageStructures = FindObjectsOfType<StorageStructure>();
        foreach (StorageStructure storageStructure in storageStructures)
        {
            if (storageStructure.GetHealth() > 0f)
            {
                switch (storageStructure.GetResourceType())
                {
                    case ResourceType.Wood:
                        newWoodMax += storageStructure.storage;
                        break;
                    case ResourceType.Metal:
                        newMetalMax += storageStructure.storage;
                        break;
                    case ResourceType.Food:
                        newFoodMax += storageStructure.storage;
                        break;
                }
            }
        }

        playerResources.SetMaximum(ResourceType.Wood, newWoodMax);
        playerResources.SetMaximum(ResourceType.Metal, newMetalMax);
        playerResources.SetMaximum(ResourceType.Food, newFoodMax);
    }


    // wood metal food
    public Vector3 GetResourceVelocity()
    {
        Vector3 resourceVelocity = Vector3.zero;

        foreach (Structure structure in FindObjectsOfType<Structure>())
        {
            resourceVelocity += structure.GetResourceDelta();
        }

        return resourceVelocity;
    }

    public void OnStructurePlaced()
    {
        CalculateStorageMaximum();

        foreach (ResourceStructure resourceStructure in FindObjectsOfType<ResourceStructure>())
        {
            resourceStructure.OnPlace();
        }

        // Update all enemies
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.SetState(EnemyState.Idle);
        }
    }

    public void RepairAll()
    {
        bool repairAll = true;
        bool repairsWereDone = false;
        bool repairsWereFailed = false;
        ResourceBundle total = new ResourceBundle(0, 0, 0);
        Structure[] structures = FindObjectsOfType<Structure>();
        foreach (Structure structure in structures)
        {
            if (structure.GetStructureType() != StructureType.Environment)
            {
                ResourceBundle repairCost = structure.RepairCost();
                bool repaired = structure.Repair(true);
                if (repaired) 
                { 
                    structure.HideHealthbar(); 
                    total += repairCost; 
                    repairsWereDone = true; 
                }
                else if (!repairCost.IsEmpty())
                {
                    repairsWereFailed = true;
                    repairAll = false;
                }
            }
        }
        if (repairsWereDone)
        {
            if (repairAll)
            {
                if (tutorialDone) { messageBox.ShowMessage("All repairs done!", 1f); }
            }
            else if (repairsWereFailed)
            {
                if (tutorialDone) { messageBox.ShowMessage("Couldn't repair everything...", 2f); }
            }
            HUDManager.GetInstance().ShowResourceDelta(total, true);
        }
        else
        {
            if (repairsWereFailed)
            {
                if (tutorialDone) { messageBox.ShowMessage("Couldn't repair anything...", 2f); }
            }
            else
            {
                if (tutorialDone) { messageBox.ShowMessage("There was nothing to repair...", 2f); }
            }
        }
        
    }

    private void Awake()
    {
        instance = this;
        audioClips = new Dictionary<string, AudioClip>
        {
            { "horn", Resources.Load("Audio/SFX/sfxHorn") as AudioClip },
            { "build", Resources.Load("Audio/SFX/sfxBuild") as AudioClip },
            { "arrow", Resources.Load("Audio/SFX/sfxArrow") as AudioClip },
            { "buildingHit", Resources.Load("Audio/SFX/sfxBuildingHit") as AudioClip },
            { "buildingDestroy", Resources.Load("Audio/SFX/sfxBuildingDestroy") as AudioClip },
            { "catapultFire", Resources.Load("Audio/SFX/sfxCatapultFire") as AudioClip },
            { "lose", Resources.Load("Audio/SFX/sfxLose") as AudioClip },
            { "win", Resources.Load("Audio/SFX/sfxWin") as AudioClip },
            { "UIclick1", Resources.Load("Audio/SFX/sfxUIClick1") as AudioClip },
            { "UIclick2", Resources.Load("Audio/SFX/sfxUIClick2") as AudioClip },
            { "UIclick3", Resources.Load("Audio/SFX/sfxUIClick3") as AudioClip },
            { "UItap", Resources.Load("Audio/SFX/sfxUITap") as AudioClip },
            { "ResourceLoss", Resources.Load("Audio/SFX/sfxResourceLoss") as AudioClip },
            { "Explosion", Resources.Load("Audio/SFX/sfxExplosion") as AudioClip },
            { "Zap", Resources.Load("Audio/SFX/sfxLightning") as AudioClip },
            { "Thud", Resources.Load("Audio/SFX/sfxShockwave") as AudioClip },
        };        objectives = SuperManager.GetInstance().GetCurrentWinConditions();
    }

    // Start is called before the first frame update
    void Start()
    {
        SuperManager superMan = SuperManager.GetInstance();
        playerResources = new PlayerResources(200, 500);
        CalculateStorageMaximum();
        messageBox = FindObjectOfType<MessageBox>();
        buildPanel = FindObjectOfType<BuildPanel>();
        volumeFull = GetComponents<AudioSource>()[0].volume;
        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierHealth))
        {
            Soldier.SetMaxHealth(1.5f * Soldier.GetMaxHealth());
        }
        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierDamage))
        {
            Soldier.SetDamage(1.3f * Soldier.GetDamage());
        }
        if (superMan.GetResearchComplete(SuperManager.BarracksSoldierSpeed))
        {
            Soldier.SetMovementSpeed(1.3f * Soldier.GetMovementSpeed());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            repairAll = true;
            RepairAll();
        }

        if (!tutorialDone)
        {
            tutorialDelay -= Time.deltaTime;
            if (tutorialDelay <= 0f)
            {
                if (tutorialAMessageTimer == 5f)
                {
                    //messageBox.ShowMessage("Your goal is to collect 3000 of each resource...", 4.5f);
                }
                tutorialAMessageTimer -= Time.deltaTime;
                if (tutorialAMessageTimer <= 0f)
                {
                    if (tutorialBMessageTimer == 3f)
                    {
                        //messageBox.ShowMessage("...good luck, have fun!", 3f);
                    }
                    tutorialBMessageTimer -= Time.deltaTime;
                    if (tutorialBMessageTimer <= 0f)
                    {
                        tutorialDone = true;
                    }
                }
            }
        }

        if (repairCount > 5 && !repairMessage && !repairAll)
        {
            messageBox.ShowMessage("You can press R to mass repair", 3f);
            if (messageBox.GetCurrentMessage() == "You can press R to mass repair") { repairMessage = true; }
        }

        panelRefreshTimer -= Time.deltaTime;
        if (panelRefreshTimer <= 0f)
        {
            panelRefreshTimer = panelRefreshCooldown;
            // do refresh
            for (int i = 1; i <= 12; i++)
            {
                BuildPanel.Buildings buildingI = (BuildPanel.Buildings)i;

                switch (buildingI)
                {
                    case BuildPanel.Buildings.Catapult:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.Catapult))
                        {
                            continue;
                        }
                        break;
                    case BuildPanel.Buildings.Barracks:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.Barracks))
                        {
                            continue;
                        }
                        break;
                    case BuildPanel.Buildings.FreezeTower:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTower))
                        {
                            continue;
                        }
                        break;
                    case BuildPanel.Buildings.ShockwaveTower:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTower))
                        {
                            continue;
                        }
                        break;
                    case BuildPanel.Buildings.LightningTower:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.LightningTower))
                        {
                            continue;
                        }
                        break;
                    default:
                        break;
                }
                ResourceBundle cost = StructureManager.GetInstance().structureCosts[StructureNames.BuildPanelToString(buildingI)];
                bool playerCanAfford = playerResources.CanAfford(cost);
                Color colour = playerCanAfford ? Color.white : buildPanel.cannotAfford;
                buildPanel.SetButtonColour(buildingI, colour);
            }
        }

        if (!gameover)
        {
            if (longhausDead)
            {
                gameover = true;
                victory = false;
                messageBox.ShowMessage("You Lost!", 3f);
                GetComponents<AudioSource>()[0].DOFade(0f, 1f);
                CreateAudioEffect("lose", Vector3.zero, 1f, false);
            }
            else
            {
                if (CheckNextWinConditionIsMet())
                {
                    objectivesCompleted++;
                    EnemyManager.GetInstance().OnObjectiveComplete();
                    foodSinceObjective = 0;
                    lumberSinceObjective = 0;
                    metalSinceObjective = 0;
                }
                bool gameAlreadyWon = false;
                SuperManager.MatchSaveData match = SuperManager.GetInstance().GetSavedMatch();
                if (match.match)
                {
                    if (match.matchWon)
                    {
                        gameAlreadyWon = true;
                    }
                }
                if (AllObjectivesCompleted() && !gameAlreadyWon)
                {
                    gameover = true;
                    victory = true;
                    SuperManager.GetInstance().OnLevelComplete();
                    messageBox.ShowMessage("You Win!", 5f);
                    GetComponents<AudioSource>()[0].DOFade(0f, 1f);
                    CreateAudioEffect("win", Vector3.zero, 1f, false);
                }
            }            
        }

        if (objectivesCompleted < objectives.Count)
        {
            string completion = "(" + (objectivesCompleted).ToString() + "/" + objectives.Count.ToString() + ") ";
            string name = completion + SuperManager.WinConditionDefinitions[objectives[objectivesCompleted]].name;
            string desc = SuperManager.WinConditionDefinitions[objectives[objectivesCompleted]].description;
            string objCompletion = "";
            switch (objectives[objectivesCompleted])
            {
                case SuperManager.Slaughter:
                    objCompletion = " (" + EnemyManager.GetInstance().GetEnemiesKilled().ToString() + "/20)";
                    break;
                case SuperManager.SlaughterII:
                    objCompletion = " (" + EnemyManager.GetInstance().GetEnemiesKilled().ToString() + "/50)";
                    break;
                case SuperManager.SlaughterIII:
                    objCompletion = " (" + EnemyManager.GetInstance().GetEnemiesKilled().ToString() + "/100)";
                    break;
                case SuperManager.Food:
                    objCompletion = " (" + foodSinceObjective.ToString() + "/1000)";
                    break;
                case SuperManager.FoodII:
                    objCompletion = " (" + foodSinceObjective.ToString() + "/2000)";
                    break;
                case SuperManager.FoodIII:
                    objCompletion = " (" + foodSinceObjective.ToString() + "/3000)";
                    break;
                case SuperManager.Lumber:
                    objCompletion = " (" + lumberSinceObjective.ToString() + "/1000)";
                    break;
                case SuperManager.LumberII:
                    objCompletion = " (" + lumberSinceObjective.ToString() + "/2000)";
                    break;
                case SuperManager.LumberIII:
                    objCompletion = " (" + lumberSinceObjective.ToString() + "/3000)";
                    break;
                case SuperManager.Metal:
                    objCompletion = " (" + metalSinceObjective.ToString() + "/1000)";
                    break;
                case SuperManager.MetalII:
                    objCompletion = " (" + metalSinceObjective.ToString() + "/2000)";
                    break;
                case SuperManager.MetalIII:
                    objCompletion = " (" + metalSinceObjective.ToString() + "/3000)";
                    break;
                default:
                    break;
            }

            HUDManager.GetInstance().SetVictoryInfo(name, desc + objCompletion);
        }
        else if (objectivesCompleted == objectives.Count)
        {
            HUDManager.GetInstance().SetVictoryInfo("(" + objectivesCompleted.ToString() + "/" + objectivesCompleted.ToString() + ") All Objectives Completed!", "Well done, you are now in freeplay.");
        }

        if (gameover)
        {
            if (victory)
            {
                if (musicDelay == 3f)
                {
                    FindObjectOfType<LevelEndscreen>().ShowVictoryScreen();
                }
                musicDelay -= Time.deltaTime;
                if (musicDelay < 0f && !musicBackOn)
                {
                    GetComponents<AudioSource>()[0].DOFade(volumeFull, 2f);
                    musicBackOn = true;
                }
            }
            else
            {
                if (gameoverTimer == 5f)
                {
                    FindObjectOfType<LevelEndscreen>().ShowDeafeatScreen();
                }
                gameoverTimer -= Time.deltaTime;
                if (gameoverTimer < 0f && !switchingScene)
                {
                    //FindObjectOfType<SceneSwitcher>().SceneSwitch("TitleScreen");
                    switchingScene = true;
                }
            }
        }
    }
    public void SaveMatch()
    {
        SuperManager.GetInstance().SaveCurrentMatch();
    }

    public void OnRestart()
    {
        SuperManager superMan = SuperManager.GetInstance();
        superMan.ClearCurrentMatch();
        superMan.PlayLevel(superMan.GetCurrentLevel());
    }

    public bool AllObjectivesCompleted()
    {
        return objectives.Count == objectivesCompleted;
    }

    private bool CheckNextWinConditionIsMet()
    {
        if (objectives.Count > objectivesCompleted)
        {
            int currentWinCondition = objectives[objectivesCompleted];
            switch (currentWinCondition)
            {
                case SuperManager.Accumulate:
                    return playerResources.AllGreaterOrEqualTo(1500);
                case SuperManager.AccumulateII:
                    return playerResources.AllGreaterOrEqualTo(2500);
                case SuperManager.AccumulateIII:
                    return playerResources.AllGreaterOrEqualTo(5000);
                case SuperManager.Slaughter:
                    return EnemyManager.GetInstance().GetEnemiesKilled() >= 20;
                case SuperManager.SlaughterII:
                    return EnemyManager.GetInstance().GetEnemiesKilled() >= 50;
                case SuperManager.SlaughterIII:
                    return EnemyManager.GetInstance().GetEnemiesKilled() >= 100;
                case SuperManager.Survive:
                    return EnemyManager.GetInstance().GetWaveSurvived(5);
                case SuperManager.SurviveII:
                    return EnemyManager.GetInstance().GetWaveSurvived(10);
                case SuperManager.SurviveIII:
                    return EnemyManager.GetInstance().GetWaveSurvived(15);
                case SuperManager.Food:
                    return foodSinceObjective >= 1000;
                case SuperManager.FoodII:
                    return foodSinceObjective >= 2000;
                case SuperManager.FoodIII:
                    return foodSinceObjective >= 3000;
                case SuperManager.Lumber:
                    return lumberSinceObjective >= 1000;
                case SuperManager.LumberII:
                    return lumberSinceObjective >= 2000;
                case SuperManager.LumberIII:
                    return lumberSinceObjective >= 3000;
                case SuperManager.Metal:
                    return metalSinceObjective >= 1000;
                case SuperManager.MetalII:
                    return metalSinceObjective >= 2000;
                case SuperManager.MetalIII:
                    return metalSinceObjective >= 3000;
                case SuperManager.Villagers:
                    return VillagerManager.GetInstance().GetVillagers() >= 15;
                case SuperManager.VillagersII:
                    return VillagerManager.GetInstance().GetVillagers() >= 35;
                case SuperManager.VillagersIII:
                    return VillagerManager.GetInstance().GetVillagers() >= 65;
                default:
                    break;
            }
        }
        return false;
    }
}
