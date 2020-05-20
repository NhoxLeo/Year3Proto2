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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RefreshCards();
        }

    }

    private void InitializeCards()
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            // Instantiate building cards
            buildings[i].card = Instantiate(buildingCardPrefab);
            buildings[i].card.transform.SetParent(cardPanel);
            buildings[i].card.transform.localScale = Vector3.one;

            // Instantiate standard upgrade cards
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
        for (int i = 0; i < buildings.Length; i++)
        {
            // Set info on building cards
            buildings[i].card.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildings[i].name;
            buildings[i].card.transform.Find("BuildingIcon").GetComponent<Image>().sprite = FindIcon(buildings[i].name);
            buildings[i].card.transform.Find("ResearchInfo/Description").GetComponent<TMP_Text>().text = buildings[i].description;
            buildings[i].card.transform.Find("ResearchInfo/Price").GetComponent<TMP_Text>().text = buildings[i].price.ToString();

            float upgradeLength = Mathf.Clamp(buildings[i].upgrades.Length - 1, 0, 4);
            for (int j = 0; j < upgradeLength; j++)
            {
                // Set info on standard upgrade cards
                buildings[i].upgrades[j].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].name;
                buildings[i].upgrades[j].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].description;

                // Show or hide price based on purchase state
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
            int special = buildings[i].upgrades.Length - 1;
            buildings[i].upgrades[special].card = buildings[i].card.transform.Find("Upgrades/UpgradeSpecial").gameObject;
            buildings[i].upgrades[special].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[special].name;
            buildings[i].upgrades[special].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[special].description;
            

            // Show or hide price based on purchase state
            if (buildings[i].upgrades[special].purchased)
            {
                buildings[i].upgrades[special].card.transform.Find("Price").gameObject.SetActive(false);
                buildings[i].upgrades[special].card.transform.Find("Purchased").gameObject.SetActive(true);
            }
            else
            {
                buildings[i].upgrades[special].card.transform.Find("Price").gameObject.SetActive(true);
                buildings[i].upgrades[special].card.transform.Find("Price").GetComponent<TMP_Text>().text = buildings[i].upgrades[special].price.ToString();
                buildings[i].upgrades[special].card.transform.Find("Purchased").gameObject.SetActive(false);
            }


            // Hide building purchase info when already purchased
            if (buildings[i].purchased)
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(true);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(false);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(true);
            }
            else
            {
                buildings[i].card.transform.Find("Upgrades").gameObject.SetActive(false);
                buildings[i].card.transform.Find("ResearchInfo").gameObject.SetActive(true);
                buildings[i].card.transform.Find("Check").gameObject.SetActive(false);
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