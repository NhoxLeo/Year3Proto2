using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelEndscreen : MonoBehaviour
{
    public Color victoryColour;
    public Color defeatColour;

    private bool showingVictory;
    private bool showingDefeat;

    private SuperManager superMan;

    private void Start()
    {
        superMan = SuperManager.GetInstance();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowVictoryScreen();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ShowDeafeatScreen();
        }
    }

    private void GetVictoryInfo()
    {
        //transform.Find("LevelModCard/Title").GetComponent<TMP_Text>().text = // win title
        //transform.Find("LevelModCard/Description").GetComponent<TMP_Text>().text = // win description
        //transform.Find("LevelModCard/Price").GetComponent<TMP_Text>().text = // win value

        //transform.Find("ModBonus").GetComponent<TMP_Text>().text = // modifer total bonus
        //transform.Find("Reward").GetComponent<TMP_Text>().text = // total reward
    }

    public void ShowVictoryScreen()
    {
        GetVictoryInfo();

        transform.Find("Defeat").gameObject.SetActive(false);
        transform.Find("Victory").gameObject.SetActive(true);
        GetComponent<Tooltip>().showTooltip = true;
        GetComponent<Image>().color = victoryColour;
        showingVictory = true;
    }

    public void ShowDeafeatScreen()
    {
        transform.Find("Victory").gameObject.SetActive(false);
        transform.Find("Defeat").gameObject.SetActive(true);
        GetComponent<Tooltip>().showTooltip = true;
        GetComponent<Image>().color = defeatColour;
        showingDefeat = true;
    }

    public void HideEndscreen()
    {
        transform.Find("Victory").gameObject.SetActive(false);
        transform.Find("Defeat").gameObject.SetActive(false);
        GetComponent<Tooltip>().showTooltip = false;
        showingVictory = false;
        showingDefeat = false;
    }
}
