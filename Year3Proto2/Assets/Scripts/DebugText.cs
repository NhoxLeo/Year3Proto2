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
        if (!SuperManager.DevMode)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                debugTextEnabled = !debugTextEnabled;
                backgroundElement.gameObject.SetActive(debugTextEnabled);
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
        debugText += "\n\n" + VillagerManager.GetInstance().GetVillagerDebugInfo();
        debugText += "\n\n" + PathManager.GetInstance().GetPathfindingDebugInfo();
    }
}
