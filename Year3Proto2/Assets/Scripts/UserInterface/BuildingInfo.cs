using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    public bool showPanel;
    private RectTransform rTrans;
    [SerializeField] private UIAnimator infoPanel;
    [SerializeField] private UIAnimator actionPanel;
    private BuildPanel buildPanel;

    public GameObject targetBuilding;
    public string buildingName;

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

    [Header("Settings")]
    public bool doAutoUpdate;                   // Whether to automatically update info panel
    public float updateInterval = 0.25f;        // Time between info panel updates
    private float updateTimer;

    private void Start()
    {
        rTrans = GetComponent<RectTransform>();
        buildPanel = FindObjectOfType<BuildPanel>();

        //headingTextFloating = transform.Find("ActionPanel/BuildingName/Name").GetComponent<TMP_Text>();
        //headingText = transform.Find("InfoPanel/Heading").GetComponent<TMP_Text>();
        //statHeadingText = transform.Find("InfoPanel/Stat/StatHeading").GetComponent<TMP_Text>();
        //statValueText = transform.Find("InfoPanel/Stat/StatValue").GetComponent<TMP_Text>();
        //statInfoText = transform.Find("InfoPanel/Stat/StatValue/StatInfo").GetComponent<TMP_Text>();
        //statIcon = transform.Find("InfoPanel/Stat/StatIcon").GetComponent<Image>();

        //repairButton = transform.Find("ActionPanel/ActionButtons/RepairButton").GetComponent<Button>();        //destroyButton = transform.Find("ActionPanel/ActionButtons/DestroyButton").GetComponent<Button>();        //destroyButtonConfirm = transform.Find("ActionPanel/DestroyConfirmButton").GetComponent<Tooltip>();        //trainVillagerButton = transform.Find("ActionPanel/TrainVillagerButton").GetComponent<Tooltip>();        //globalFoodText = transform.Find("PanelMask/Allocation/ToggleDescription").GetComponent<TMP_Text>();
        statInfoText.text = "";
        updateTimer = updateInterval;
    }
    private void LateUpdate()
    {
        tool.showTooltip = showPanel;

        if (targetBuilding != null)
        {
            headingText.text = buildingName;

            //if (buildingName == "Barracks") { globalFoodText.text = "Affects all " + buildingName; }
            //else { globalFoodText.text = "Affects all " + buildingName + "s"; }

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
        repairButton.interactable = false;        destroyButton.gameObject.SetActive(false);        trainVillagerButton.showTooltip = false;
        if (targetBuilding == null)
        {
            return;
        }
        switch (buildingName)
        {
            case "Ballista Tower":
                BallistaTower ballista = targetBuilding.GetComponent<BallistaTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = ballista.fireRate.ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = ballista.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = ballista.CanBeRepaired();
                break;
            case "Catapult Tower":
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Fire Rate";
                statValueText.text = catapult.fireRate.ToString("F");
                statInfoText.text = "per second";
                //foodValueText.text = catapult.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");

                destroyButton.gameObject.SetActive(true);
                repairButton.interactable = catapult.CanBeRepaired();
                break;
            case "Barracks":
                Barracks barracks = targetBuilding.GetComponent<Barracks>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Troop Capacity";
                statValueText.text = barracks.GetTroopCapacity().ToString("0");
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
                statValueText.text = "Protect me!";                statInfoText.text = "";
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

        tool.SetHeight(182.0f);
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

    public void TrainVillager()
    {
        Longhaus.TrainVillager();
    }

    public void SetVisibility(bool visible)
    {
        showPanel = visible;
    }
}