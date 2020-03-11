using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageBox : MonoBehaviour
{
    private Tooltip tool;
    private float timer;
    private bool timerMode;

    private TMP_Text displayText;

    void Start()
    {
        tool = GetComponent<Tooltip>();
        displayText = transform.GetChild(1).GetComponent<TMP_Text>();
        timerMode = true;
    }


    void Update()
    {
        if (timerMode)
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
        else
        {
            tool.showTooltip = true;
        }
        

        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowMessage("You pressed M, thus triggering a test message!", 3.5f);
        }
    }

    public void ShowMessage(string message, float time = 0f)
    {
        if ((timerMode && timer <= 0f) || !timerMode)
        {
            if (tool.showTooltip && displayText.text != message) { tool.PulseTip(); }
            displayText.text = message;

            timer = time;
            timerMode = time != 0f;

        }
    }

    public string GetCurrentMessage()
    {
        return displayText.text;
    }

    public void HideMessage()
    {
        timer = 0f;
        timerMode = true;
        tool.showTooltip = false;
    }
}
