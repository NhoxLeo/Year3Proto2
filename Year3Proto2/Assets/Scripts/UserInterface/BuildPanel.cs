﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BuildPanel : MonoBehaviour
{
    public bool showPanel;
    [SerializeField]
    public Color cannotAfford;

    private CanvasGroup canvas;
    private RectTransform rTrans;

    private StructureManager structMan;

    [Serializable]
    public enum Buildings
    {
        None,
        Archer,
        Catapult,
        Farm,
        Granary,
        LumberMill,
        LumberPile,
        Mine,
        MetalStorage
    }

    public Buildings tooltipSelected;
    private GameObject tooltipBox;
    private Tooltip tooltip;
    private TMP_Text tooltipHeading;
    private TMP_Text tooltipDescription;
    private TMP_Text woodCostText;
    private TMP_Text metalCostText;
    private float tooltipTimer;

    public Buildings buildingSelected;
    private GameObject buildIndicator;
    private GameObject buildTipBox;
    private Tooltip buildTip;
    private TMP_Text buildTipHeading;
    private SuperManager superMan;

    [Serializable]
    public struct TooltipInfo
    {
        public string[] heading;
        public string[] description;
        public Vector2[] cost;

    }
    [SerializeField]
    private TooltipInfo toolInfo;

    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        rTrans = GetComponent<RectTransform>();
        structMan = FindObjectOfType<StructureManager>();
        superMan = SuperManager.GetInstance();
        rTrans.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.0f);

        tooltipBox = transform.Find("BuildPanelTooltip").gameObject;
        tooltip = tooltipBox.GetComponent<Tooltip>();
        tooltipHeading = transform.Find("BuildPanelTooltip/PanelMask/Heading").GetComponent<TMP_Text>();
        tooltipDescription = transform.Find("BuildPanelTooltip/PanelMask/Description").GetComponent<TMP_Text>();
        woodCostText = transform.Find("BuildPanelTooltip/PanelMask/CostValueWood").GetComponent<TMP_Text>();
        metalCostText = transform.Find("BuildPanelTooltip/PanelMask/CostValueMetal").GetComponent<TMP_Text>();

        buildIndicator = transform.Find("PanelMask/BuildingIndicator").gameObject;
        buildIndicator.SetActive(false);
        buildTipBox = GameObject.Find("SelectedBuilding");
        buildTip = buildTipBox.GetComponent<Tooltip>();
        buildTipHeading = buildTipBox.transform.Find("PanelMask/Heading").GetComponent<TMP_Text>();

        //if (!superMan.saveData.research[0]) { transform.Find("PanelMask/IconOreStorage").GetComponent<Image>().color = locked; }
        //if (!superMan.saveData.research[1]) { transform.Find("PanelMask/IconCatapult").GetComponent<Image>().color = locked; }
    }

    public void SetButtonColour(Buildings _button, Color _colour)
    {
        switch (_button)
        {
            case Buildings.Archer:
                transform.Find("PanelMask/IconArcher").GetComponent<Image>().color = _colour;
                break;
            case Buildings.Catapult:
                transform.Find("PanelMask/IconCatapult").GetComponent<Image>().color = _colour;
                break;
            case Buildings.Farm:
                transform.Find("PanelMask/IconFarm").GetComponent<Image>().color = _colour;
                break;
            case Buildings.Granary:
                transform.Find("PanelMask/IconSilo").GetComponent<Image>().color = _colour;
                break;
            case Buildings.LumberMill:
                transform.Find("PanelMask/IconLumberMill").GetComponent<Image>().color = _colour;
                break;
            case Buildings.LumberPile:
                transform.Find("PanelMask/IconLumberPile").GetComponent<Image>().color = _colour;
                break;
            case Buildings.Mine:
                transform.Find("PanelMask/IconMine").GetComponent<Image>().color = _colour;
                break;
            case Buildings.MetalStorage:
                transform.Find("PanelMask/IconOreStorage").GetComponent<Image>().color = _colour;
                break;
            default:
                break;
        }
    }

    void Update()
    {

        if (tooltipSelected == Buildings.None)
        {
            tooltipTimer += Time.unscaledDeltaTime;
        }
        else
        {
            tooltipTimer = 0.0f;
        }
    }

    public TooltipInfo GetToolInfo()
    {
        return toolInfo;
    }

    public void TogglePanel()
    {
        if (!showPanel)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    public void ShowPanel()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.0f), 0.275f, 1, 0.5f);

        canvas.DOKill(true);
        canvas.DOFade(1.0f, 0.2f);
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
        rTrans.DOSizeDelta(new Vector2(1136.0f, 212.0f), 0.4f).SetEase(Ease.OutQuint);

        showPanel = true;
    }

    public void HidePanel()
    {
        canvas.DOKill(true);
        canvas.DOFade(0.0f, 0.2f);
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        rTrans.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.4f).SetEase(Ease.OutQuint);

        showPanel = false;
    }

    public void SetTooltip(int tool)
    {
        tooltipSelected = (Buildings)tool;

        if (tooltipSelected == Buildings.MetalStorage && !superMan.saveData.research[0]) { return; }
        if (tooltipSelected == Buildings.Catapult && !superMan.saveData.research[1]) { return; }

        if (tooltipSelected == Buildings.None)
        {
            tooltip.showTooltip = false;
        }
        else
        {
            tooltip.showTooltip = true;

            tooltipHeading.text = toolInfo.heading[(int)tooltipSelected];
            tooltipDescription.text = toolInfo.description[(int)tooltipSelected];

            int woodCost = (int)toolInfo.cost[(int)tooltipSelected].x;
            int metalCost = (int)toolInfo.cost[(int)tooltipSelected].y;

            woodCostText.text = woodCost.ToString();
            metalCostText.text = metalCost.ToString();

            metalCostText.gameObject.SetActive((metalCost > 0));
            

            float targetPos = transform.Find("PanelMask").GetChild(tool + 7).transform.localPosition.x;
            if (tooltipTimer > 0.15f)
            {
                tooltipBox.transform.DOLocalMoveX(targetPos, 0.0f);
            }
            else
            {
                tooltipBox.transform.DOLocalMoveX(targetPos, 0.15f).SetEase(Ease.OutQuint);
            }

        }
    }

    public void SelectBuilding(int buildingType)
    {
        if ((Buildings)buildingType == Buildings.MetalStorage && !superMan.saveData.research[0]) { return; }
        if ((Buildings)buildingType == Buildings.Catapult && !superMan.saveData.research[1]) { return; }

        if ((Buildings)buildingType == buildingSelected)
        {
            buildingSelected = Buildings.None;
        }
        else
        {
            buildingSelected = (Buildings)buildingType;
        }

        if (buildingSelected == Buildings.None)
        {
            buildTip.showTooltip = false;

            UINoneSelected();
            structMan.ResetBuilding();
        }
        else
        {
            structMan.ResetBuilding();
            if (structMan.SetBuilding(buildingSelected))
            {
                buildIndicator.SetActive(true);
                buildIndicator.transform.DOKill(true);
                buildIndicator.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.0f), 0.15f, 1, 0.5f);
                Vector2 targetPos = transform.Find("PanelMask").GetChild(buildingType + 7).transform.localPosition;
                Vector2 indiPos = buildIndicator.transform.localPosition;
                indiPos.x = targetPos.x;
                buildIndicator.transform.localPosition = indiPos;

                buildTip.showTooltip = true;
                buildTip.PulseTip();
                buildTipHeading.text = toolInfo.heading[(int)buildingSelected];
            }
            else
            {
                buildingSelected = Buildings.None;
            }
        }

    }

    public void UINoneSelected()
    {
        buildIndicator.SetActive(false);

        buildTip.showTooltip = false;
    }

    public void ResetBuildingSelected()
    {
        buildingSelected = Buildings.None;
    }
}
