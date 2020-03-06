using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class BuildPanel : MonoBehaviour
{
    public bool showPanel;

    private CanvasGroup canvas;
    private RectTransform rTrans;

    private StructureManager structMan;
    public enum Buildings
    {
        None,
        Archer,
        Catapult,
        Farm,
        Silo,
        LumberMill,
        LumberPile,
        Mine,
        MetalStorage
    }

    public Buildings tooltipID;
    private GameObject tooltipBox;
    private RectTransform toolTransform;
    private CanvasGroup toolCanvas;
    private TMP_Text tooltipHeading;
    private TMP_Text tooltipDescription;

    public Buildings selectedBuilding;
    private GameObject buildIndicator;
    private GameObject buildTip;
    private RectTransform buildTipTransform;
    private CanvasGroup buildTipCanvas;
    private TMP_Text buildTipHeading;

    [Serializable]
    public struct TooltipInfo
    {
        [Header("Archer")]
        public string ArcherHeading;
        public string ArcherDescription;

        [Header("Catapult")]
        public string CatapultHeading;
        public string CatapultDescription;

        [Header("Farm")]
        public string FarmHeading;
        public string FarmDescription;

        [Header("FoodStorage")]
        public string FoodStorageHeading;
        public string FoodStorageDescription;

        [Header("LumberMill")]
        public string MillHeading;
        public string MillDescription;

        [Header("WoodStorage")]
        public string WoodStorageHeading;
        public string WoodStorageDescription;

        [Header("Mine")]
        public string MineHeading;
        public string MineDescription;

        [Header("OreStorage")]
        public string OreStorageHeading;
        public string OreStorageDescription;
    }
    [SerializeField]
    private TooltipInfo toolInfo;

    void Start()
    {
        canvas = GetComponent<CanvasGroup>();
        rTrans = GetComponent<RectTransform>();
        structMan = FindObjectOfType<StructureManager>();
        rTrans.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.0f);

        tooltipBox = transform.Find("BuildPanelTooltip").gameObject;
        tooltipHeading = transform.Find("BuildPanelTooltip/PanelMask/Heading").GetComponent<TMP_Text>();
        tooltipDescription = transform.Find("BuildPanelTooltip/PanelMask/Description").GetComponent<TMP_Text>();
        toolTransform = tooltipBox.GetComponent<RectTransform>();
        toolCanvas = tooltipBox.GetComponent<CanvasGroup>();
        toolTransform.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.0f);
        toolCanvas.alpha = 0.0f;

        buildIndicator = transform.Find("PanelMask/BuildingIndicator").gameObject;
        buildIndicator.SetActive(false);
        buildTip = GameObject.Find("SelectedBuilding");
        buildTipTransform = buildTip.GetComponent<RectTransform>();
        buildTipTransform.DOSizeDelta(new Vector2(64.0f, 76.0f), 0.0f);
        buildTipCanvas = buildTip.GetComponent<CanvasGroup>();
        buildTipHeading = buildTip.transform.Find("PanelMask/Heading").GetComponent<TMP_Text>();
    }


    void LateUpdate()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector3 toolPos = tooltipBox.transform.position;
        toolPos.x = mousePos.x;
        tooltipBox.transform.position = toolPos;
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
        tooltipID = (Buildings)tool;

        if (tooltipID == Buildings.None)
        {
            HideTooltip();
        }
        else
        {
            ShowTooltip();
        }

        switch (tooltipID)
        {
            case Buildings.Archer:
                tooltipHeading.text = toolInfo.ArcherHeading;
                tooltipDescription.text = toolInfo.ArcherDescription;
                break;

            case Buildings.Catapult:
                tooltipHeading.text = toolInfo.CatapultHeading;
                tooltipDescription.text = toolInfo.CatapultDescription;
                break;

            case Buildings.Farm:
                tooltipHeading.text = toolInfo.FarmHeading;
                tooltipDescription.text = toolInfo.FarmDescription;
                break;

            case Buildings.Silo:
                tooltipHeading.text = toolInfo.FoodStorageHeading;
                tooltipDescription.text = toolInfo.FoodStorageDescription;
                break;

            case Buildings.LumberMill:
                tooltipHeading.text = toolInfo.MillHeading;
                tooltipDescription.text = toolInfo.MillDescription;
                break;

            case Buildings.LumberPile:
                tooltipHeading.text = toolInfo.WoodStorageHeading;
                tooltipDescription.text = toolInfo.WoodStorageDescription;
                break;

            case Buildings.Mine:
                tooltipHeading.text = toolInfo.MineHeading;
                tooltipDescription.text = toolInfo.MineDescription;
                break;

            case Buildings.MetalStorage:
                tooltipHeading.text = toolInfo.OreStorageHeading;
                tooltipDescription.text = toolInfo.OreStorageDescription;
                break;

            default:
                break;
        }
    }

    public void SelectBuilding(int buildingType)
    {
        if ((Buildings)buildingType == selectedBuilding)
        {
            selectedBuilding = Buildings.None;
        }
        else
        {
            selectedBuilding = (Buildings)buildingType;
        }

        if (selectedBuilding == Buildings.None)
        {
            buildIndicator.SetActive(false);

            buildTipTransform.DOSizeDelta(new Vector2(64.0f, 76.0f), 0.25f).SetEase(Ease.OutQuint);
            buildTipCanvas.DOKill(true);
            buildTipCanvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);

            //structMan.RefundBuilding(selectedBuilding);
        }
        else
        {
            buildIndicator.SetActive(true);
            Vector2 targetPos = transform.Find("PanelMask").GetChild(buildingType + 7).transform.localPosition;
            Vector2 indiPos = buildIndicator.transform.localPosition;
            indiPos.x = targetPos.x;
            buildIndicator.transform.localPosition = indiPos;

            buildTipTransform.DOSizeDelta(new Vector2(276.0f, 76.0f), 0.25f).SetEase(Ease.OutQuint);
            buildTipCanvas.DOKill(true);
            buildTipCanvas.DOFade(1.0f, 0.15f);

            structMan.BuyBuilding(selectedBuilding);
        }

    }

    private void ShowTooltip()
    {
        toolTransform.DOSizeDelta(new Vector2(276.0f, 158.0f), 0.25f).SetEase(Ease.OutQuint);
        toolCanvas.DOKill(true);
        toolCanvas.DOFade(1.0f, 0.15f);
    }

    private void HideTooltip()
    {
        toolTransform.DOSizeDelta(new Vector2(64.0f, 158.0f), 0.25f).SetEase(Ease.OutQuint);
        toolCanvas.DOKill(true);
        toolCanvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);
    }
}
