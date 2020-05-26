using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapScreen : MonoBehaviour
{
    public int selectedLevel;

    [System.Serializable]
    public struct Level
    {
        public bool completed;
        public bool locked;
        public bool inProgress;
        public string victoryTitle;
        public string victoryDescription;
        public int victoryValue;
        public int reward;
        public int bonusTotal;
        public List<Modifier> modifiers;
    }
    [SerializeField]
    private List<Level> levels;

    [System.Serializable]
    public struct Modifier
    {
        public string title;
        public string description;
        public float modBonus;
    }

    public struct LevelBadge
    {
        public Button button;
        public TMP_Text levelNumber;
        public Tooltip selectionOutline;
        public GameObject checkmark;
        public GameObject progressIndicator;
    }
    private List<LevelBadge> levelBadges;

    private TMP_Text RPCounter;

    public GameObject modPrefab;
    public Sprite victoryIcon;
    public Sprite modifierIcon;

    private Transform levelGroup;
    private Transform levelPanel;
    private Transform modifierGroup;

    private void Start()
    {
        levelGroup = transform.Find("Levels");
        RPCounter = transform.Find("RPCounter").GetComponent<TMP_Text>();
        levelPanel = transform.Find("LevelPanel");
        modifierGroup = transform.Find("LevelPanel/Modifiers");

        InitializeLevels();
    }

    private void Update()
    {

    }

    private void InitializeLevels()
    {
        levelBadges = new List<LevelBadge>();

        for (int i = 0; i < levels.Count; i++)
        {
            // Get references for each level badge
            Transform badge = levelGroup.GetChild(i);

            if (badge != null)
            {
                LevelBadge newBadge;

                newBadge.button = badge.GetComponent<Button>();
                newBadge.levelNumber = badge.GetComponentInChildren<TMP_Text>();
                newBadge.selectionOutline = badge.GetComponentInChildren<Tooltip>();
                newBadge.checkmark = badge.transform.Find("Check").gameObject;
                newBadge.progressIndicator = badge.transform.Find("InProgress").gameObject;

                levelBadges.Add(newBadge);
            }

        }

        SetSelectedLevel(1);
    }

    private void SetLevelPanel() 
    {
        // Set level badge of level panel
        levelPanel.Find("LevelBadge/LevelText").GetComponent<TMP_Text>().text = selectedLevel.ToString();
        levelPanel.Find("LevelBadge/Check").gameObject.SetActive(levels[selectedLevel - 1].completed);
        levelPanel.Find("LevelBadge/InProgress").gameObject.SetActive(levels[selectedLevel - 1].inProgress);

        // Set victory info
        levelPanel.Find("VictoryCard/Title").GetComponent<TMP_Text>().text = levels[selectedLevel - 1].victoryTitle;
        levelPanel.Find("VictoryCard/Description").GetComponent<TMP_Text>().text = levels[selectedLevel - 1].victoryDescription;
        levelPanel.Find("VictoryCard/Price").GetComponent<TMP_Text>().text = levels[selectedLevel - 1].victoryValue.ToString();

        // Destroy old modifier cards
        for (int i = 0; i < modifierGroup.childCount; i++)
        {
            Destroy(modifierGroup.GetChild(i).gameObject);
        }

        // Setup modifier cards
        float modToalValue = 0.0f;
        for (int i = 0; i < levels[selectedLevel - 1].modifiers.Count; i++)
        {
            // Instantiate
            GameObject card = Instantiate(modPrefab);
            card.transform.SetParent(modifierGroup);
            card.transform.localScale = Vector3.one;

            // Set info
            card.transform.Find("Title").GetComponent<TMP_Text>().text = levels[selectedLevel - 1].modifiers[i].title;
            card.transform.Find("Description").GetComponent<TMP_Text>().text = levels[selectedLevel - 1].modifiers[i].description;
            card.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + (levels[selectedLevel - 1].modifiers[i].modBonus * 100).ToString("0") + "%";
            modToalValue += levels[selectedLevel - 1].modifiers[i].modBonus;
        }

        // Set total bonus and reward text
        levelPanel.Find("ModifierTotal").GetComponent<TMP_Text>().text = "+" + modToalValue * 100 + "%";
        levelPanel.Find("Reward").GetComponent<TMP_Text>().text = (levels[selectedLevel - 1].victoryValue * (1.0f + modToalValue)).ToString("0");

        // Adjust text on button
        TMP_Text buttonText = levelPanel.Find("Button/Text").GetComponent<TMP_Text>();
        buttonText.text = levels[selectedLevel - 1].completed ? "REPLAY" : "CONQUER";
        if (levels[selectedLevel - 1].inProgress)
        {
            buttonText.text = "CONTINUE";
        }

    }

    public void SetSelectedLevel(int _level)
    {
        selectedLevel = _level;

        for (int i = 0; i < levelBadges.Count; i++)
        {
            // Set values of level badges
            //if (i > 0) { levelBadges[i].button.interactable = (levels[i - 1].completed); }
            levelBadges[i].button.interactable = !levels[i].locked;
            levelBadges[i].levelNumber.text = (i + 1).ToString();
            levelBadges[i].selectionOutline.showTooltip = (i == selectedLevel - 1);
            levelBadges[i].selectionOutline.PulseTip();
            levelBadges[i].checkmark.SetActive(levels[i].completed);
            levelBadges[i].progressIndicator.SetActive(levels[i].inProgress);
        }

        levelPanel.GetComponent<Tooltip>().PulseTip();

        SetLevelPanel();
    }


}