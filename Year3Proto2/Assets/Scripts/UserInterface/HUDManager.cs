//
// Bachelor of Creative Technologies
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name        : HUDManager.cs
// Description      : Manages and updates info on the Heads Up Display
// Author           : David Morris
// Mail             : David.Mor7851@mediadesign.school.nz
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class HUDManager : MonoBehaviour
{
    private static HUDManager instance;

    private float updateInterval = 0.125f;
    private float updateTimer;

    UIAnimator animator;
    public bool doShowHUD = true;

    [Header("Resource Cards")]
    [SerializeField] private UIAnimator resourceBar;
    public Color gainColour;
    public Color lossColour;
    public Color fullColour;
    [SerializeField] private TMP_Text villagerText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] private RectTransform resourceBarTransform;

    [Header("Delta Popups")]
    [SerializeField] private Tooltip foodDeltaTip;
    [SerializeField] private TMP_Text foodDeltaText;
    private float foodDeltaTimer;
    [SerializeField] private Tooltip woodDeltaTip;
    [SerializeField] private TMP_Text woodDeltaText;
    private float woodDeltaTimer;
    [SerializeField] private Tooltip metalDeltaTip;
    [SerializeField] private TMP_Text metalDeltaText;
    private float metalDeltaTimer;

    [Header("Misc")]
    [SerializeField] private TMP_Text victoryProgress;
    [SerializeField] private Button villagerButton;
    [SerializeField] private TMP_Text villagerCost;
    [SerializeField] private TMP_Text nextWave;
    [SerializeField] private Transform villAlloc;
    [SerializeField] private UIAnimator helpScreen;
    [SerializeField] private RectTransform nextWaveTooltip;
    [SerializeField] private Button nextWaveButton;
    [SerializeField] private BuildPanel buildPanel;

    [SerializeField] private Toggle showVillagerWidgets;

    [SerializeField] private OptionCategoryObject defenceCategory;
    [SerializeField] private OptionCategoryObject resourceCategory;
    private int currentTab = 1;

    [Header("Pause Menu")]
    [SerializeField] private PauseMenu pauseMenu;

    private bool nextWaveUpdate = false;

    public static HUDManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        animator = GetComponent<UIAnimator>();
        RefreshResources();
        GetVictoryInfo();
        bool showTutorial = SuperManager.GetInstance().GetShowTutorial();
        //resourceBar.SetVisibility(!showTutorial);
        //buildPanel.showPanel = !showTutorial;
        //helpScreen.SetVisibility(showTutorial);
        //TutorialManager.GetInstance().AdvanceTutorialTo(showTutorial ? TutorialManager.TutorialState.Start : TutorialManager.TutorialState.End, true);
        showVillagerWidgets.isOn = SuperManager.GetInstance().GetShowWidgets();
        UpdateVillagerWidgetMode();
        LayoutRebuilder.ForceRebuildLayoutImmediate(resourceBarTransform);
    }

    void LateUpdate()
    {
        animator.SetVisibility(doShowHUD);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            if (!pauseMenu.isPaused && !pauseMenu.isHelp)
            {
                SetHUD(!doShowHUD);
            }
        }

        updateTimer -= Time.unscaledDeltaTime;
        if (updateTimer <= 0)
        {
            RefreshResources();
            GameManager gameMan = GameManager.GetInstance();
            gameMan.UpdateObjectiveText();
            gameMan.UpdateBuildPanel();
            updateTimer = updateInterval;
        }


        // Resource Deltas
        // Food
        if (foodDeltaTimer > 0.0f)
        {
            foodDeltaTimer -= Time.unscaledDeltaTime;
            foodDeltaTip.showTooltip = true;
        }
        else
        {
            foodDeltaTip.showTooltip = false;
        }
        // Wood
        if (woodDeltaTimer > 0.0f)
        {
            woodDeltaTimer -= Time.unscaledDeltaTime;
            woodDeltaTip.showTooltip = true;
        }
        else
        {
            woodDeltaTip.showTooltip = false;
        }
        // Metal
        if (metalDeltaTimer > 0.0f)
        {
            metalDeltaTimer -= Time.unscaledDeltaTime;
            metalDeltaTip.showTooltip = true;
        }
        else
        {
            metalDeltaTip.showTooltip = false;
        }

        EnemyManager enemyMan = EnemyManager.GetInstance();
        // Info Bar
        int wavesSurvived = enemyMan.GetWavesSurvived();
        string plural = (wavesSurvived == 1) ? "" : "s";
        victoryProgress.text = wavesSurvived.ToString() + " Invasion" + plural + " Survived";

        nextWaveButton.interactable = enemyMan.CanSpawnNextWave();
        if (nextWaveUpdate)
        {
            FetchNextWaveInfo();
        }
    }

    private void GetVictoryInfo()
    {
        List<MapScreen.Level> levels = new List<MapScreen.Level>();
        SuperManager superMan = SuperManager.GetInstance();
        superMan.GetLevelData(ref levels);
        int currentLevel = superMan.GetCurrentLevel();
        transform.Find("ResourceBar/LevelModCard/Title").GetComponent<TMP_Text>().text = levels[currentLevel].victoryTitle;
        transform.Find("ResourceBar/LevelModCard/Description").GetComponent<TMP_Text>().text = levels[currentLevel].victoryDescription;
        transform.Find("ResourceBar/LevelModCard/Price").GetComponent<TMP_Text>().text = levels[currentLevel].victoryValue.ToString();
    }

    public void SetVictoryInfo(string _title, string _description)
    {
        transform.Find("ResourceBar/LevelModCard/Title").GetComponent<TMP_Text>().text = _title;
        transform.Find("ResourceBar/LevelModCard/Description").GetComponent<TMP_Text>().text = _description;
    }

    public void RefreshResources()
    {
        GameManager gameMan = GameManager.GetInstance();
        VillagerManager villagerMan = VillagerManager.GetInstance();

        // available out of total
        string availableVillagers = villagerMan.GetAvailable().ToString("0");
        string villagers = villagerMan.GetVillagers().ToString("0");
        if (villagerText.text != availableVillagers + "/" + villagers)
        {
            villagerText.text = availableVillagers + "/" + villagers;
            LayoutRebuilder.ForceRebuildLayoutImmediate(resourceBarTransform);
        }

        Vector3 resources = gameMan.playerResources.GetResources();
        Vector3 capacity = gameMan.playerResources.GetCapacity();
        Vector3 velocity = gameMan.CalculateResourceVelocity();

        float foodVel = velocity.x;
        string foodVelDP = AddSign(Mathf.Round(foodVel));
        foodText.text = resources.x.ToString("0") + "/" + capacity.x.ToString("0") + " (" + foodVelDP + ")";
        foodText.color = (Mathf.Sign(foodVel) == 1) ? gainColour : lossColour;
        if (gameMan.playerResources.ResourceIsFull(ResourceType.Food))
        {
            foodText.color = fullColour;
        }

        float woodVel = velocity.y;
        string woodVelDP = AddSign(Mathf.Round(woodVel));
        woodText.text = resources.y.ToString("0") + "/" + capacity.y.ToString("0") + " (" + woodVelDP + ")";
        woodText.color = (Mathf.Sign(woodVel) == 1) ? gainColour : lossColour;
        if (gameMan.playerResources.ResourceIsFull(ResourceType.Wood))
        {
            woodText.color = fullColour;
        }

        float metalVel = velocity.z;
        string metalVelDP = AddSign(Mathf.Round(metalVel));
        metalText.text = resources.z.ToString("0") + "/" + capacity.z.ToString("0") + " (" + metalVelDP + ")";
        metalText.color = (Mathf.Sign(metalVel) == 1) ? gainColour : lossColour;
        if (gameMan.playerResources.ResourceIsFull(ResourceType.Metal))
        {
            metalText.color = fullColour;
        }

        // Update content size fitters
        //LayoutRebuilder.ForceRebuildLayoutImmediate(resourceBarTransform);

        // Update villager button interation and tooltip text color
        bool canAffordVillager = gameMan.playerResources.CanAfford(villagerMan.GetVillagerTrainCost());
        villagerButton.interactable = canAffordVillager;
        villagerCost.color = canAffordVillager ? gainColour : lossColour;
    }

    public void ShowResourceDelta(int _food, int _wood, int _metal)
    {
        if (_food != 0)
        {
            foodDeltaTimer = 1.5f;
            foodDeltaText.text = AddSign(_food);
            foodDeltaText.color = (_food > 0) ? gainColour : lossColour;
            foodDeltaTip.PulseTip();
        }

        if (_wood != 0)
        {
            woodDeltaTimer = 1.5f;
            woodDeltaText.text = AddSign(_wood);
            woodDeltaText.color = (_wood > 0) ? gainColour : lossColour;
            woodDeltaTip.PulseTip();
        }

        if (_metal != 0)
        {
            metalDeltaTimer = 1.5f;
            metalDeltaText.text = AddSign(_metal);
            metalDeltaText.color = (_metal > 0) ? gainColour : lossColour;
            metalDeltaTip.PulseTip();
        }


        RefreshResources();
    }

    public void ShowResourceDelta(ResourceBundle _resourceDelta, bool _makeNegative = false)
    {
        if (_makeNegative)
        {
            ShowResourceDelta((int)-_resourceDelta.food, (int)-_resourceDelta.wood, (int)-_resourceDelta.metal);
        }
        else
        {
            ShowResourceDelta((int)_resourceDelta.food, (int)_resourceDelta.wood, (int)_resourceDelta.metal);
        }
    }

    public void SetHUD(bool _active)
    {
        doShowHUD = _active;
    }

    public void SetOverUI(bool _isOver)
    {
        if (StructureManager.GetInstance() == null)
            return;

        StructureManager.GetInstance().SetIsOverUI(_isOver);
    }

    private string AddSign(float _value)
    {
        string _signedValue = (_value > 0) ? "+" : "";

        return _signedValue + _value;
    }

    public void HideHelpScreen()
    {
        SuperManager.GetInstance().SetShowTutorial(false);
        buildPanel.SetPanelVisibility(true);
    }

    public void UpdateVillagerWidgetMode()
    {
        SetVillagerWidgets(showVillagerWidgets.isOn);
    }

    private void SetVillagerWidgets(bool _villagers)
    {
        SuperManager.GetInstance().SetShowWidgets(_villagers);
        SetAllVillagerWidgets(_villagers);
    }

    public void SetVillagerWidgetVisibility(UIAnimator _widget, bool _visible)
    {
        _widget.SetVisibility(_visible);
    }

    public void HideAllVillagerWidgets()
    {
        SetAllVillagerWidgets(false);
    }

    private void SetAllVillagerWidgets(bool _enabled)
    {
        for (int i = 0; i < villAlloc.transform.childCount; i++)
        {
            SetVillagerWidgetVisibility(villAlloc.transform.GetChild(i).GetComponent<UIAnimator>(), _enabled);
        }
    }

    public void FetchVillagerInfo()
    {
        // Get info about the cost to train a Villager
        villagerCost.text = ((int)VillagerManager.GetInstance().GetVillagerTrainCost().food).ToString();
    }

    public void TrainVillager()
    {
        VillagerManager.GetInstance().TrainVillager();
        FetchVillagerInfo();
    }

    public void SetNextWaveUpdate(bool _update)
    {
        nextWaveUpdate = _update;
    }

    public void FetchNextWaveInfo()
    {
        EnemyManager enemyMan = EnemyManager.GetInstance();
        string time = "Time until the next wave spawns naturally: " + enemyMan.GetTime().ToString("0") + "s";
        string remaining = "";
        if (!enemyMan.CanSpawnNextWave())
        {
            remaining += "\n\nButton Cooldown: " + enemyMan.GetNextWaveWaitTime() + "s";
        }
        if (SuperManager.GetInstance().GetShowTutorial())
        {
            nextWave.text = "The enemy will start attacking once you have completed the tutorial. You cannot summon the next wave.";
        }
        else if(!enemyMan.spawning)
        {
            nextWave.text = "The enemy will start attacking when you start building. You cannot summon the next wave.";
        }
        else
        {
            nextWave.text = time + remaining;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(nextWaveTooltip);
    }

    public void NextWave()
    {
        EnemyManager enemyMan = EnemyManager.GetInstance();
        if (enemyMan.CanSpawnNextWave())
        {
            InfoManager.RecordNewAction();
            enemyMan.SpawnNextWave();
        }
    }

    public void SetCurrentTab(int _tab)
    {
        currentTab = _tab;
    }

    public void SwitchTabs()
    {
        if (currentTab == 0)
        {
            resourceCategory.SwitchTo();
            currentTab = 1;
        }
        else
        {
            defenceCategory.SwitchTo();
            currentTab = 0;
        }
    }

    public int GetCurrentTab()
    {
        return currentTab;
    }

    public void ToggleShowVillagers()
    {
        showVillagerWidgets.isOn = !showVillagerWidgets.isOn;
        UpdateVillagerWidgetMode();
    }
}
