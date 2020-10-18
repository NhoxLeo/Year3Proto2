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
        public List<Modifier> modifiers;

        public int reward;

        public float GetTotalCoefficient()
        {
            float total = 0f;
            foreach (Modifier mod in modifiers) { total += mod.modBonus; }
            return total;
        }
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
    private int loadingFrameCounter = 0;

    private void Start()
    {
        Time.timeScale = 1.0f;

        levelGroup = transform.Find("Levels");
        RPCounter = transform.Find("RPCounter").GetComponent<TMP_Text>();
        levelPanel = transform.Find("LevelPanel");
        modifierGroup = transform.Find("LevelPanel/Modifiers");

        levels = new List<Level>();
        SuperManager.GetInstance().GetLevelData(ref levels);

        InitializeLevels();
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
        SetSelectedLevel(SuperManager.GetInstance().GetSavedMatch().match ? SuperManager.GetInstance().GetSavedMatch().levelID : 0);
    }

    private void LateUpdate()
    {
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

    private void RefreshLevelPanel()
    {
        // Set level badge of level panel
        levelPanel.Find("LevelBadge/LevelText").GetComponent<TMP_Text>().text = (selectedLevel + 1).ToString();
        levelPanel.Find("LevelBadge/Check").gameObject.SetActive(levels[selectedLevel].completed);
        levelPanel.Find("LevelBadge/InProgress").gameObject.SetActive(levels[selectedLevel].inProgress);

        // Set victory info
        levelPanel.Find("VictoryCard/Title").GetComponent<TMP_Text>().text = levels[selectedLevel].victoryTitle;
        levelPanel.Find("VictoryCard/Description").GetComponent<TMP_Text>().text = levels[selectedLevel].victoryDescription;
        levelPanel.Find("VictoryCard/Price").GetComponent<TMP_Text>().text = levels[selectedLevel].victoryValue.ToString();

        // Destroy old modifier cards
        for (int i = 0; i < modifierGroup.childCount; i++)
        {
            Destroy(modifierGroup.GetChild(i).gameObject);
        }

        // Setup modifier cards
        for (int i = 0; i < levels[selectedLevel].modifiers.Count; i++)
        {
            // Instantiate
            GameObject card = Instantiate(modPrefab);
            card.transform.SetParent(modifierGroup);
            card.transform.localScale = Vector3.one;

            // Set info
            card.transform.Find("Title").GetComponent<TMP_Text>().text = levels[selectedLevel].modifiers[i].title;
            card.transform.Find("Description").GetComponent<TMP_Text>().text = levels[selectedLevel].modifiers[i].description;
            card.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + (levels[selectedLevel].modifiers[i].modBonus * 100).ToString("0") + "%";
        }

        // Set total bonus and reward text
        levelPanel.Find("ModifierTotal").GetComponent<TMP_Text>().text = "+" + levels[selectedLevel].GetTotalCoefficient() * 100 + "%";
        levelPanel.Find("Reward").GetComponent<TMP_Text>().text = levels[selectedLevel].reward.ToString("0");

        // Adjust text on button
        TMP_Text buttonText = levelPanel.Find("Button/Text").GetComponent<TMP_Text>();
        buttonText.text = levels[selectedLevel].completed ? "REPLAY" : "CONQUER";
        if (levels[selectedLevel].inProgress)
        {
            buttonText.text = "CONTINUE";
        }

        RPCounter.text = SuperManager.GetInstance().GetResearchPoints().ToString();
    }

    public void SetSelectedLevel(int _level)
    {
        selectedLevel = _level;

        UpdateLevelBadges();

        levelPanel.GetComponent<Tooltip>().PulseTip();

        RefreshLevelPanel();
    }

    private void UpdateLevelBadges()
    {
        for (int i = 0; i < levelBadges.Count; i++)
        {
            // Set values of level badges
            levelBadges[i].button.interactable = !levels[i].locked;
            levelBadges[i].levelNumber.text = (i + 1).ToString();
            levelBadges[i].selectionOutline.showTooltip = i == selectedLevel;
            levelBadges[i].selectionOutline.PulseTip();
            levelBadges[i].checkmark.SetActive(levels[i].completed);
            levelBadges[i].progressIndicator.SetActive(levels[i].inProgress);
        }
    }

    public void GoToLevel()
    {
        // if we're conquering we need to go to a new level
        // but if we're resuming we need to load 
        SuperManager.GetInstance().PlayLevel(selectedLevel);
    }
}