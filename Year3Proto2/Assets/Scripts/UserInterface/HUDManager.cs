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
    private float updateInterval = 0.5f;
    private float updateTimer;

    UIAnimator animator;
    public bool doShowHUD = true;
    private bool buildMode = true;

    [Header("Resource Cards")]
    [SerializeField] private UIAnimator resourceBar;
    public Color gainColour;
    public Color lossColour;
    public Color fullColour;
    private GameManager game;
    private StructureManager structMan;
    [SerializeField] private TMP_Text villagerText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text metalText;
    [SerializeField] HorizontalLayoutGroup hLayoutGroup;
    [SerializeField] HorizontalLayoutGroup foodCard;
    [SerializeField] HorizontalLayoutGroup woodCard;
    [SerializeField] HorizontalLayoutGroup metalCard;

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
    [SerializeField] private Transform villAlloc;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private BuildPanel buildPanel;

    private EnemySpawner spawner;

    void Start()
    {
        animator = GetComponent<UIAnimator>();

        game = FindObjectOfType<GameManager>();
        structMan = FindObjectOfType<StructureManager>();
        spawner = FindObjectOfType<EnemySpawner>();

        RefreshResources();
        GetVictoryInfo();
        bool showTutorial = SuperManager.GetInstance().GetShowTutorial();
        resourceBar.SetVisibility(!showTutorial);
        buildPanel.showPanel = !showTutorial;
        helpScreen.SetActive(showTutorial);
    }

    void LateUpdate()
    {
        animator.SetVisibility(doShowHUD);

        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            doShowHUD = !doShowHUD;
        }

        updateTimer -= Time.unscaledDeltaTime;
        if (updateTimer <= 0)
        {
            RefreshResources();
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


        // Info Bar

        int wavesSurvived = Mathf.Clamp(spawner.GetWaveCurrent() - 1, 0, 999);
        if (spawner.GetWaveCurrent() >= 1 && spawner.enemyCount == 0) { wavesSurvived++; }
        string plural = (wavesSurvived == 1) ? "" : "s";
        victoryProgress.text = wavesSurvived.ToString() + " Invasion" + plural + " Survived";
    }

    private void GetVictoryInfo()
    {
        List<MapScreen.Level> levels = new List<MapScreen.Level>();
        SuperManager superMan = SuperManager.GetInstance();
        superMan.GetLevelData(ref levels);

        transform.Find("ResourceBar/LevelModCard/Title").GetComponent<TMP_Text>().text = levels[superMan.currentLevel].victoryTitle;
        transform.Find("ResourceBar/LevelModCard/Description").GetComponent<TMP_Text>().text = levels[superMan.currentLevel].victoryDescription;
        transform.Find("ResourceBar/LevelModCard/Price").GetComponent<TMP_Text>().text = levels[superMan.currentLevel].victoryValue.ToString();
    }

    public void RefreshResources()
    {
        // available out of total
        string availableVillagers = Longhaus.GetAvailable().ToString("0");
        string villagers = Longhaus.GetVillagers().ToString("0");
        villagerText.text = availableVillagers + "/" + villagers;

        Vector3 velocity = game.GetResourceVelocity();

        float foodVel = velocity.z;
        string foodVelDP = AddSign(Mathf.Round(foodVel * 10f) * .1f);
        foodText.text = game.playerResources.Get(ResourceType.Food).ToString() + "/" + game.playerResources.GetResourceMax(ResourceType.Food).ToString() + " (" + foodVelDP + "/s)";
        foodText.color = (Mathf.Sign(foodVel) == 1) ? gainColour : lossColour;
        if (game.playerResources.ResourceIsFull(ResourceType.Food))
        {
            foodText.color = fullColour;
        }

        float woodVel = velocity.x;
        string woodVelDP = AddSign(Mathf.Round(woodVel * 10f) * .1f);
        woodText.text = game.playerResources.Get(ResourceType.Wood).ToString() + "/" + game.playerResources.GetResourceMax(ResourceType.Wood).ToString() + " (" + woodVelDP + "/s)";
        woodText.color = (Mathf.Sign(woodVel) == 1) ? gainColour : lossColour;
        if (game.playerResources.ResourceIsFull(ResourceType.Wood))
        {
            woodText.color = fullColour;
        }

        float metalVel = velocity.y;
        string metalVelDP = AddSign(Mathf.Round(metalVel * 10f) * .1f);
        metalText.text = game.playerResources.Get(ResourceType.Metal).ToString() + "/" + game.playerResources.GetResourceMax(ResourceType.Metal).ToString() + " (" + metalVelDP + "/s)";
        metalText.color = (Mathf.Sign(metalVel) == 1) ? gainColour : lossColour;
        if (game.playerResources.ResourceIsFull(ResourceType.Metal))
        {
            metalText.color = fullColour;
        }

        // Update content size fitters
        Canvas.ForceUpdateCanvases();
        hLayoutGroup.SetLayoutHorizontal();
        foodCard.SetLayoutHorizontal();
        woodCard.SetLayoutHorizontal();
        metalCard.SetLayoutHorizontal();
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
            ShowResourceDelta(-_resourceDelta.foodCost, -_resourceDelta.woodCost, -_resourceDelta.metalCost);
        }
        else
        {
            ShowResourceDelta(_resourceDelta.foodCost, _resourceDelta.woodCost, _resourceDelta.metalCost);
        }
    }

    public void SetOverUI(bool _isOver)
    {
        if (structMan == null)
            return;

        structMan.SetIsOverUI(_isOver);
    }

    private string AddSign(float _value)
    {
        string _signedValue = (_value > 0) ? "+" : "";

        return _signedValue + _value;
    }

    public void HideHelpScreen()
    {
        SuperManager.GetInstance().SetShowTutorial(false);
        Invoke("DisableHelpScreen", 2.0f);
    }

    private void DisableHelpScreen()
    {
        helpScreen.SetActive(false);
    }

    public void ToggleHUDMode()
    {
        buildMode = !buildMode;

        if (buildMode)
        {
            SetAllVillagerWidgets(false);
        }
        else
        {
            SetAllVillagerWidgets(true);
        }
    }

    public void ShowOneVillagerWidget(VillagerAllocation _widget)
    {
        if (buildMode)
        {
            SetAllVillagerWidgets(false);
        }

        for (int i = 0; i < villAlloc.transform.childCount; i++)
        {
            UIAnimator widgetAnimatorAll = villAlloc.transform.GetChild(i).GetComponent<UIAnimator>();
            UIAnimator widgetAnimator = _widget.gameObject.GetComponent<UIAnimator>();
            widgetAnimator.SetVisibility(true);
            if (widgetAnimatorAll != widgetAnimator)
            {
                widgetAnimatorAll.SetVisibility(false);
            }
        }
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
            Debug.Log(villAlloc.transform.childCount);
    }
}
