using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchScreen : MonoBehaviour
{
    [System.Serializable]
    public struct Buildings
    {
        public GameObject card;
        public string name;
        public string description;
        public int price;
        public bool purchased;

        public Upgrades[] upgrades;
    }

    [SerializeField]
    private Buildings[] buildings;

    [System.Serializable]
    public struct Upgrades
    {
        public GameObject card;
        public string name;
        public string description;
        public int price;
        public bool canPurchase;
        public bool purchased;
    }

    public GameObject buildingCardPrefab;
    public GameObject upgradeCardPrefab;
    public Transform cardPanel;

    private void Start()
    {
        cardPanel = transform.Find("BuildingCards");

        InitializeCards();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            RefreshCards();
        }
    }

    private void InitializeCards()
    {
        //buildings = new Buildings[numberOfBuildings];

        for (int i = 0; i < buildings.Length; i++)
        {
            // Instantiate building cards
            buildings[i].card = Instantiate(buildingCardPrefab);
            buildings[i].card.transform.SetParent(cardPanel);
            buildings[i].card.transform.localScale = Vector3.one;

            // Instantiate standard upgrade cards

            //buildings[i].upgrades = new Upgrades[numberOfUpgrades];
            float upgradeLength = Mathf.Clamp(buildings[i].upgrades.Length - 1, 0, 4);
            for (int j = 0; j < upgradeLength; j++)
            {
                // Instantiate standard upgrade cards
                buildings[i].upgrades[j].card = Instantiate(upgradeCardPrefab);
                buildings[i].upgrades[j].card.transform.SetParent(buildings[i].card.transform.Find("Upgrades/UpgradesStandard"));
                buildings[i].upgrades[j].card.transform.localScale = Vector3.one;
            }
        }

        RefreshCards();
    }

    private void RefreshCards()
    {
        // Get info from Research Manager
        // ...here

        for (int i = 0; i < buildings.Length; i++)
        {
            // Set info on building cards
            buildings[i].card.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildings[i].name;
            buildings[i].card.transform.Find("BuildingIcon").GetComponent<Image>().sprite = FindIcon(buildings[i].name);
            buildings[i].card.transform.Find("ResearchInfo/Description").GetComponent<TMP_Text>().text = buildings[i].description;
            buildings[i].card.transform.Find("ResearchInfo/Price").GetComponent<TMP_Text>().text = buildings[i].price.ToString();

            int upgradeLength = Mathf.Clamp(buildings[i].upgrades.Length - 1, 0, 4);

            // Update some stuff depending on whether the building is purchased
            if (buildings[i].purchased)
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(true);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(false);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(true);

                int counter = 0;
                for (int j = 0; j < buildings[i].upgrades.Length; j++)
                {
                    // Enable purchase of upgrades
                    buildings[i].upgrades[j].canPurchase = true;

                    // Check if special upgrade can be purchased
                    if (buildings[i].upgrades[j].purchased && j < upgradeLength) { counter++; }
                }

                buildings[i].upgrades[upgradeLength].canPurchase = (counter == upgradeLength);
            }
            else
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(false);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(true);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(false);

                for (int j = 0; j < buildings[i].upgrades.Length; j++)
                {
                    // Disable purchase of upgrades
                    buildings[i].upgrades[j].canPurchase = false;
                }

                buildings[i].upgrades[upgradeLength].canPurchase = false;
            }

            // Update upgrade info

            for (int j = 0; j < upgradeLength; j++)
            {
                // Set info on standard upgrade cards
                buildings[i].upgrades[j].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].name;
                buildings[i].upgrades[j].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].description;
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
            buildings[i].upgrades[upgradeLength].card = buildings[i].card.transform.Find("Upgrades/UpgradeSpecial").gameObject;
            buildings[i].upgrades[upgradeLength].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[upgradeLength].name;
            buildings[i].upgrades[upgradeLength].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[upgradeLength].description;
            buildings[i].upgrades[upgradeLength].card.transform.Find("Lock").gameObject.SetActive(!buildings[i].upgrades[upgradeLength].canPurchase);
            buildings[i].upgrades[upgradeLength].card.GetComponent<Button>().interactable = buildings[i].upgrades[upgradeLength].canPurchase;

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
    }

    private Sprite FindIcon(string _name)
    {
        string formattedName = _name.Replace(" ", "");
        formattedName = "uiBuilding" + formattedName;
        Sprite buildingSprite = Resources.Load<Sprite>("BuildingThumbs/" + formattedName);

        if (buildingSprite == null)
        {
            buildingSprite = Resources.Load<Sprite>("BuildingThumbs/uiBuildingPlaceholder");
        }

        return buildingSprite;
    }
}