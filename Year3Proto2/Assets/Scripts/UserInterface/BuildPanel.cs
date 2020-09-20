using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour
{
    public bool showPanel = true;
    private bool panelShown = true;

    [SerializeField]
    public Color cannotAfford;

    public Sprite lockedBuilding;

    private CanvasGroup canvas;
    private float yPos;

    private StructureManager structMan;

    [Serializable]
    public enum Buildings
    {
        None,
        Ballista,
        Catapult,
        Barracks,
        FreezeTower,
        LigtningTower,
        ShockwaveTower,
        Farm,
        LumberMill,
        Mine,
        Granary,
        LumberPile,
        MetalStorage
    }

    public Buildings tooltipSelected;
    [SerializeField] private GameObject tooltipBox;
    [SerializeField] private Tooltip tooltip;
    [SerializeField] private TMP_Text tooltipHeading;
    [SerializeField] private TMP_Text tooltipDescription;
    [SerializeField] private TMP_Text woodCostText;
    [SerializeField] private TMP_Text metalCostText;
    public float tooltipTimer;

    public Buildings buildingSelected;
    private GameObject buildTipBox;
    private Tooltip buildTip;
    private TMP_Text buildTipHeading;
    private SuperManager superMan;

    public BuildingButton hoveredButton;

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
        GetInfo();

        yPos = transform.localPosition.y;
        structMan = StructureManager.GetInstance();
        superMan = SuperManager.GetInstance();

        buildTipBox = GameObject.Find("SelectedBuilding");
        buildTip = buildTipBox.GetComponent<Tooltip>();
        buildTipHeading = buildTipBox.transform.Find("PanelMask/Heading").GetComponent<TMP_Text>();


        if (!superMan.GetResearchComplete(SuperManager.Catapult)) { transform.Find("PanelMask/Content/DefenceBuildings/IconCatapult").GetComponent<Image>().sprite = lockedBuilding; }
        if (!superMan.GetResearchComplete(SuperManager.Barracks)) { transform.Find("PanelMask/Content/DefenceBuildings/IconBarracks").GetComponent<Image>().sprite = lockedBuilding; }
    }

    public void SetButtonColour(Buildings _button, Color _colour)
    {
        switch (_button)
        {
            case Buildings.Ballista:
                transform.Find("PanelMask/Content/DefenceBuildings/IconArcher").GetComponent<Image>().color = _colour;
                break;

            case Buildings.Catapult:
                transform.Find("PanelMask/Content/DefenceBuildings/IconCatapult").GetComponent<Image>().color = _colour;
                break;

            case Buildings.Barracks:
                transform.Find("PanelMask/Content/DefenceBuildings/IconBarracks").GetComponent<Image>().color = _colour;
                break;
            case Buildings.FreezeTower:
                transform.Find("PanelMask/IconFreezeTower").GetComponent<Image>().color = _colour;
                break;
            case Buildings.ShockwaveTower:
                transform.Find("PanelMask/IconShockwaveTower").GetComponent<Image>().color = _colour;
                break;
            case Buildings.LigtningTower:
                transform.Find("PanelMask/IconLightningTower").GetComponent<Image>().color = _colour;
                break;
            case Buildings.Farm:
                transform.Find("PanelMask/Content/ResourceBuildings/IconFarm").GetComponent<Image>().color = _colour;
                break;

            case Buildings.Granary:
                transform.Find("PanelMask/Content/ResourceBuildings/IconSilo").GetComponent<Image>().color = _colour;
                break;

            case Buildings.LumberMill:
                transform.Find("PanelMask/Content/ResourceBuildings/IconLumberMill").GetComponent<Image>().color = _colour;
                break;

            case Buildings.LumberPile:
                transform.Find("PanelMask/Content/ResourceBuildings/IconLumberPile").GetComponent<Image>().color = _colour;
                break;

            case Buildings.Mine:
                transform.Find("PanelMask/Content/ResourceBuildings/IconMine").GetComponent<Image>().color = _colour;
                break;

            case Buildings.MetalStorage:
                transform.Find("PanelMask/Content/ResourceBuildings/IconOreStorage").GetComponent<Image>().color = _colour;
                break;

            default:
                break;
        }
    }

    private void Update()
    {
        if (tooltipSelected == Buildings.None)
        {
            tooltipTimer += Time.unscaledDeltaTime;
        }
        else
        {
            tooltipTimer = 0.0f;
        }

        if (showPanel && !panelShown)
        {
            ShowPanel();
            panelShown = true;
        }

        if (!showPanel && panelShown)
        {
            HidePanel();
            panelShown = false;
        }
    }

    public TooltipInfo GetToolInfo()
    {
        return toolInfo;
    }

    public void TogglePanel()
    {
        showPanel = !showPanel;
    }

    public void SetPanelVisibility(bool _visible)
    {
        showPanel = _visible;
    }

    private void ShowPanel()
    {
        transform.DOKill(true);

        transform.DOLocalMoveY(yPos, 0.4f).SetEase(Ease.OutBack);

        //showPanel = true;
    }

    private void HidePanel()
    {
        transform.DOKill(true);

        transform.DOLocalMoveY(yPos - 158.0f, 0.4f).SetEase(Ease.InBack);

        //showPanel = false;
    }

    private void GetInfo()
    {
        toolInfo.heading = new string[13];
        toolInfo.heading[(int)Buildings.None]               = "";
        toolInfo.heading[(int)Buildings.Ballista]           = StructureNames.BuildPanelToString(Buildings.Ballista);
        toolInfo.heading[(int)Buildings.Catapult]           = StructureNames.BuildPanelToString(Buildings.Catapult);
        toolInfo.heading[(int)Buildings.Barracks]           = StructureNames.BuildPanelToString(Buildings.Barracks);
        toolInfo.heading[(int)Buildings.FreezeTower]        = StructureNames.BuildPanelToString(Buildings.FreezeTower);
        toolInfo.heading[(int)Buildings.ShockwaveTower]     = StructureNames.BuildPanelToString(Buildings.ShockwaveTower);
        toolInfo.heading[(int)Buildings.LigtningTower]      = StructureNames.BuildPanelToString(Buildings.LigtningTower);
        toolInfo.heading[(int)Buildings.Farm]               = StructureNames.BuildPanelToString(Buildings.Farm);
        toolInfo.heading[(int)Buildings.Granary]            = StructureNames.BuildPanelToString(Buildings.Granary);
        toolInfo.heading[(int)Buildings.LumberMill]         = StructureNames.BuildPanelToString(Buildings.LumberMill);
        toolInfo.heading[(int)Buildings.LumberPile]         = StructureNames.BuildPanelToString(Buildings.LumberPile);
        toolInfo.heading[(int)Buildings.Mine]               = StructureNames.BuildPanelToString(Buildings.Mine);
        toolInfo.heading[(int)Buildings.MetalStorage]       = StructureNames.BuildPanelToString(Buildings.MetalStorage);

        toolInfo.description = new string[13];
        toolInfo.description[(int)Buildings.None]           = "";
        toolInfo.description[(int)Buildings.Ballista]       = StructureManager.StructureDescriptions[Buildings.Ballista];
        toolInfo.description[(int)Buildings.Catapult]       = StructureManager.StructureDescriptions[Buildings.Catapult];
        toolInfo.description[(int)Buildings.Barracks]       = StructureManager.StructureDescriptions[Buildings.Barracks];
        toolInfo.heading[(int)Buildings.FreezeTower]        = StructureManager.StructureDescriptions[Buildings.FreezeTower];
        toolInfo.heading[(int)Buildings.ShockwaveTower]     = StructureManager.StructureDescriptions[Buildings.ShockwaveTower];
        toolInfo.heading[(int)Buildings.LigtningTower]      = StructureManager.StructureDescriptions[Buildings.LigtningTower];
        toolInfo.description[(int)Buildings.Farm]           = StructureManager.StructureDescriptions[Buildings.Farm];
        toolInfo.description[(int)Buildings.Granary]        = StructureManager.StructureDescriptions[Buildings.Granary];
        toolInfo.description[(int)Buildings.LumberMill]     = StructureManager.StructureDescriptions[Buildings.LumberMill];
        toolInfo.description[(int)Buildings.LumberPile]     = StructureManager.StructureDescriptions[Buildings.LumberPile];
        toolInfo.description[(int)Buildings.Mine]           = StructureManager.StructureDescriptions[Buildings.Mine];
        toolInfo.description[(int)Buildings.MetalStorage]   = StructureManager.StructureDescriptions[Buildings.MetalStorage];
    }

    public void SetTooltip(int tool)
    {
        tooltipSelected = (Buildings)tool;

        if (tooltipSelected == Buildings.Catapult && !superMan.GetResearchComplete(SuperManager.Catapult)) { return; }
        if (tooltipSelected == Buildings.Barracks && !superMan.GetResearchComplete(SuperManager.Barracks)) { return; }

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

            float targetPos = hoveredButton.transform.localPosition.x;
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
        if ((Buildings)buildingType == Buildings.Catapult && !superMan.GetResearchComplete(SuperManager.Catapult)) { return; }
        if ((Buildings)buildingType == Buildings.Barracks && !superMan.GetResearchComplete(SuperManager.Barracks)) { return; }

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
        //buildIndicator.SetActive(false);

        buildTip.showTooltip = false;
    }

    public void ResetBuildingSelected()
    {
        buildingSelected = Buildings.None;
    }
}
