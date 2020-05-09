using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour
{
    private SuperManager superMan;
    private int levelSelected;
    Button[] buttons;
    Button playButton;
    Image[] images;
    // Start is called before the first frame update
    void Start()
    {
        superMan = FindObjectOfType<SuperManager>();
        images = FindObjectsOfType<Image>();
        buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            if (button.name.Contains("Play"))
            {
                playButton = button;
            }
        }
        UpdateButtons();
        levelSelected = -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateButtons()
    {
        foreach (Button button in buttons)
        {
            if (int.TryParse(button.gameObject.name.Substring(1, 1), out int _ID))
            {
                button.interactable = superMan.CanPlayLevel(_ID);
            }
        }
        foreach (Image indicator in images)
        {
            if (indicator.gameObject.name.Contains("Indicator"))
            {
                if (int.TryParse(indicator.gameObject.name.Substring(1, 1), out int _ID))
                {
                    indicator.color = superMan.CanPlayLevel(_ID) ? Color.green : Color.red;
                }
            }
        }
    }

    public void AttemptSelectLevel(int _ID)
    {
        if (superMan.CanPlayLevel(_ID))
        {
            if (levelSelected == _ID)
            {
                playButton.interactable = false;
                foreach (Image indicator in images)
                {
                    if (indicator.gameObject.name.Contains("Indicator"))
                    {
                        if (int.TryParse(indicator.gameObject.name.Substring(1, 1), out int _imageID))
                        {
                            if (_imageID == _ID)
                            {
                                indicator.color = Color.green;
                            }
                        }
                    }
                }
                levelSelected = -1;
            }
            else
            {
                playButton.interactable = true;
                foreach (Image indicator in images)
                {
                    if (indicator.gameObject.name.Contains("Indicator"))
                    {
                        if (int.TryParse(indicator.gameObject.name.Substring(1, 1), out int _imageID))
                        {
                            if (_imageID == _ID)
                            {
                                indicator.color = Color.cyan;
                            }
                        }
                    }
                }
                levelSelected = _ID;
            }

        }
        
    }

    public void Play()
    { 
        if (levelSelected != -1)
        {
            superMan.currentLevel = levelSelected;
            switch (levelSelected)
            {
                case 1:
                    FindObjectOfType<SceneSwitcher>().SceneSwitch("SamDev");
                    break;
                case 2:
                    FindObjectOfType<SceneSwitcher>().SceneSwitch("SamDev");
                    break;
                case 3:
                    FindObjectOfType<SceneSwitcher>().SceneSwitch("SamDev");
                    break;
            }
        }
    }
}
