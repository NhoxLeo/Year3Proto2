using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

[Serializable]
public struct PlayerResources
{
    private int rFood;
    private int rFoodMax;
    private int rMetal;
    private int rMetalMax;
    private int rWood;
    private int rWoodMax;
    public PlayerResources(int _startAmount, int _maxAmount)
    {
        rWood = _startAmount;
        rMetal = _startAmount;
        rFood = _startAmount;

        rWoodMax = _maxAmount;
        rMetalMax = _maxAmount;
        rFoodMax = _maxAmount;
    }

    public void AddBatch(ResourceBatch _batch)
    {
        switch (_batch.type)
        {
            case ResourceType.Wood:
                rWood += _batch.amount;
                if (rWood > rWoodMax) { rWood = rWoodMax; }
                break;
            case ResourceType.Metal:
                rMetal += _batch.amount;
                if (rMetal > rMetalMax) { rMetal = rMetalMax; }
                break;
            case ResourceType.Food:
                rFood += _batch.amount;
                if (rFood > rFoodMax) { rFood = rFoodMax; }
                if (rFood < 0)
                {
                    VillagerManager.GetInstance().AddStarveTicks(-rFood);
                    rFood = 0;
                }
                break;
        }
    }

    public bool AttemptPurchase(ResourceBundle _cost)
    {
        if (CanAfford(_cost))
        {
            rWood -= _cost.woodCost;
            rMetal -= _cost.metalCost;
            rFood -= _cost.foodCost;
            return true;
        }
        else return false;
    }

    public bool CanAfford(ResourceBundle _cost)
    {
        return (rWood >= _cost.woodCost || _cost.woodCost <= 0) && (rMetal >= _cost.metalCost || _cost.metalCost <= 0) && (rFood >= _cost.foodCost || _cost.foodCost <= 0);
    }

