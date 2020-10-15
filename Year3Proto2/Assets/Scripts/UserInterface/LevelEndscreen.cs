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

    public bool showingVictory = false;
    public bool showingDefeat = false;

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowVictoryScreen();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowDeafeatScreen();
        }
        */
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

        transform.Find("Defeat").gameObject.SetActive(false);
        transform.Find("Victory").gameObject.SetActive(true);
        GetComponent<Tooltip>().showTooltip = true;
        GetComponent<Image>().color = victoryColour;
        showingVictory = true;

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

        FindObjectOfType<HUDManager>().doShowHUD = true;
        FindObjectOfType<EnemyManager>().SetSpawning(true);
    }

    public void DefeatGoToLevelSelect()
    {
        SuperManager.GetInstance().ClearCurrentMatch();
        FindObjectOfType<SceneSwitcher>().SceneSwitch("LevelSelect");
    }

    public void VictoryGoToLevelSelect()
    {
        SuperManager.GetInstance().SaveCurrentMatch();
        FindObjectOfType<SceneSwitcher>().SceneSwitch("ResearchTree");
    }
}
