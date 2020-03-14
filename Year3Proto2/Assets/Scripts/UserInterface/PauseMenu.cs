using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    private Tooltip tool;

    void Start()
    {
        tool = GetComponent<Tooltip>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        tool.showTooltip = !tool.showTooltip;
        GlobalData.isPaused = tool.showTooltip;
    }
}
