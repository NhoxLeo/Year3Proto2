using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DebugText : MonoBehaviour
{
    private TMP_Text debugReadout;
    private string debugText;
    private bool debugTextEnabled;
    private Image backgroundElement;

    private void Awake()
    {
        backgroundElement = transform.GetChild(0).GetComponent<Image>();
        debugReadout = transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (SuperManager.DevMode)
        {
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
            {
                if (Input.GetKeyDown(KeyCode.I))
                {
                    debugTextEnabled = !debugTextEnabled;
                    backgroundElement.gameObject.SetActive(debugTextEnabled);
                }
            }
        }
        else
        {
            if (debugTextEnabled)
            {
                debugTextEnabled = false;
                backgroundElement.gameObject.SetActive(false);
            }
        }
        if (debugTextEnabled)
        {
            UpdateDebugText();
            debugReadout.text = debugText;
        }
    }

    private void UpdateDebugText()
    {
        debugText = InfoManager.GetStatsDebugInfo();
        debugText += "\n\n" + EnemyManager.GetInstance().GetEnemySpawnInfo();
        debugText += "\n\n" + VillagerManager.GetInstance().GetVillagerDebugInfo();
        //debugText += "\n\n" + PathManager.GetInstance().GetPathfindingDebugInfo();
    }
}