    public void DeductResource(ResourceType _type, int _deduction)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                rWood -= _deduction;
                break;
            case ResourceType.Metal:
                rMetal -= _deduction;
                break;
            case ResourceType.Food:
                rFood -= _deduction;
                break;
        }
    }

    public void DeductResourceBundle(ResourceBundle _bundle)
    {
        rWood -= _bundle.woodCost;
        rMetal -= _bundle.metalCost;
        rFood -= _bundle.foodCost;
    }

    public void AddResourceBundle(ResourceBundle _bundle)
    {
        rWood += _bundle.woodCost;
        rMetal += _bundle.metalCost;
        rFood += _bundle.foodCost;
    }

    public int Get(ResourceType _type)
    {
        int value = 0;
        switch (_type)
        {
            case ResourceType.Wood:
                value = rWood;
                break;
            case ResourceType.Metal:
                value = rMetal;
                break;
            case ResourceType.Food:
                value = rFood;
                break;
        }
        return value;
    }

    public int GetResourceMax(ResourceType _type)
    {
        switch (_type)
        {
            case ResourceType.Wood:
                return rWoodMax;
            case ResourceType.Metal:
                return rMetalMax;
            case ResourceType.Food:
                return rFoodMax;
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
                return (rWood == rWoodMax);
            case ResourceType.Metal:
                return (rMetal == rMetalMax);
            case ResourceType.Food:
                return (rFood == rFoodMax);
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
                rWoodMax = _newMax;
                break;
            case ResourceType.Metal:
                rMetalMax = _newMax;
                break;
            case ResourceType.Food:
                rFoodMax = _newMax;
                break;
        }
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
    private List<ResourceBatch> recentBatches;
    private float tutorialAMessageTimer = 5.0f;
    private float tutorialBMessageTimer = 3.0f;
    private float tutorialDelay = 2.0f;
    private bool musicBackOn = false;
    private bool switchingScene = false;
    private float musicDelay = 3.0f;
    private static int repairCount = 0;
    private MessageBox messageBox;
    private HUDManager HUDMan;
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
        recentBatches.Add(_newBatch);
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
            HUDMan.ShowResourceDelta(total, true);
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
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        SuperManager superMan = SuperManager.GetInstance();
        playerResources = new PlayerResources(200, 500);
        CalculateStorageMaximum();
        recentBatches = new List<ResourceBatch>();
        messageBox = FindObjectOfType<MessageBox>();
        HUDMan = FindObjectOfType<HUDManager>();
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

        List<ResourceBatch> batchesToRemove = new List<ResourceBatch>();
        for (int i = 0; i < recentBatches.Count; i++)
        {
            recentBatches[i].AddTime(Time.deltaTime);
            if (recentBatches[i].age >= batchMaxAge)
            {
                batchesToRemove.Add(recentBatches[i]);
            }
        }
        foreach (ResourceBatch batch in batchesToRemove)
        {
            recentBatches.Remove(batch);
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
            for (int i = 1; i <= 9; i++)
            {
                if ((BuildPanel.Buildings)i == BuildPanel.Buildings.Catapult)
                {
                    if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.Catapult))
                    {
                        continue;
                    }
                }
                if ((BuildPanel.Buildings)i == BuildPanel.Buildings.Barracks)
                {
                    if (!SuperManager.GetInstance().GetResearchComplete(SuperManager.Barracks))
                    {
                        continue;
                    }
                }
                ResourceBundle cost = StructureManager.GetInstance().structureCosts[StructureManager.StructureNames[(BuildPanel.Buildings)i]];
                bool playerCanAfford = playerResources.CanAfford(cost);
                Color colour = playerCanAfford ? Color.white : buildPanel.cannotAfford;
                buildPanel.SetButtonColour((BuildPanel.Buildings)i, colour);
            }
        }

        if (!gameover)
        {
            if (longhausDead == true)
            {
                gameover = true;
                victory = false;
                messageBox.ShowMessage("You Lost!", 3f);
                GetComponents<AudioSource>()[0].DOFade(0f, 1f);
                CreateAudioEffect("lose", Vector3.zero, 1f, false);
            }
            else if (WinConditionIsMet() && !SuperManager.GetInstance().GetSavedMatch().matchWon)
            {
                gameover = true;
                victory = true;
                SuperManager.GetInstance().OnLevelComplete();
                messageBox.ShowMessage("You Win!", 5f);
                GetComponents<AudioSource>()[0].DOFade(0f, 1f);
                CreateAudioEffect("win", Vector3.zero, 1f, false);
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
        SuperManager.GetInstance().ClearCurrentMatch();
    }

    public bool WinConditionIsMet()
    {
        switch (SuperManager.GetInstance().GetCurrentWinCondition())
        {
            case SuperManager.Accumulate:
                return playerResources.Get(ResourceType.Metal) >= 1500 && playerResources.Get(ResourceType.Food) >= 1500 && playerResources.Get(ResourceType.Wood) >= 1500;
            case SuperManager.AccumulateII:
                return playerResources.Get(ResourceType.Metal) >= 2500 && playerResources.Get(ResourceType.Food) >= 2500 && playerResources.Get(ResourceType.Wood) >= 2500;
            case SuperManager.AccumulateIII:
                return playerResources.Get(ResourceType.Metal) >= 7500 && playerResources.Get(ResourceType.Food) >= 7500 && playerResources.Get(ResourceType.Wood) >= 7500;
            case SuperManager.Slaughter:
                return EnemyManager.GetInstance().GetEnemiesKilled() > 300;
            case SuperManager.SlaughterII:
                return EnemyManager.GetInstance().GetEnemiesKilled() > 800;
            case SuperManager.SlaughterIII:
                return EnemyManager.GetInstance().GetEnemiesKilled() > 2000;
            case SuperManager.Survive:
                return EnemyManager.GetInstance().GetWaveCurrent() == 25 && EnemyManager.GetInstance().GetEnemiesAlive() == 0 || EnemyManager.GetInstance().GetWaveCurrent() > 25;
            case SuperManager.SurviveII:
                return EnemyManager.GetInstance().GetWaveCurrent() == 50 && EnemyManager.GetInstance().GetEnemiesAlive() == 0 || EnemyManager.GetInstance().GetWaveCurrent() > 50;
            case SuperManager.SurviveIII:
                return EnemyManager.GetInstance().GetWaveCurrent() == 100 && EnemyManager.GetInstance().GetEnemiesAlive() == 0 || EnemyManager.GetInstance().GetWaveCurrent() > 100;
            default:
                break;
        }
        return false;
    }
}
