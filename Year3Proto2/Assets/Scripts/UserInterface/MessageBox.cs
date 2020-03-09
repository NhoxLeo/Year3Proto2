using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageBox : MonoBehaviour
{
    private Tooltip tool;
    private float timer;

    private TMP_Text displayText;

    void Start()
    {
        displayText = GetComponent<TMP_Text>();
    }


    void Update()
    {
        if (timer > 0.0f)
        {
            timer -= Time.unscaledDeltaTime;
            tool.showTooltip = true;
        }
        else
        {
            tool.showTooltip = false;
        }
    }

    public void ShowMessage(string message, float time)
    {
        displayText.text = message;
        timer = time;

        if (tool.showTooltip)
            tool.PulseTip();
    }
}
