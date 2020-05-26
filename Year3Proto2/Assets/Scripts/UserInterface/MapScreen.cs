using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [System.Serializable]
    public struct LevelBadge
    {
        public TMP_Text levelNumber;
        public Tooltip selectionOutline;
        public GameObject checkmark;
        public GameObject progressIndicator;
    }
    [SerializeField]
    private List<LevelBadge> levelBadges;

    private TMP_Text RPCounter;

    public GameObject modPrefab;
    public Sprite victoryIcon;
    public Sprite modifierIcon;

    private Transform levelGroup;
    private Transform levelPanel;
    private Transform modifierGroup;
    private TMP_Text bonusTotalText;
    private TMP_Text rewardText;
    private TMP_Text startButtonText;

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

                newBadge.levelNumber = badge.GetComponentInChildren<TMP_Text>();
                newBadge.selectionOutline = badge.GetComponentInChildren<Tooltip>();
                newBadge.checkmark = badge.transform.Find("Check").gameObject;
                newBadge.progressIndicator = badge.transform.Find("InProgress").gameObject;

                levelBadges.Add(newBadge);
            }

            SetSelectedLevel(1);
        }
    }

    public void SetSelectedLevel(int _level)
    {
        selectedLevel = _level;

        for (int i = 0; i < levelBadges.Count; i++)
        {
            // Set values of level badges
            levelBadges[i].levelNumber.text = (i + 1).ToString();
            levelBadges[i].selectionOutline.showTooltip = (i == selectedLevel - 1);
            levelBadges[i].selectionOutline.PulseTip();
            levelBadges[i].checkmark.SetActive(levels[i].completed);
            levelBadges[i].progressIndicator.SetActive(levels[i].inProgress);
        }
    }
}