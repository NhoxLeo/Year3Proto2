﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private TMP_Text headingText;               // Text showing name of building
    private TMP_Text statHeadingText;           // Text showing name of stat e.g Production Rate
    private TMP_Text statValueText;             // Text showing value of stat
    private TMP_Text statInfoText;              // Additional info shown next to stat value
    private Image statIcon;                     // Icon shown next to stat value
    private GameObject foodComponent;           // Section for food allocation
    private TMP_Text foodValueText;             // Text showing current food allocation

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

        statInfoText.text = "";
    }


    void LateUpdate()
    {
        tool.showTooltip = showPanel;

        headingText.text = buildingName;
        string env = "Environment";
        if (buildingName.Contains(env))
        {
            string newHeading = headingText.text.Remove(headingText.text.IndexOf(env), env.Length);
            headingText.text = newHeading;
        }

        SetPosition();

        if (Input.GetKeyDown(KeyCode.K))
        {
            SetInfo();
        }
    }

    public void SetInfo()
    {
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
                break;

            case "Catapult Tower":
                foodComponent.SetActive(true);
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Attack Power";
                //statValueText.text = catapult.damage;
                statInfoText.text = "AOE Damage";
                break;

            case "Farm":
                foodComponent.SetActive(false);
                Farm farm = targetBuilding.GetComponent<Farm>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = farm.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + farm.productionTime.ToString("0") + "s";
                break;

            case "Granary":
                foodComponent.SetActive(false);
                Granary granary = targetBuilding.GetComponent<Granary>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = granary.storage.ToString();
                statInfoText.text = "";
                break;

            case "Lumber Mill":
                foodComponent.SetActive(true);
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mill.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mill.productionTime.ToString("0") + "s";
                foodValueText.text = mill.GetFoodAllocation().ToString("0") + "/" + ResourceStructure.GetFoodAllocationMax().ToString("0");
                break;

            case "Lumber Pile":
                foodComponent.SetActive(false);
                LumberPile pile = targetBuilding.GetComponent<LumberPile>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = pile.storage.ToString();
                statInfoText.text = "";
                break;

            case "Mine":
                foodComponent.SetActive(true);
                Mine mine = targetBuilding.GetComponent<Mine>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Production Rate";
                statValueText.text = mine.GetProductionVolume().ToString("0");
                statInfoText.text = "Every " + mine.productionTime.ToString("0") + "s";
                foodValueText.text = mine.GetFoodAllocation().ToString("0") + "/" + ResourceStructure.GetFoodAllocationMax().ToString("0");
                break;

            case "Metal Storage":
                foodComponent.SetActive(false);
                MetalStorage metStore = targetBuilding.GetComponent<MetalStorage>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = metStore.storage.ToString();
                statInfoText.text = "";
                break;

            case "Longhaus":
                foodComponent.SetActive(false);
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "";
                statValueText.text = "Protect me!";
                statInfoText.text = "";
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
        else if (transform.localPosition.x > 576.0f)
        {
            xPivot = 1.25f;
        }

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
   
    public void SetVisibility(bool visible)
    {
        showPanel = visible;
    }
}
