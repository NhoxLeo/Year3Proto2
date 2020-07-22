using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public bool showPanel;
    private RectTransform rTrans;
    private Tooltip tool;
    private BuildPanel buildPanel;

    public GameObject targetBuilding;
    public string buildingName;

    public Sprite defenceSprite;
    public Sprite foodSprite;
    public Sprite woodSprite;
    public Sprite metalSprite;
    public Sprite emptySprite;
    public Sprite destroySprite;
    public Sprite minimizeSprite;

    private TMP_Text headingText;               // Text showing name of building
    private TMP_Text statHeadingText;           // Text showing name of stat e.g Production Rate
    private TMP_Text statValueText;             // Text showing value of stat
    private TMP_Text statInfoText;              // Additional info shown next to stat value
    private Image statIcon;                     // Icon shown next to stat value
    private GameObject foodComponent;           // Section for food allocation
    private TMP_Text foodValueText;             // Text showing current food allocation
    private TMP_Text globalFoodText;            // "Affects all Buildings"

    public EnvInfo repairTooltip;              // Tooltip that appears over repair and destroy buttons
    private Button repairButton;                // Button for repairing buildings
    private Button destroyButton;               // Button to destroy buildings
    private bool showDestroyConfirm = false;    // Whether to show the detruction confrimation button
    private Tooltip destroyButtonConfirm;       // Button to confirm destruction of a building

    public bool doAutoUpdate;                   // Whether to automatically update info panel
    public float updateInterval = 0.25f;        // Time between info panel updates
    private float updateTimer;

    private void Start()
    {
        rTrans = GetComponent<RectTransform>();
        tool = GetComponent<Tooltip>();
        buildPanel = FindObjectOfType<BuildPanel>();

        headingText = transform.Find("PanelMask/Heading").GetComponent<TMP_Text>();
        statHeadingText = transform.Find("PanelMask/Stat/StatHeading").GetComponent<TMP_Text>();
        statValueText = transform.Find("PanelMask/Stat/StatValue").GetComponent<TMP_Text>();
        statInfoText = transform.Find("PanelMask/Stat/StatValue/StatInfo").GetComponent<TMP_Text>();
        statIcon = transform.Find("PanelMask/Stat/StatIcon").GetComponent<Image>();

        foodComponent = transform.Find("PanelMask/Allocation").gameObject;
        foodValueText = transform.Find("PanelMask/Allocation/FoodBox/FoodValue").GetComponent<TMP_Text>();

        repairButton = transform.Find("ActionButtons/RepairButton").GetComponent<Button>();        destroyButton = transform.Find("ActionButtons/DestroyButton").GetComponent<Button>();        destroyButtonConfirm = transform.Find("DestroyConfirmButton").GetComponent<Tooltip>();        globalFoodText = transform.Find("PanelMask/Allocation/ToggleDescription").GetComponent<TMP_Text>();
        statInfoText.text = "";
        updateTimer = updateInterval;
    }
    private void LateUpdate()
    {
        tool.showTooltip = showPanel;

        if (targetBuilding != null)
        {
            headingText.text = buildingName;

            if (buildingName == "Barracks") { globalFoodText.text = "Affects all " + buildingName; }
            else { globalFoodText.text = "Affects all " + buildingName + "s"; }

            // Auto update info
            if (showPanel && doAutoUpdate)
            {
                updateTimer -= Time.deltaTime;
                if (updateTimer <= 0.0f)
                {
                    SetInfo();
                    updateTimer = updateInterval;
                }
            }
        }
        else
        {
            showPanel = false;
        }

        string env = "Environment";
        if (buildingName.Contains(env))
        {
            string newHeading = headingText.text.Remove(headingText.text.IndexOf(env), env.Length);
            headingText.text = newHeading;
        }

        //SetPosition();

        if (!showPanel && showDestroyConfirm)
        {
            showDestroyConfirm = false;
            destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
            destroyButtonConfirm.showTooltip = showDestroyConfirm;
        }
    }

    public void SetInfo()
    {
        repairButton.gameObject.SetActive(true);
        repairButton.interactable = false;        destroyButton.gameObject.SetActive(false);
        if (targetBuilding == null)
        {
            return;
        }
        switch (buildingName)
        {
            case "Ballista Tower":
                foodComponent.SetActive(true);
                BallistaTower ballista = targetBuilding.GetComponent<BallistaTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = ballista.fireRate.ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = ballista.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");
                foodValueText.text = ballista.GetAllocated().ToString("0") + "/" + ballista.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = ballista.CanBeRepaired();
                break;
            case "Catapult Tower":
                foodComponent.SetActive(true);
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = catapult.fireRate.ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = catapult.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");
                foodValueText.text = catapult.GetAllocated().ToString("0") + "/" + catapult.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = catapult.CanBeRepaired();
                break;
            case "Barracks":
                foodComponent.SetActive(true);
                Barracks barracks = targetBuilding.GetComponent<Barracks>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Troop Capacity";
                statValueText.text = barracks.GetTroopCapacity().ToString("0");
                statInfoText.text = "units";
                //foodValueText.text = barracks.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");
                foodValueText.text = barracks.GetAllocated().ToString("0") + "/" + barracks.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = barracks.CanBeRepaired();
                break;

            case "Farm":
                foodComponent.SetActive(true);
                Farm farm = targetBuilding.GetComponent<Farm>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = farm.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + farm.productionTime.ToString("0") + "s";
                foodValueText.text = farm.GetAllocated().ToString("0") + "/" + farm.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = farm.CanBeRepaired();
                break;
            case "Granary":
                foodComponent.SetActive(false);
                Granary granary = targetBuilding.GetComponent<Granary>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = granary.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = granary.CanBeRepaired();
                break;
            case "Lumber Mill":
                foodComponent.SetActive(true);
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mill.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mill.productionTime.ToString("0") + "s";
                //foodValueText.text = mill.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");
                foodValueText.text = mill.GetAllocated().ToString("0") + "/" + mill.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = mill.CanBeRepaired();
                break;
            case "Lumber Pile":
                foodComponent.SetActive(false);
                LumberPile pile = targetBuilding.GetComponent<LumberPile>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = pile.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = pile.CanBeRepaired();
                break;
            case "Mine":
                foodComponent.SetActive(true);
                Mine mine = targetBuilding.GetComponent<Mine>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mine.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mine.productionTime.ToString("0") + "s";
                foodValueText.text = mine.GetAllocated().ToString("0") + "/" + mine.GetVillagerCapacity().ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = mine.CanBeRepaired();
                break;
            case "Metal Storage":
                foodComponent.SetActive(false);
                MetalStorage metStore = targetBuilding.GetComponent<MetalStorage>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = metStore.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = metStore.CanBeRepaired();
                break;
            case "Longhaus":                foodComponent.SetActive(false);
                Longhaus haus = targetBuilding.GetComponent<Longhaus>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Your home base";
                statValueText.text = "Protect me!";                statInfoText.text = "";
                repairButton.interactable = haus.CanBeRepaired();
                break;
            case "Forest Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Lumber Mills";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;
            case "Hills Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Mines";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;
            case "Plains Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Farms";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;
            default:
                foodComponent.SetActive(false);
                statIcon.sprite = emptySprite;
                statHeadingText.text = "Unknown Stat";
                statValueText.text = "???";
                statInfoText.text = "Missing target";
                break;
        }

        tool.SetHeight(foodComponent.activeSelf ? 308.0f : 182.0f);
    }

    private void SetPosition()
    {
        if (targetBuilding == null)
            return;
        // Position info panel near target building
        Vector3 pos = Camera.main.WorldToScreenPoint(targetBuilding.transform.position);
        transform.position = pos;
        // Adjust position of info panel if near edge of bounds
        float xPivot = -0.25f;
        float yPivot = 0.5f;
        if (transform.localPosition.y < -220f)
        {
            xPivot = 0.5f;
            yPivot = -0.3f;
        }
        if (transform.localPosition.x > 576.0f)
        {
            xPivot = 1.25f;
        }
        // Smooth Lerping motion
        float dt = Time.unscaledDeltaTime;
        Vector2 pivot = new Vector2(Mathf.Lerp(rTrans.pivot.x, xPivot, dt * 10.0f), Mathf.Lerp(rTrans.pivot.y, yPivot, dt * 10.0f));
        rTrans.pivot = pivot;

        // Close info panel if out of bounds or covering build panel
        if (transform.localPosition.x < -930.0f || transform.localPosition.x > 930.0f)
        {
            showPanel = false;
        }
        if (buildPanel.showPanel && transform.localPosition.y < -420.0f)
        {
            showPanel = false;
        }
        else if (transform.localPosition.y < -520.0f)
        {
            showPanel = false;
        }
    }

    public void SetTargetBuilding(GameObject building, string name)
    {
        targetBuilding = building;
        buildingName = name;        if (tool.showTooltip)
        {
            tool.PulseTip();
        }

        showDestroyConfirm = false;
        destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
        destroyButtonConfirm.showTooltip = showDestroyConfirm;

        SetInfo();
    }

    public void AllocateFood(int amount)
    {
        if (targetBuilding == null)
            return;

        Structure structure = targetBuilding.GetComponent<Structure>();
        if (amount > 0) { structure.AllocateVillager(); }
        else { structure.DeallocateVillager(); }

        SetInfo();
    }

    public void RepairBuilding()
    {
        if (targetBuilding == null)
            return;

        Structure targetStructure = targetBuilding.GetComponent<Structure>();
        //ResourceBundle repairCost = targetStructure.RepairCost();
        targetStructure.Repair();
        SetInfo();
    }

    public void DestroyBuilding()
    {
        FindObjectOfType<StructureManager>().DestroySelectedBuilding();
    }

    public void ToggleDestroyConfirm()
    {
        showDestroyConfirm = !showDestroyConfirm;
        destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
        destroyButtonConfirm.showTooltip = showDestroyConfirm;

        //repairTooltip.SetVisibility(!showDestroyConfirm);
    }

    public void SetVisibility(bool visible)
    {
        showPanel = visible;
    }
}