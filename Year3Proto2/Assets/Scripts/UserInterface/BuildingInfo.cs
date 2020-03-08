using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BuildingInfo : MonoBehaviour
{
    private Tooltip tool;
    public GameObject targetBuilding;

    public Sprite defenceSprite;
    public Sprite foodSprite;
    public Sprite woodSprite;
    public Sprite metalSprite;

    private TMP_Text headingText;
    private TMP_Text statHeadingText;
    private TMP_Text statValueText;
    private TMP_Text statInfoText;
    private Image statIcon;

    private GameObject foodComponent;
    private TMP_Text foodValueText;

    public string buildingName;

    void Start()
    {
        tool = GetComponent<Tooltip>();

        headingText = transform.Find("PanelMask/Heading").GetComponent<TMP_Text>();
        statHeadingText = transform.Find("PanelMask/Stat/StatHeading").GetComponent<TMP_Text>();
        statValueText = transform.Find("PanelMask/Stat/StatValue").GetComponent<TMP_Text>();
        statInfoText = transform.Find("PanelMask/Stat/StatValue/StatInfo").GetComponent<TMP_Text>();
        statIcon = transform.Find("PanelMask/Stat/StatIcon").GetComponent<Image>();

        foodComponent = transform.Find("PanelMask/Allocation").gameObject;
        foodValueText = transform.Find("PanelMask/Allocation/FoodBox/FoodValue").GetComponent<TMP_Text>();

        statInfoText.text = "";
    }


    void Update()
    {
        headingText.text = buildingName;

        if (Input.GetKeyDown(KeyCode.K))
        {
            SetInfo();
        }

    }


    private void SetInfo()
    {
        switch (buildingName)
        {
            case "Archer Tower":
                foodComponent.SetActive(true);
                ArcherTower archer = targetBuilding.GetComponent<ArcherTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Attack Power";

                tool.height = 256.0f;
                break;

            case "Catapult Tower":
                foodComponent.SetActive(true);
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();
                statIcon.sprite = defenceSprite;
                statHeadingText.text = "Attack Power";

                tool.height = 256.0f;
                break;

            case "Farm":
                foodComponent.SetActive(false);
                Farm farm = targetBuilding.GetComponent<Farm>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Production Rate";

                tool.height = 158.0f;
                break;

            case "Granary":
                foodComponent.SetActive(false);
                Granary granary = targetBuilding.GetComponent<Granary>();
                statIcon.sprite = foodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = granary.storage.ToString();

                tool.height = 158.0f;
                break;

            case "Lumber Mill":
                foodComponent.SetActive(true);
                LumberMill mill = targetBuilding.GetComponent<LumberMill>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Production Rate";

                tool.height = 256.0f;
                break;

            case "Lumber Pile":
                foodComponent.SetActive(false);
                LumberPile pile = targetBuilding.GetComponent<LumberPile>();
                statIcon.sprite = woodSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = pile.storage.ToString();

                tool.height = 158.0f;
                break;

            case "Mine":
                foodComponent.SetActive(true);
                Mine mine = targetBuilding.GetComponent<Mine>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Production Rate";
                mine.IncreaseFoodAllocation();

                tool.height = 256.0f;
                break;

            case "Metal Storage":
                foodComponent.SetActive(false);
                MetalStorage metStore = targetBuilding.GetComponent<MetalStorage>();
                statIcon.sprite = metalSprite;
                statHeadingText.text = "Storage Capacity";
                statValueText.text = metStore.storage.ToString();

                
                tool.height = 158.0f;
                break;

            default:

                break;
        }

        tool.SetHeight();
    }

    public void SetTargetBuilding(GameObject building, string name)
    {
        targetBuilding = building;
        buildingName = name;

        SetInfo();
    }

    public void AllocateFood(int amount)
    {

        switch (buildingName)
        {
            case "Archer Tower":
                ArcherTower archer = targetBuilding.GetComponent<ArcherTower>();

                break;

            case "Catapult Tower":
                CatapultTower catapult = targetBuilding.GetComponent<CatapultTower>();

                break;

            case "Farm":
                Farm farm = targetBuilding.GetComponent<Farm>();
                if (amount > 0) { farm.IncreaseFoodAllocation(); }
                else { farm.DecreaseFoodAllocation(); }
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
    }
    
}
