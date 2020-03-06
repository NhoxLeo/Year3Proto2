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

    public Buildings tooltipSelected;
    private GameObject tooltipBox;
    private RectTransform toolTransform;
    private CanvasGroup toolCanvas;
    private TMP_Text tooltipHeading;
    private TMP_Text tooltipDescription;
    private float tooltipTimer;

    public Buildings buildingSelected;
    private GameObject buildIndicator;
    private GameObject buildTip;
    private RectTransform buildTipTransform;
    private CanvasGroup buildTipCanvas;
    private TMP_Text buildTipHeading;

    [Serializable]
    public struct TooltipInfo
    {
        public string[] heading;
        public string[] description;

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

    void Update()
    {
        //Vector2 mousePos = Input.mousePosition;
        //Vector3 toolPos = tooltipBox.transform.position;
        //toolPos.x = mousePos.x;
        //tooltipBox.transform.position = toolPos;

        if (tooltipSelected == Buildings.None)
        {
            tooltipTimer += Time.unscaledDeltaTime;
        }
        else
        {
            tooltipTimer = 0.0f;
        }
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

        if (tooltipSelected == Buildings.None)
        {
            HideTooltip();
        }
        else
        {
            ShowTooltip();

            tooltipHeading.text = toolInfo.heading[(int)tooltipSelected];
            tooltipDescription.text = toolInfo.description[(int)tooltipSelected];

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
            buildIndicator.SetActive(false);

            buildTipTransform.DOSizeDelta(new Vector2(64.0f, 76.0f), 0.25f).SetEase(Ease.OutQuint);
            buildTipCanvas.DOKill(true);
            buildTipCanvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);

            structMan.ResetBuilding();
        }
        else
        {
            structMan.ResetBuilding();
            if (structMan.SetBuilding(buildingSelected))
            {
                buildIndicator.SetActive(true);
                buildIndicator.transform.DOKill(true);
                buildIndicator.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.0f), 0.15f, 1, 0.5f);
                Vector2 targetPos = transform.Find("PanelMask").GetChild(buildingType + 7).transform.localPosition;
                Vector2 indiPos = buildIndicator.transform.localPosition;
                indiPos.x = targetPos.x;
                buildIndicator.transform.localPosition = indiPos;

                buildTip.transform.DOKill(true);
                buildTip.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f);
                buildTipTransform.DOSizeDelta(new Vector2(276.0f, 76.0f), 0.25f).SetEase(Ease.OutQuint);
                buildTipCanvas.DOKill(true);
                buildTipCanvas.DOFade(1.0f, 0.15f);
                buildTipHeading.text = toolInfo.heading[(int)buildingSelected];
            }
            else
            {
                buildingSelected = Buildings.None;
            }
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
