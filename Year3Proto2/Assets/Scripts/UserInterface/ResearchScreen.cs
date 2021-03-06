﻿using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchScreen : MonoBehaviour
{
    [System.Serializable]
    public struct Building
    {
        public int ID;
        public GameObject card;
        public string name;
        public string description;
        public int price;
        public bool purchased;
        public List<Upgrade> upgrades;
    }

    [SerializeField]
    private List<Building> buildings;

    [System.Serializable]
    public struct Upgrade
    {
        public int ID;
        public GameObject card;
        public string name;
        public string description;
        public int price;
        public bool canPurchase;
        public bool purchased;
    }

    private TMP_Text RPCounter;
    public GameObject buildingCardPrefab;
    public GameObject upgradeCardPrefab;
    private Transform cardPanel;
    private SuperManager superMan;

    public List<SuperManager.ResearchElementDefinition> researchDefinitions;
    public Dictionary<int, bool> completedResearch;
    private int loadingFrameCounter = 0;

    // Scrolling
    public bool dragging;

    private Vector3 dragPotentialStart;
    private Vector3 dragOffset;

    private void Start()
    {
        Time.timeScale = 1.0f;

        superMan = SuperManager.GetInstance();
        RPCounter = transform.Find("RPCounter").GetComponent<TMP_Text>();
        cardPanel = transform.Find("BuildingCards");
        GetResearchInfo();

        InitializeCards();
        //Debug.Log(Screen.width / Screen.height);

        if (Screen.width / Screen.height >= 2.0f)
        {
            Vector3 localPos = cardPanel.localPosition;
            localPos.x = 0.0f;
            cardPanel.localPosition = localPos;
        }
    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            RefreshCards();
        }
        if (SuperManager.DevMode)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                // give the player some research points, they've earned it
                superMan.SetResearchPoints(3000);
                superMan.WriteGameData();
                UpdateRPCounter();
            }
        }

        if (dragging)
        {
            //cardPanel.transform.position = Input.mousePosition + dragOffset;
            Vector3 pos = cardPanel.position;
            pos.x = Input.mousePosition.x + dragOffset.x;
            cardPanel.position = pos;

            Vector3 localPos = cardPanel.localPosition;
            if (localPos.x < -200.0f)
            {
                float diff = localPos.x - -200.0f;
                float squeeze = (diff / 4);
                localPos.x = -200.0f + squeeze;
            }
            else if (localPos.x > 200.0f)
            {
                float diff = localPos.x - 200.0f;
                float squeeze = (diff / 4);
                localPos.x = 200.0f + squeeze;
            }
            cardPanel.localPosition = localPos;
        }

        if (loadingFrameCounter < 20)
        {
            loadingFrameCounter++;
            if (loadingFrameCounter == 20)
            {
                SceneSwitcher switcher = FindObjectOfType<SceneSwitcher>();
                if (switcher.GetLoadingScreenIsActive())
                {
                    switcher.EndLoad();
                }
            }
        }
    }

    private void InitializeCards()
    {
        // Get number of unlockable buildings
        buildings = new List<Building>();

        foreach (SuperManager.ResearchElementDefinition building in researchDefinitions)
        {
            // If the element is a building
            if (building.reqID == -1)
            {
                // set out building data
                Building newBuilding = new Building
                {
                    name = building.name,
                    price = building.price,
                    description = building.description,
                    purchased = completedResearch[building.ID],
                    ID = building.ID
                };
                // create card
                newBuilding.card = Instantiate(buildingCardPrefab);
                newBuilding.card.transform.SetParent(cardPanel);
                newBuilding.card.transform.localScale = Vector3.one;
                newBuilding.card.transform.Find("ResearchInfo/ResearchButton").GetComponent<ResearchButtonDelegate>().ID = building.ID;

                // set out building upgrade data
                newBuilding.upgrades = new List<Upgrade>();
                foreach (SuperManager.ResearchElementDefinition upgrade in researchDefinitions)
                {
                    // if the upgrade requires the building (it's an upgrade)
                    if (upgrade.reqID == building.ID)
                    {
                        newBuilding.upgrades.Add(new Upgrade
                        {
                            name = upgrade.name,
                            description = upgrade.description,
                            price = upgrade.price,
                            purchased = completedResearch[upgrade.ID],
                            ID = upgrade.ID
                        });
                        if (upgrade.isSpecialUpgrade)
                        {
                            newBuilding.card.transform.Find("Upgrades/UpgradeSpecial").GetComponent<ResearchButtonDelegate>().ID = upgrade.ID;
                            break;
                        }
                    }
                }

                // create upgrade cards
                int upgradeLength = newBuilding.upgrades.Count - 1;
                for (int i = 0; i < upgradeLength; i++)
                {
                    // Instantiate standard upgrade cards
                    Upgrade upgrade = newBuilding.upgrades[i];
                    upgrade.card = Instantiate(upgradeCardPrefab);
                    upgrade.card.transform.SetParent(newBuilding.card.transform.Find("Upgrades/UpgradesStandard"));
                    upgrade.card.transform.localScale = Vector3.one;
                    upgrade.card.GetComponent<ResearchButtonDelegate>().ID = upgrade.ID;
                    newBuilding.upgrades[i] = upgrade;
                }

                buildings.Add(newBuilding);
            }
        }

        // Get info from Research Manager
        GetResearchInfo();

        for (int i = 0; i < buildings.Count; i++)
        {
            // Set info on building cards
            buildings[i].card.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildings[i].name;
            buildings[i].card.transform.Find("BuildingIcon").GetComponent<Image>().sprite = FindIcon(buildings[i].name);
            buildings[i].card.transform.Find("ResearchInfo/Description").GetComponent<TMP_Text>().text = buildings[i].description;
            buildings[i].card.transform.Find("ResearchInfo/Price").GetComponent<TMP_Text>().text = buildings[i].price.ToString();

            int upgradeLength = Mathf.Clamp(buildings[i].upgrades.Count - 1, 0, 4);

            // Update upgrade info

            for (int j = 0; j < upgradeLength; j++)
            {
                // Set info on standard upgrade cards
                buildings[i].upgrades[j].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].name;
                buildings[i].upgrades[j].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].description;
            }

            // Set info on special upgrade card
            Upgrade special = buildings[i].upgrades[upgradeLength];
            special.card = buildings[i].card.transform.Find("Upgrades/UpgradeSpecial").gameObject;
            special.card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[upgradeLength].name;
            special.card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[upgradeLength].description;
        }

        RefreshCards();
    }

    private void GetResearchInfo()
    {
        SuperManager superMan = SuperManager.GetInstance();
        researchDefinitions = SuperManager.ResearchDefinitions;
        completedResearch = superMan.GetResearch();
    }

    private void RefreshCards()
    {
        // Get info from Research Manager
        GetResearchInfo();

        for (int i = 0; i < buildings.Count; i++)
        {
            int upgradeLength = Mathf.Clamp(buildings[i].upgrades.Count - 1, 0, 4);

            // Update some stuff depending on whether the building is purchased
            if (buildings[i].purchased)
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(true);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(false);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(true);

                int counter = 0;
                for (int j = 0; j < buildings[i].upgrades.Count; j++)
                {
                    // Enable purchase of upgrades
                    Upgrade temp = buildings[i].upgrades[j];
                    temp.canPurchase = true;
                    buildings[i].upgrades[j] = temp;
                    // Check if special upgrade can be purchased
                    if (buildings[i].upgrades[j].purchased && j < upgradeLength) { counter++; }
                }

                Upgrade specialTemp = buildings[i].upgrades[upgradeLength];
                specialTemp.canPurchase = counter == upgradeLength;
                buildings[i].upgrades[upgradeLength] = specialTemp;
            }
            else
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(false);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(true);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(false);

                for (int j = 0; j < buildings[i].upgrades.Count; j++)
                {
                    // Disable purchase of upgrades
                    Upgrade temp = buildings[i].upgrades[j];
                    temp.canPurchase = false;
                    buildings[i].upgrades[j] = temp;
                }

                Upgrade specialTemp = buildings[i].upgrades[upgradeLength];
                specialTemp.canPurchase = false;
                buildings[i].upgrades[upgradeLength] = specialTemp;
            }

            // Update upgrade info

            for (int j = 0; j < upgradeLength; j++)
            {
                buildings[i].upgrades[j].card.GetComponent<Button>().interactable = buildings[i].upgrades[j].canPurchase;

                // Show or hide price of standard upgrade based on purchase state
                if (buildings[i].upgrades[j].purchased)
                {
                    buildings[i].upgrades[j].card.transform.Find("Price").gameObject.SetActive(false);
                    buildings[i].upgrades[j].card.transform.Find("Purchased").gameObject.SetActive(true);
                }
                else
                {
                    buildings[i].upgrades[j].card.transform.Find("Price").gameObject.SetActive(true);
                    buildings[i].upgrades[j].card.transform.Find("Price").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].price.ToString();
                    buildings[i].upgrades[j].card.transform.Find("Purchased").gameObject.SetActive(false);
                }
            }

            // Set info on special upgrade card
            Upgrade special = buildings[i].upgrades[upgradeLength];
            special.card = buildings[i].card.transform.Find("Upgrades/UpgradeSpecial").gameObject;
            special.card.transform.Find("Lock").gameObject.SetActive(!special.canPurchase);
            special.card.GetComponent<Button>().interactable = special.canPurchase;
            buildings[i].upgrades[upgradeLength] = special;

            // Show or hide price of special upgrade based on purchase state
            if (buildings[i].upgrades[upgradeLength].purchased)
            {
                buildings[i].upgrades[upgradeLength].card.transform.Find("Price").gameObject.SetActive(false);
                buildings[i].upgrades[upgradeLength].card.transform.Find("Purchased").gameObject.SetActive(true);
            }
            else
            {
                buildings[i].upgrades[upgradeLength].card.transform.Find("Price").gameObject.SetActive(true);
                buildings[i].upgrades[upgradeLength].card.transform.Find("Price").GetComponent<TMP_Text>().text = buildings[i].upgrades[upgradeLength].price.ToString();
                buildings[i].upgrades[upgradeLength].card.transform.Find("Purchased").gameObject.SetActive(false);
            }
        }

        UpdateRPCounter();
    }

    private void UpdateRPCounter()
    {
        RPCounter.text = SuperManager.GetInstance().GetResearchPoints().ToString();
    }

    private Sprite FindIcon(string _name)
    {
        Sprite buildingSprite = null;

        if (_name != null)
        {
            string formattedName = _name.Replace(" ", "");
            formattedName = "uiBuilding" + formattedName;
            buildingSprite = Resources.Load<Sprite>("BuildingThumbs/" + formattedName);
        }

        if (buildingSprite == null)
        {
            buildingSprite = Resources.Load<Sprite>("BuildingThumbs/uiBuildingPlaceholder");
        }

        return buildingSprite;
    }

    public void ResearchButton(int _ID)
    {
        // attempt the research
        bool success = SuperManager.GetInstance().AttemptResearch(_ID);

        // we're talking about a building
        if (researchDefinitions[_ID].reqID == -1)
        {
            int buildingNum = BuildingFromResearchID(_ID);
            Building temp = buildings[buildingNum];
            temp.purchased = success;
            buildings[buildingNum] = temp;
        }
        else // we're talking about an upgrade
        {
            //get the building
            int buildingNum = BuildingFromResearchID(researchDefinitions[_ID].reqID);
            int upgradeNum = UpgradeFromResearchID(buildings[buildingNum], _ID);
            Upgrade temp = buildings[buildingNum].upgrades[upgradeNum];
            temp.purchased = success;
            buildings[buildingNum].upgrades[upgradeNum] = temp;
        }
        // SuperManager.GetInstance().saveData.researchPoints <<< DAVID
        // refresh cards
        RefreshCards();
    }

    private int BuildingFromResearchID(int _ID)
    {
        int buildingNum = -1;
        for (int i = 0; i < buildings.Count; i++)
        {
            if (buildings[i].ID == _ID)
            {
                buildingNum = i;
            }
        }
        return buildingNum;
    }

    private int UpgradeFromResearchID(Building _building, int _ID)
    {
        int upgradeNum = -1;
        for (int i = 0; i < _building.upgrades.Count; i++)
        {
            if (_building.upgrades[i].ID == _ID)
            {
                upgradeNum = i;
            }
        }
        return upgradeNum;
    }

    public void StartPotantialDrag()
    {
        dragPotentialStart = Input.mousePosition;
    }

    public void DragCard()
    {
        dragging = true;

        dragOffset = cardPanel.position - dragPotentialStart;

        cardPanel.DOKill(true);
    }

    public void ReleaseCard()
    {
        dragging = false;

        cardPanel.DOLocalMoveX(Mathf.Clamp(cardPanel.localPosition.x, -200.0f, 200.0f), 0.4f).SetEase(Ease.OutQuint);
    }
}