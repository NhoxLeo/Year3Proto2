﻿using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private UIAnimator tool;
    public bool isPaused;
    public bool isHelp;
    public bool isActive;

    private UIAnimator currentAnimator;

    private void Start()
    {
        tool = GetComponent<UIAnimator>();
        isPaused = false;
        isHelp = SuperManager.GetInstance().GetShowTutorial();
        isActive = false;
    }

    private void Update()
    {
        if (!isHelp)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            Time.timeScale = isPaused ? 0.0f : 1.0f;
        }
    }

    public void Paused(bool _paused)
    {
        if (_paused)
        {
            SuperManager.GetInstance().OnPause();
        }
        else
        {
            SuperManager.GetInstance().OnResume();
        }
        HUDManager.GetInstance().SetHUD(!_paused);
        tool.showElement = _paused;
        isPaused = _paused;
        GlobalData.isPaused = _paused;
    }

    public void PausedButtons(bool _paused)
    {
        tool.showElement = _paused;
    }

    public void PausedActive(bool _paused)
    {
        if(!isHelp)
        {
            isActive = _paused;
            tool.showElement = !isActive;
        }
        else
        {
            isHelp = false;
        }
    }

    public void SetCurrentAnimator(UIAnimator _animator)
    {
        currentAnimator = _animator;
    }

    public void TogglePause()
    {
        LevelEndscreen levelEndscreen = FindObjectOfType<LevelEndscreen>();
        if (levelEndscreen)
        {
            if (levelEndscreen.showingVictory)
            {
                levelEndscreen.HideEndscreen();
            }
            else if (levelEndscreen.showingDefeat)
            {
                levelEndscreen.DefeatGoToLevelSelect();
            }
            else
            {
                if (isActive)
                {
                    if (currentAnimator != null)
                    {
                        currentAnimator.SetVisibility(false);
                    }
                    PausedButtons(true);
                    isActive = false;
                }
                else
                {
                    Paused(!isPaused);
                }
            }
        }
    }

    public void OnQuitToTitleScreen()
    {
        SuperManager.GetInstance().OnBackToMenus();
    }
}