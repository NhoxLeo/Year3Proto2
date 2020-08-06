using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    private UIAnimator tool;
    public bool isPaused;

    void Start()
    {
        tool = GetComponent<UIAnimator>();
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }



        Time.timeScale = isPaused ? 0.0f : 1.0f;
    }

    public void ToggleMenu()
    {
        tool.showElement = !tool.showElement;
        isPaused = !isPaused;
        GlobalData.isPaused = tool.showElement;
    }
}
