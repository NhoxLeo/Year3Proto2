using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResearchManager : MonoBehaviour
{
    SuperManager superMan;
    TMP_Text researchPointsText;

    // Start is called before the first frame update
    void Start()
    {
        superMan = SuperManager.GetInstance();
        researchPointsText = GameObject.Find("CurrentResearchPoints").GetComponent<TMP_Text>();
        researchPointsText.text = "Current Research Points: " + superMan.saveData.researchPoints.ToString();
        UpdateButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            if (int.TryParse(button.gameObject.name.Substring(1, 1), out int _ID))
            {
                if (superMan.GetResearchComplete(_ID))
                {
                    button.interactable = false;
                }
                else if (superMan.researchDefinitions[_ID].reqID == -1)
                {
                    button.interactable = true;
                }
                else if (superMan.GetResearchComplete(superMan.researchDefinitions[_ID].reqID))
                {
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;
                }
            }
        }
    }

    public void Research(int _ID)
    {
        if (superMan.AttemptResearch(_ID))
        {
            researchPointsText.text = "Current Research Points: " + superMan.saveData.researchPoints.ToString();
            UpdateButtons();
        }
    }

    public void BackToMenu()
    {
        FindObjectOfType<SceneSwitcher>().SceneSwitch("TitleScreen");
    }
}
