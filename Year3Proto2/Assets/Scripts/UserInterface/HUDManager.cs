﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    private float updateInterval = 0.667f;
    private float updateTimer;

    HorizontalLayoutGroup hLayoutGroup;
    private CanvasGroup canvas;
    private CanvasGroup villAllocCanvas;
    private TMP_Text buildButtonText;
    public bool doShowHUD = true;
    private bool hudShown;
    private bool buildMode = true;

    public Color gainColour;
    public Color lossColour;
    public Color fullColour;
    private GameManager game;
    private StructureManager structMan;
    private TMP_Text villagerText;
    private TMP_Text foodText;
    private TMP_Text woodText;
    private TMP_Text metalText;

    private float foodDeltaTimer;
    private Tooltip foodDeltaTip;
    private TMP_Text foodDeltaText;

    private float woodDeltaTimer;
    private Tooltip woodDeltaTip;
    private TMP_Text woodDeltaText;

    private float metalDeltaTimer;
    private Tooltip metalDeltaTip;
    private TMP_Text metalDeltaText;

    [SerializeField] private UIAnimator resourceBar;
    [SerializeField] private GameObject helpScreen;
    [SerializeField] private BuildPanel buildPanel;

    private EnemySpawner spawner;
    private TMP_Text victoryProgress;

    void Start()
    {
        villAllocCanvas = transform.Find("VillagerAllocataionWidgets").GetComponent<CanvasGroup>();
        villAllocCanvas.alpha = 0.0f;
        buildButtonText = transform.Find("BuildButton/Text").GetComponent<TMP_Text>();
        buildButtonText.text = "BUILDING";
        hLayoutGroup = transform.Find("ResourceBar/ResourceCards").GetComponent<HorizontalLayoutGroup>();
        canvas = GetComponent<CanvasGroup>();

        game = FindObjectOfType<GameManager>();
        structMan = FindObjectOfType<StructureManager>();
        spawner = FindObjectOfType<EnemySpawner>();

        villagerText = transform.Find("ResourceBar/ResourceCards/ResourceCardVillager/VillagerText").GetComponent<TMP_Text>();
        foodText = transform.Find("ResourceBar/ResourceCards/ResourceCardFood/FoodText").GetComponent<TMP_Text>();
        woodText = transform.Find("ResourceBar/ResourceCards/ResourceCardWood/WoodText").GetComponent<TMP_Text>();
        metalText = transform.Find("ResourceBar/ResourceCards/ResourceCardMetal/MetalText").GetComponent<TMP_Text>();
        victoryProgress = transform.Find("ResourceBar/VictoryProgress/ProgressText").GetComponent<TMP_Text>();

        foodDeltaTip = transform.Find("ResourceBar/ResourceCards/ResourceCardFood/FoodText/FoodIcon/FoodDelta").GetComponent<Tooltip>();
        foodDeltaText = transform.Find("ResourceBar/ResourceCards/ResourceCardFood/FoodText/FoodIcon/FoodDelta/FoodDeltaText").GetComponent<TMP_Text>();

        woodDeltaTip = transform.Find("ResourceBar/ResourceCards/ResourceCardWood/WoodText/WoodIcon/WoodDelta").GetComponent<Tooltip>();
        woodDeltaText = transform.Find("ResourceBar/ResourceCards/ResourceCardWood/WoodText/WoodIcon/WoodDelta/WoodDeltaText").GetComponent<TMP_Text>();

        metalDeltaTip = transform.Find("ResourceBar/ResourceCards/ResourceCardMetal/MetalText/MetalIcon/MetalDelta").GetComponent<Tooltip>();
        metalDeltaText = transform.Find("ResourceBar/ResourceCards/ResourceCardMetal/MetalText/MetalIcon/MetalDelta/MetalDeltaText").GetComponent<TMP_Text>();

        GetVictoryInfo();

        resourceBar.SetVisibility(!GlobalData.showTutorial);
        buildPanel.showPanel = !GlobalData.showTutorial;
        helpScreen.SetActive(GlobalData.showTutorial);
    }

    void LateUpdate()
    {
        if (doShowHUD && !hudShown)
        {
            ShowHUD();
            hudShown = true;
        }

        if (!doShowHUD && hudShown)
        {
            HideHUD();
            hudShown = false;
        }

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
        HorizontalLayoutGroup hLayoutGroup = transform.Find("ResourceBar/ResourceCards").GetComponent<HorizontalLayoutGroup>();
        hLayoutGroup.SetLayoutHorizontal();
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

    private void ShowHUD()
    {
        canvas.DOKill(true);
        canvas.DOFade(1.0f, 0.3f).SetEase(Ease.InOutSine);
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    private void HideHUD()
    {
        canvas.DOKill(true);
        canvas.DOFade(0.0f, 0.3f).SetEase(Ease.InOutSine);
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public void HideHelpScreen()
    {
        GlobalData.showTutorial = false;
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
            villAllocCanvas.DOFade(0.0f, 0.3f);
            SetAllVillagerWidgets(false);
            FindObjectOfType<BuildPanel>().showPanel = true;
            buildButtonText.text = "BUILDING";
        }
        else
        {
            SetAllVillagerWidgets(true);
            villAllocCanvas.DOFade(1.0f, 0.3f);
            FindObjectOfType<BuildPanel>().showPanel = false;
            buildButtonText.text = "VILLAGERS";
        }
    }

    public void ShowOneVillagerWidget(VillagerAllocation _widget)
    {
        if (buildMode)
        {
            SetAllVillagerWidgets(false);
            _widget.gameObject.SetActive(true);
            villAllocCanvas.DOFade(1.0f, 0.1f);
        }
    }

    public void HideAllVillagerWidgets()
    {
        if (buildMode)
        {
            villAllocCanvas.DOFade(0.0f, 0.1f);
        }
    }

    private void SetAllVillagerWidgets(bool _enabled)
    {
        VillagerAllocation[] widgets = Resources.FindObjectsOfTypeAll<VillagerAllocation>();
        foreach (VillagerAllocation widget in widgets)
        {
            widget.gameObject.SetActive(_enabled);
        }
    }
}
