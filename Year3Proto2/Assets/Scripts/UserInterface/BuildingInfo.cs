using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public bool showPanel;
    [SerializeField] private UIAnimator infoPanel;
    [SerializeField] private UIAnimator actionPanel;
    private Vector3 actionPanelPos;
    private BuildPanel buildPanel;
    private StructureManager structMan;

    private GameObject targetBuilding;
    private Structure targetStructure;
    private string buildingName;

    [Header("Sprites")]
    [SerializeField] private Sprite defenceSprite;
    [SerializeField] private Sprite villagerSprite;
    [SerializeField] private Sprite foodSprite;
    [SerializeField] private Sprite woodSprite;
    [SerializeField] private Sprite metalSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite destroySprite;
    [SerializeField] private Sprite minimizeSprite;

    [Header("Text")]
    [SerializeField] private TMP_Text headingTextFloating;       // Text showing name of building
    [SerializeField] private TMP_Text headingText;               // Text showing name of building
    [SerializeField] private TMP_Text statHeadingText;           // Text showing name of stat e.g Production Rate
    [SerializeField] private TMP_Text statValueText;             // Text showing value of stat
    [SerializeField] private TMP_Text statInfoText;              // Additional info shown next to stat value
    [SerializeField] private Image statIcon;                     // Icon shown next to stat value

    [Header("Buttons")]
    //[SerializeField] private EnvInfo repairTooltip;            // Tooltip that appears over repair and destroy buttons
    [SerializeField] private Button repairButton;                // Button for repairing buildings
    [SerializeField] private Button destroyButton;               // Button to destroy buildings
    [SerializeField] private bool showDestroyConfirm = false;    // Whether to show the detruction confrimation button
    [SerializeField] private Tooltip destroyButtonConfirm;       // Button to confirm destruction of a building
    [SerializeField] private Tooltip trainVillagerButton;        // Button to train a new villager for the Longhaus

    [Header("Tooltip")]                                          // Repair and Destroy tooltips
    [SerializeField] private RectTransform tooltipTransform;
    [SerializeField] private TMP_Text tooltipHeading;
    [SerializeField] private GameObject costComponent;
    [SerializeField] private TMP_Text woodCost;
    [SerializeField] private TMP_Text metalCost;
    [SerializeField] private TMP_Text tooltipDescription;
    private int tooltipMode = -1;

    [Header("Settings")]
    public bool doAutoUpdate;                   // Whether to automatically update info panel
    public float updateInterval = 0.25f;        // Time between info panel updates
    private float updateTimer;

    private void Start()
    {
        buildPanel = FindObjectOfType<BuildPanel>();
        structMan = FindObjectOfType<StructureManager>();
        buildingName = "Building Name";

        statInfoText.text = "";

        updateTimer = updateInterval;

    }

    private void LateUpdate()
    {
        infoPanel.SetVisibility(showPanel);
        actionPanel.SetVisibility(showPanel);

        if (targetBuilding != null)
        {
            headingText.text = buildingName;
            headingTextFloating.text = buildingName;

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
            headingTextFloating.text = newHeading;
        }

        SetPosition();

        if (!showPanel && showDestroyConfirm)
        {
            showDestroyConfirm = false;
            destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
            destroyButtonConfirm.showTooltip = showDestroyConfirm;
        }
        

        if (Input.GetKeyDown(KeyCode.X))
        {
            targetBuilding.GetComponent<Structure>().Damage(50.0f);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            targetBuilding.GetComponent<Structure>().Damage(-50.0f);
        }
    }

    public void SetInfo()
    {
        repairButton.gameObject.SetActive(true);
        repairButton.interactable = false;
        destroyButton.gameObject.SetActive(false);
        trainVillagerButton.showTooltip = false;

        if (targetBuilding == null)
        {
            return;
        }

        switch (buildingName)
        {
            case "Ballista Tower":
                Ballista ballista = targetBuilding.GetComponent<Ballista>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = ballista.GetFireRate().ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = ballista.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = ballista.CanBeRepaired();
                break;

            case "Catapult Tower":
                Catapult catapult = targetBuilding.GetComponent<Catapult>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = catapult.GetFireRate().ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = catapult.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = catapult.CanBeRepaired();
                break;

            case "Barracks":
                Barracks barracks = targetBuilding.GetComponent<Barracks>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Troop Capacity";
                //statValueText.text = barracks.GetTroopCapacity().ToString("0");
                statInfoText.text = "units";
                //foodValueText.text = barracks.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = barracks.CanBeRepaired();
                break;


            case "Farm":
                Farm farm = targetBuilding.GetComponent<Farm>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = farm.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + farm.productionTime.ToString("0") + "s";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = farm.CanBeRepaired();
                break;

            case "Granary":
                Granary granary = targetBuilding.GetComponent<Granary>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = granary.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = granary.CanBeRepaired();
                break;

            case "Lumber Mill":
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mill.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mill.productionTime.ToString("0") + "s";
                //foodValueText.text = mill.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = mill.CanBeRepaired();
                break;

            case "Lumber Pile":
                LumberPile pile = targetBuilding.GetComponent<LumberPile>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = pile.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = pile.CanBeRepaired();
                break;

            case "Mine":
                Mine mine = targetBuilding.GetComponent<Mine>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mine.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mine.productionTime.ToString("0") + "s";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = mine.CanBeRepaired();
                break;

            case "Metal Storage":
                MetalStorage metStore = targetBuilding.GetComponent<MetalStorage>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = metStore.storage.ToString();
                statInfoText.text = "";

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = metStore.CanBeRepaired();
                break;

            case "Longhaus":
                Longhaus haus = targetBuilding.GetComponent<Longhaus>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Your home base";
                statValueText.text = "Protect me!";
                statInfoText.text = "";

                trainVillagerButton.showTooltip = true;
                repairButton.interactable = haus.CanBeRepaired();
                break;

            case "Forest Environment":
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Lumber Mills";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;

            case "Hills Environment":
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Mines";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;

            case "Plains Environment":
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Farms";
                statInfoText.text = "";

                repairButton.gameObject.SetActive(false);
                break;

            default:
                statIcon.sprite = emptySprite;
                statHeadingText.text = "Unknown Stat";
                statValueText.text = "???";
                statInfoText.text = "Missing target";
                break;
        }

        switch (tooltipMode)
        {
            case -1:
                tooltipDescription.gameObject.SetActive(false);
                break;

            case 0:
                FetchRepairInfo();
                break;
            case 1:
                FetchCompensationInfo();
                break;
        }
    }

    private void SetPosition()
    {
        if (targetBuilding == null)
            return;

        // Position action panel near target building
        Vector3 pos = Camera.main.WorldToScreenPoint(targetBuilding.transform.position);
        actionPanel.transform.position = pos;
    }

    public void SetTargetBuilding(GameObject building, string name)
    {
        targetBuilding = building;
        targetStructure = targetBuilding.GetComponent<Structure>();
        buildingName = name;

        if (infoPanel.showElement)
        {
            infoPanel.Pulse();
            actionPanel.Pulse();
        }

        showDestroyConfirm = false;
        destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
        destroyButtonConfirm.showTooltip = showDestroyConfirm;

        SetInfo();
    }

    public void SetTooptipMode(int _mode)
    {
        tooltipMode = +_mode;
    }

    private void RefreshTooltip()
    {
        if (tooltipDescription.text == "")
        {
            tooltipDescription.gameObject.SetActive(false);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipTransform);
    }

    public void FetchRepairInfo()
    {
        tooltipHeading.text = "Repair (R: Mass Repair)";
        ResourceBundle repairCost = targetStructure.RepairCost();

        if (repairCost.woodCost + repairCost.metalCost != 0)
        {
            costComponent.SetActive(true);
            woodCost.text = repairCost.woodCost.ToString();
            metalCost.text = repairCost.metalCost.ToString();

            tooltipDescription.gameObject.SetActive(true);
            tooltipDescription.text = repairButton.interactable ? "" : "Cannot repair while recently damaged";
        }
        else
        {
            costComponent.SetActive(false);
            tooltipDescription.gameObject.SetActive(true);
            tooltipDescription.text = "Building is at full health";
        }

        RefreshTooltip();
    }

    public void FetchCompensationInfo()
    {
        float health = targetStructure.GetHealth();
        float maxHealth = targetStructure.GetMaxHealth();
        ResourceBundle compensation = new ResourceBundle(0.5f * (health / maxHealth) * (Vector3)structMan.structureCosts[targetStructure.GetStructureName()]);

        tooltipHeading.text = "Destroy Building";

        costComponent.SetActive(true);
        woodCost.text = "+" + compensation.woodCost;
        metalCost.text = "+" + compensation.metalCost;
        tooltipDescription.gameObject.SetActive(true);
        tooltipDescription.text = "Villagers will be evacuated";

        RefreshTooltip();
    }



    public void RepairBuilding()
    {
        if (targetBuilding == null)
            return;

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
    }

    public void HideDestroyConfirm()
    {
        showDestroyConfirm = false;
        destroyButton.gameObject.GetComponent<Image>().sprite = showDestroyConfirm ? minimizeSprite : destroySprite;
        destroyButtonConfirm.showTooltip = showDestroyConfirm;
    }

    public void TrainVillager()
    {
        Longhaus.TrainVillager();
    }

    public void SetVisibility(bool visible)
    {
        showPanel = visible;
    }
}