using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public struct PlayerResources
{
    private float food;
    private float foodMax;
    private float metal;
    private float metalMax;
    private float wood;
    private float woodMax;
    public PlayerResources(float _startAmount, float _maxAmount)
    {
        wood = _startAmount;
        metal = _startAmount;
        food = _startAmount;

        woodMax = _maxAmount;
        metalMax = _maxAmount;
        foodMax = _maxAmount;
    }

    public bool AttemptPurchase(ResourceBundle _cost)
    {
        if (CanAfford(_cost))
        {
            wood -= _cost.wood;
            metal -= _cost.metal;
            food -= _cost.food;
            return true;
        }
        else return false;
    }

    public bool CanAfford(ResourceBundle _cost)
    {
        return (wood >= _cost.wood || _cost.wood <= 0) && (metal >= _cost.metal || _cost.metal <= 0) && (food >= _cost.food || _cost.food <= 0);
    }

    public void DeductResourceBundle(ResourceBundle _bundle)
    {
        wood -= _bundle.wood;
        metal -= _bundle.metal;
        food -= _bundle.food;
    }

    public void AddResourceBundle(ResourceBundle _bundle)
    {
        food += _bundle.food;
        if (food > foodMax) { food = foodMax; }
        if (food < 0)
        {
            VillagerManager.GetInstance().AddStarveTicks(-food);
            food = 0; 
        }


        wood += _bundle.wood;
        if (wood > woodMax) { wood = woodMax; }
        if (wood < 0) { wood = 0; }

        metal += _bundle.metal;
        if (metal > metalMax) { metal = metalMax; }
        if (metal < 0) { metal = 0; }
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

    public void SetMaximum(ResourceType _type, float _newMax)
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

    public Vector3 GetResources()
    {
        return new Vector3(food, wood, metal);
    }

    public Vector3 GetCapacity()
    {
        return new Vector3(foodMax, woodMax, metalMax);
    }

    public void CheatSetMaxed()
    {
        food = foodMax;
        wood = woodMax;
        metal = metalMax;
    }
}

public enum SoundType
{
    SoundEffect,
    Music,
    Ambient
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
    public bool repairAll = false;
    private float volumeFull;
    private float panelRefreshTimer = 0.0f;
    private float panelRefreshCooldown = 0.5f;
    [HideInInspector]
    public bool gameAlreadyWon = false;
    public int objectivesCompleted = 0;
    public List<int> objectives;
    public float foodSinceObjective = 0;
    public float lumberSinceObjective = 0;
    public float metalSinceObjective = 0;
    public bool cheatAlwaysMaxed = false;
    public static bool ShowEnemyHealthbars = true;

    private AudioSource music;
    private AudioSource ambience;
    private float musicVolume;
    private float ambienceVolume;
    private static GameObject ExplosionOne = null;
    private static GameObject ExplosionTwo = null;

    public static GameManager GetInstance()
    {
        return instance;
    }

    public static GameObject GetExplosion(int _variant)
    {
        if (_variant == 1)
        {
            if (!ExplosionOne)
            {
                ExplosionOne = Resources.Load("Explosion") as GameObject;
            }
            return ExplosionOne;
        }
        else
        {
            if (!ExplosionTwo)
            {
                ExplosionTwo = Resources.Load("ExplosionPetard") as GameObject;
            }
            return ExplosionTwo;
        }
    }

    public static void CreateAudioEffect(string _sfxName, Vector3 _positon, SoundType _type = SoundType.SoundEffect, float _volume = 1.0f, bool _spatial = true, float _dopplerLevel = 0f)
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
        switch (_type)
        {
            case SoundType.SoundEffect:
            spawnAudioComp.volume = _volume * SuperManager.EffectsVolume;
                break;
            case SoundType.Music:
            spawnAudioComp.volume = _volume * SuperManager.MusicVolume;
                break;
            case SoundType.Ambient:
            spawnAudioComp.volume = _volume * SuperManager.AmbientVolume;
                break;
            default:
                break;
        }
        spawnAudioComp.Play();
    }

    public static void IncrementRepairCount()
    {
        repairCount++;
    }

    public void CalculateStorageMaximum()
    {
        int newWoodMax = Longhaus.woodStorage;
        int newMetalMax = Longhaus.metalStorage;
        int newFoodMax = Longhaus.foodStorage;

        // gets every structure and adds to find totals.
        foreach (StorageStructure storageStructure in FindObjectsOfType<StorageStructure>())
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

        foreach (Structure structure in StructureManager.GetInstance().GetPlayerStructures())
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
        };
        objectives = SuperManager.GetInstance().GetCurrentWinConditions();
        music = GetComponents<AudioSource>()[0];
        ambience = GetComponents<AudioSource>()[1];
        musicVolume = music.volume;
        ambienceVolume = ambience.volume;
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
        CheckControls();
        // get resourceDelta
        ResourceBundle resourcesThisFrame = new ResourceBundle(GetResourceVelocity() * Time.deltaTime);
        foodSinceObjective += Mathf.Clamp(resourcesThisFrame.food, 0f, resourcesThisFrame.food);
        lumberSinceObjective += Mathf.Clamp(resourcesThisFrame.wood, 0f, resourcesThisFrame.wood);
        metalSinceObjective += Mathf.Clamp(resourcesThisFrame.metal, 0f, resourcesThisFrame.metal);
        playerResources.AddResourceBundle(resourcesThisFrame);


        // Hold both mouse buttons
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (SuperManager.DevMode)
            {
                // Press M
                if (Input.GetKeyDown(KeyCode.M))
                {
                    cheatAlwaysMaxed = !cheatAlwaysMaxed;
                }
            }
        }

        if (SuperManager.DevMode && cheatAlwaysMaxed)
        {
            playerResources.CheatSetMaxed();
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
                    case BuildPanel.Buildings.Ballista:
                        if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.Ballista))
                        {
                            continue;
                        }
                        break;
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
            if (GlobalData.longhausDead)
            {
                gameover = true;
                victory = false;
                messageBox.ShowMessage("You Lost!", 3f);
                music.DOFade(0f, 0.5f);
                CreateAudioEffect("lose", Vector3.zero, SoundType.Music, 1f, false);
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
                    music.DOFade(0f, 0.5f);
                    CreateAudioEffect("win", Vector3.zero, SoundType.Music, 1f, false);
                }
            }            
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
                    music.DOFade(musicVolume * SuperManager.MusicVolume, 2f);
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
        

        if (!gameover || musicDelay < -2f)
        {
            music.volume = musicVolume * SuperManager.MusicVolume;
            ambience.volume = ambienceVolume * SuperManager.AmbientVolume;
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

    public void UpdateObjectiveText()
    {
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
                    objCompletion = " (" + ((int)foodSinceObjective).ToString() + "/1000)";
                    break;
                case SuperManager.FoodII:
                    objCompletion = " (" + ((int)foodSinceObjective).ToString() + "/2000)";
                    break;
                case SuperManager.FoodIII:
                    objCompletion = " (" + ((int)foodSinceObjective).ToString() + "/3000)";
                    break;
                case SuperManager.Lumber:
                    objCompletion = " (" + ((int)lumberSinceObjective).ToString() + "/1000)";
                    break;
                case SuperManager.LumberII:
                    objCompletion = " (" + ((int)lumberSinceObjective).ToString() + "/2000)";
                    break;
                case SuperManager.LumberIII:
                    objCompletion = " (" + ((int)lumberSinceObjective).ToString() + "/3000)";
                    break;
                case SuperManager.Metal:
                    objCompletion = " (" + ((int)metalSinceObjective).ToString() + "/1000)";
                    break;
                case SuperManager.MetalII:
                    objCompletion = " (" + ((int)metalSinceObjective).ToString() + "/2000)";
                    break;
                case SuperManager.MetalIII:
                    objCompletion = " (" + ((int)metalSinceObjective).ToString() + "/3000)";
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
    }

    private void CheckControls()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            repairAll = true;
            RepairAll();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowEnemyHealthbars = !ShowEnemyHealthbars;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HUDManager.GetInstance().SwitchTabs();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            HUDManager.GetInstance().ToggleShowVillagers();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            VillagerManager.GetInstance().TrainVillager();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 3 : 7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 1 : 8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 2 : 9);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 4 : 10);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 5 : 11);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            buildPanel.SelectBuilding(HUDManager.GetInstance().GetCurrentTab() == 0 ? 6 : 12);
        }
    }

    private bool CheckNextWinConditionIsMet()
    {
        if (objectives.Count > objectivesCompleted)
        {
            int currentWinCondition = objectives[objectivesCompleted];
            switch (currentWinCondition)
            {
                case SuperManager.Accumulate:
                    return playerResources.CanAfford(new ResourceBundle(1500f, 1500f, 1500f));
                case SuperManager.AccumulateII:
                    return playerResources.CanAfford(new ResourceBundle(2500f, 2500f, 2500f));
                case SuperManager.AccumulateIII:
                    return playerResources.CanAfford(new ResourceBundle(5000f, 5000f, 5000f));
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

    public bool GetGameLost()
    {
        return gameover && !victory;
    }
}
