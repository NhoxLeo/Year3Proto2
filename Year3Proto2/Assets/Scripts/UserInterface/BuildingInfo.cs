using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingInfo : MonoBehaviour
{
    public bool showPanel;
    private RectTransform rTrans;
    private Tooltip tool;
    private BuildPanel buildPanel;
    private GameManager gameMan;

    public GameObject targetBuilding;
    public string buildingName;

    public Sprite defenceSprite;
    public Sprite foodSprite;
    public Sprite woodSprite;
    public Sprite metalSprite;
    public Sprite emptySprite;

    private TMP_Text headingText;               // Text showing name of building
    private TMP_Text statHeadingText;           // Text showing name of stat e.g Production Rate
    private TMP_Text statValueText;             // Text showing value of stat
    private TMP_Text statInfoText;              // Additional info shown next to stat value
    private Image statIcon;                     // Icon shown next to stat value
    private GameObject foodComponent;           // Section for food allocation
    private TMP_Text foodValueText;             // Text showing current food allocation
    private Button repairButton;                // Button for repairing buildings

    public bool doAutoUpdate;                   // Whether to automatically update info panel
    public float updateInterval = 1.5f;         // Time between info panel updates
    private float updateTimer;

    void Start()
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
        repairButton = transform.Find("PanelMask/RepairButton").GetComponent<Button>();
        statInfoText.text = "";
        gameMan = FindObjectOfType<GameManager>();
        updateTimer = updateInterval;
    }
    void LateUpdate()
    {
        tool.showTooltip = showPanel;

        if (targetBuilding != null)
        {
            headingText.text = buildingName;
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
            headingText.text = "missingno";
        }

        string env = "Environment";
        if (buildingName.Contains(env))
        {
            string newHeading = headingText.text.Remove(headingText.text.IndexOf(env), env.Length);
            headingText.text = newHeading;
        }

        SetPosition();
    }

    public void SetInfo()
    {
        repairButton.interactable = false;
        if (targetBuilding == null)
        {
            return;
        }
        switch (buildingName)
        {
            case "Archer Tower":
                foodComponent.SetActive(true);
                ArcherTower archer = targetBuilding.GetComponent<ArcherTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Attack Power";
                statValueText.text = archer.arrowDamage.ToString("0");
                statInfoText.text = "Single target";
                foodValueText.text = "?/?";

                repairButton.interactable = (archer.GetHealth() < archer.GetMaxHealth());
                break;
            case "Catapult Tower":
                foodComponent.SetActive(true);
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Attack Power";
                statValueText.text = "???";
                statInfoText.text = "AOE Damage";
                foodValueText.text = "?/?";

                repairButton.interactable = (catapult.GetHealth() < catapult.GetMaxHealth());
                break;

            case "Farm":
                foodComponent.SetActive(false);
                Farm farm = targetBuilding.GetComponent<Farm>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = farm.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + farm.productionTime.ToString("0") + "s";

                repairButton.interactable = (farm.GetHealth() < farm.GetMaxHealth());
                break;
            case "Granary":
                foodComponent.SetActive(false);
                Granary granary = targetBuilding.GetComponent<Granary>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = granary.storage.ToString();
                statInfoText.text = "";
                repairButton.interactable = (granary.GetHealth() < granary.GetMaxHealth());
                break;
            case "Lumber Mill":
                foodComponent.SetActive(true);
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mill.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mill.productionTime.ToString("0") + "s";
                foodValueText.text = mill.GetFoodAllocation().ToString("0") + "/" + ResourceStructure.GetFoodAllocationMax().ToString("0");
                repairButton.interactable = (mill.GetHealth() < mill.GetMaxHealth());
                break;
            case "Lumber Pile":
                foodComponent.SetActive(false);
                LumberPile pile = targetBuilding.GetComponent<LumberPile>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = pile.storage.ToString();
                statInfoText.text = "";
                repairButton.interactable = (pile.GetHealth() < pile.GetMaxHealth());
                break;
            case "Mine":
                foodComponent.SetActive(true);
                Mine mine = targetBuilding.GetComponent<Mine>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mine.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mine.productionTime.ToString("0") + "s";
                foodValueText.text = mine.GetFoodAllocation().ToString("0") + "/" + ResourceStructure.GetFoodAllocationMax().ToString("0");
                repairButton.interactable = (mine.GetHealth() < mine.GetMaxHealth());
                break;
            case "Metal Storage":
                foodComponent.SetActive(false);
                MetalStorage metStore = targetBuilding.GetComponent<MetalStorage>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = metStore.storage.ToString();
                repairButton.interactable = (metStore.GetHealth() < metStore.GetMaxHealth()) ? true : false;
                statInfoText.text = "";
                break;
            case "Longhaus":                foodComponent.SetActive(false);
                Longhaus haus = targetBuilding.GetComponent<Longhaus>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Your home base";
                statValueText.text = "Protect me!";                statInfoText.text = "";
                repairButton.interactable = (haus.GetHealth() < haus.GetMaxHealth());
                break;
            case "Forest Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Lumber Mills";
                statInfoText.text = "";
                break;
            case "Hill Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Mines";
                statInfoText.text = "";
                break;
            case "Plains Environment":
                foodComponent.SetActive(false);
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Bonus Building Type";
                statValueText.text = "Farms";
                statInfoText.text = "";
                break;
            default:
                foodComponent.SetActive(false);
                statIcon.sprite = emptySprite;
                statHeadingText.text = "Unknown Stat";
                statValueText.text = "???";
                statInfoText.text = "Missing target";
                break;
        }

        tool.SetHeight(foodComponent.activeSelf ? 242.0f : 152.0f);
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
        buildingName = name;
        SetInfo();
    }

    public void AllocateFood(int amount)
    {
        if (targetBuilding == null)
            return;

        switch (buildingName)
        {
            case "Archer Tower":
                ArcherTower archer = targetBuilding.GetComponent<ArcherTower>();
                break;
            case "Catapult Tower":
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                break;
            case "Lumber Mill":
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                if (amount > 0) { mill.IncreaseFoodAllocation(); }
                else { mill.DecreaseFoodAllocation(); }
                break;
            case "Mine":
                Mine mine = targetBuilding.GetComponent<Mine>();
                if (amount > 0) { mine.IncreaseFoodAllocation(); }
                else { mine.DecreaseFoodAllocation(); }
                break;
            default:
                break;
        }

        SetInfo();
    }

    public void RepairBuilding()
    {
        if (targetBuilding == null)
            return;

        Structure targetStructure = targetBuilding.GetComponent<Structure>();
        ResourceBundle repairCost = targetStructure.RepairCost();
        if (gameMan.playerData.CanAfford(repairCost))
        {
            gameMan.playerData.DeductResource(ResourceType.wood, repairCost.woodCost);
            gameMan.playerData.DeductResource(ResourceType.metal, repairCost.metalCost);
            gameMan.playerData.DeductResource(ResourceType.food, repairCost.foodCost);
            targetStructure.Repair();
        }
        SetInfo();
    }
   
    public void SetVisibility(bool visible)
    {
        showPanel = visible;
    }

}
