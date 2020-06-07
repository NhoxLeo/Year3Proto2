using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelEndscreen : MonoBehaviour
{
    public Color victoryColour;
    public Color defeatColour;

    private bool showingVictory;
    private bool showingDefeat;

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
        GetComponent<Tooltip>().showTooltip = true;
        showingVictory = false;
        showingDefeat = false;
    }
}
