using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchScreen : MonoBehaviour
{
    [System.Serializable]
    public struct Buildings
    {
        public GameObject card;
        public string name;
        public string description;
        public Sprite buildingThumb;
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

    void Start()
    {
        cardPanel = transform.Find("BuildingCards");

        InitializeCards();
    }


    void Update()
    {
        
    }

    private void InitializeCards()
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            // Instantiate building cards
            buildings[i].card = Instantiate(buildingCardPrefab);
            buildings[i].card.transform.SetParent(cardPanel);
            buildings[i].card.transform.localScale = Vector3.one;

            // Set info on building cards
            buildings[i].card.transform.Find("BuildingName").GetComponent<TMP_Text>().text = buildings[i].name;
            buildings[i].card.transform.Find("ResearchInfo/Description").GetComponent<TMP_Text>().text = buildings[i].description;
            buildings[i].card.transform.Find("ResearchInfo/Price").GetComponent<TMP_Text>().text = buildings[i].price.ToString();

            // Instantiate upgrade cards
            for (int j = 0; j < buildings[i].upgrades.Length; j++)
            {
                // Instantiate upgrade cards
                buildings[i].upgrades[j].card = Instantiate(upgradeCardPrefab);
                buildings[i].upgrades[j].card.transform.SetParent(buildings[i].card.transform.Find("Upgrades/UpgradesStandard"));
                buildings[i].upgrades[j].card.transform.localScale = Vector3.one;

                // Set info on upgrade cards
                buildings[i].upgrades[j].card.transform.Find("UpgradeName").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].name;
                buildings[i].upgrades[j].card.transform.Find("UpgradeDesc").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].description;
                buildings[i].upgrades[j].card.transform.Find("Price").GetComponent<TMP_Text>().text = buildings[i].upgrades[j].price.ToString();
            }

            // Hide building purchase info when already purchased
            GameObject locked = buildings[i].card.transform.Find("ResearchInfo").gameObject;
            GameObject unlocked = buildings[i].card.transform.Find("Upgrades").gameObject;
            if (buildings[i].purchased)
            {
                locked.SetActive(false);
            }
            else
            {
                unlocked.SetActive(false);
            }
        }
    }

    //private Sprite FindIcon(string name)
    //{
    //    switch (name)
    //    {

    //        default:
    //            break;
    //    }


    //}
}
