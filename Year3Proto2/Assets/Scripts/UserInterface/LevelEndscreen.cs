using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class LevelEndscreen : MonoBehaviour
{
    public Color victoryColour;
    public Color defeatColour;

    [SerializeField] private GameObject keepPlayingButton = null;
    [SerializeField] private GameObject researchButton = null;
    [SerializeField] private GameObject gameEndTitleButton = null;

    public bool showingVictory = false;
    public bool showingDefeat = false;

    private bool alreadyComplete = false;


    private void Start()
    {
        alreadyComplete = GameManager.GetInstance().gameAlreadyWon;
    }

    private void GetVictoryInfo()
    {
        List<MapScreen.Level> levels = new List<MapScreen.Level>();
        SuperManager superMan = SuperManager.GetInstance();
        superMan.GetLevelData(ref levels);
        int currentLevel = superMan.GetCurrentLevel();
        transform.Find("Victory/LevelModCard/Title").GetComponent<TMP_Text>().text = levels[currentLevel].victoryTitle;
        transform.Find("Victory/LevelModCard/Description").GetComponent<TMP_Text>().text = levels[currentLevel].victoryDescription;
        transform.Find("Victory/LevelModCard/Price").GetComponent<TMP_Text>().text = levels[currentLevel].victoryValue.ToString();

        transform.Find("Victory/ModBonus").GetComponent<TMP_Text>().text = "+" + levels[currentLevel].GetTotalCoefficient() * 100 + "%";
        transform.Find("Victory/Reward").GetComponent<TMP_Text>().text = levels[currentLevel].reward.ToString();
    }

    public void ShowVictoryScreen()
    {
        GetVictoryInfo();
        if (SuperManager.GetInstance().GetCurrentLevel() == 3 && !alreadyComplete)
        {
            GlobalData.gameEnd = true;
        }

        transform.Find("Defeat").gameObject.SetActive(false);
        transform.Find("Victory").gameObject.SetActive(true);
        GetComponent<Tooltip>().showTooltip = true;
        GetComponent<Image>().color = victoryColour;

        if (GlobalData.gameEnd)
        {
            keepPlayingButton.SetActive(false);
            researchButton.SetActive(false);
            gameEndTitleButton.SetActive(true);
        }
        else
        {
            keepPlayingButton.SetActive(true);
            researchButton.SetActive(true);
            gameEndTitleButton.SetActive(false);
        }

        showingVictory = true;
        Time.timeScale = 0f;

        FindObjectOfType<HUDManager>().doShowHUD = false;
        FindObjectOfType<EnemyManager>().SetSpawning(false);
    }

    public void ShowDeafeatScreen()
    {
        transform.Find("Victory").gameObject.SetActive(false);
        transform.Find("Defeat").gameObject.SetActive(true);
        GetComponent<Tooltip>().showTooltip = true;
        GetComponent<Image>().color = defeatColour;
        showingDefeat = true;
        Time.timeScale = 0f;

        FindObjectOfType<HUDManager>().doShowHUD = false;
        FindObjectOfType<EnemyManager>().SetSpawning(false);
    }

    public void HideEndscreen()
    {
        transform.Find("Victory").gameObject.SetActive(false);
        transform.Find("Defeat").gameObject.SetActive(false);
        GetComponent<Tooltip>().showTooltip = false;
        showingVictory = false;
        showingDefeat = false;
        Time.timeScale = 1f;
        FindObjectOfType<HUDManager>().doShowHUD = true;
        FindObjectOfType<EnemyManager>().SetSpawning(true);
    }

    public void DefeatGoToLevelSelect()
    {
        SuperManager superMan = SuperManager.GetInstance();
        superMan.ClearCurrentMatch();
        superMan.OnBackToMenus();
        FindObjectOfType<SceneSwitcher>().SceneSwitch("LevelSelect");
    }

    public void VictoryGoToLevelSelect()
    {
        SuperManager superMan = SuperManager.GetInstance();
        superMan.SaveCurrentMatch();
        superMan.OnBackToMenus();
        FindObjectOfType<SceneSwitcher>().SceneSwitch("ResearchTree");
    }

    public void VictoryGoToTitle()
    {
        SuperManager superMan = SuperManager.GetInstance();
        superMan.SaveCurrentMatch();
        superMan.OnBackToMenus();
        GlobalData.gameEnd = false;
        FindObjectOfType<SceneSwitcher>().SceneSwitch("ResearchTree");
    }
}
