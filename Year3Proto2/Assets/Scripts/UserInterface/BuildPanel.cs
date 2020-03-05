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

    public enum Tooltip
    {
        None,
        Archer,
        Catapult,
        Farm,
        Silo,
        LumberMill,
        LumberPile,
        Mine,
        OreStorage
    }
    public Tooltip tooltipID;
    private GameObject tooltipBox;
    private RectTransform toolTransform;
    private CanvasGroup toolCanvas;
    private TMP_Text tooltipHeading;
    private TMP_Text tooltipDescription;

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
        rTrans.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.0f);

        tooltipBox = transform.Find("BuildPanelTooltip").gameObject;
        tooltipHeading = transform.Find("BuildPanelTooltip/PanelMask/Heading").GetComponent<TMP_Text>();
        tooltipDescription = transform.Find("BuildPanelTooltip/PanelMask/Description").GetComponent<TMP_Text>();
        toolTransform = tooltipBox.GetComponent<RectTransform>();
        toolCanvas = tooltipBox.GetComponent<CanvasGroup>();
        toolTransform.DOSizeDelta(new Vector2(64.0f, 212.0f), 0.0f);
        toolCanvas.alpha = 0.0f;
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
        tooltipID = (Tooltip)tool;

        if (tooltipID == Tooltip.None)
        {
            HideTooltip();
        }
        else
        {
            ShowTooltip();
        }

        switch (tooltipID)
        {
            case Tooltip.Archer:
                tooltipHeading.text = toolInfo.ArcherHeading;
                tooltipDescription.text = toolInfo.ArcherDescription;
                break;

            case Tooltip.Catapult:
                tooltipHeading.text = toolInfo.CatapultHeading;
                tooltipDescription.text = toolInfo.CatapultDescription;
                break;

            case Tooltip.Farm:
                tooltipHeading.text = toolInfo.FarmHeading;
                tooltipDescription.text = toolInfo.FarmDescription;
                break;

            case Tooltip.Silo:
                tooltipHeading.text = toolInfo.FoodStorageHeading;
                tooltipDescription.text = toolInfo.FoodStorageDescription;
                break;

            case Tooltip.LumberMill:
                tooltipHeading.text = toolInfo.MillHeading;
                tooltipDescription.text = toolInfo.MillDescription;
                break;

            case Tooltip.LumberPile:
                tooltipHeading.text = toolInfo.WoodStorageHeading;
                tooltipDescription.text = toolInfo.WoodStorageDescription;
                break;

            case Tooltip.Mine:
                tooltipHeading.text = toolInfo.MineHeading;
                tooltipDescription.text = toolInfo.MineDescription;
                break;

            case Tooltip.OreStorage:
                tooltipHeading.text = toolInfo.OreStorageHeading;
                tooltipDescription.text = toolInfo.OreStorageDescription;
                break;

            default:
                break;
        }
    }

    private void ShowTooltip()
    {
        //tooltipBox.SetActive(true);
        toolTransform.DOSizeDelta(new Vector2(276.0f, 158.0f), 0.25f).SetEase(Ease.OutQuint);
        toolCanvas.DOKill(true);
        toolCanvas.DOFade(1.0f, 0.15f);

        Debug.Log("Showing Tooltip");
    }

    private void HideTooltip()
    {
        //tooltipBox.SetActive(false);
        toolTransform.DOSizeDelta(new Vector2(64.0f, 158.0f), 0.25f).SetEase(Ease.OutQuint);
        toolCanvas.DOKill(true);
        toolCanvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);

        Debug.Log("Hiding Tooltip");
    }
}
