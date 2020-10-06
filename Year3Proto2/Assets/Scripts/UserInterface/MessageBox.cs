using UnityEngine;
using TMPro;

public class MessageBox : MonoBehaviour
{
    private Tooltip tool;
    private float timer;
    private bool timerMode;

    private TMP_Text displayText;

    void Awake()
    {
        tool = GetComponent<Tooltip>();
        displayText = transform.GetComponentInChildren<TMP_Text>();
        timerMode = true;
        timer = 0.0f;
    }


    void Update()
    {
        if (SuperManager.messageBox)
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
        }
    }

    public void ShowMessage(string message, float time = 0f)
    {
        if (SuperManager.messageBox)
        {
            if ((timerMode && timer <= 0f) || !timerMode)
            {
                if (tool)
                {
                    //Debug.Log("tool returns.");
                    if (tool.showTooltip)
                    {
                        //Debug.Log("tool.showTooltip is true.");
                    }
                    else
                    {
                        //Debug.Log("tool.showTooltip is false.");
                    }
                }
                else
                {
                    Debug.LogError("tool returns null.");
                }
                if (displayText)
                {
                    //Debug.Log("displayText returns.");
                    //Debug.Log("displayText.text == " + displayText.text);
                }
                else
                {
                    Debug.LogError("displayText returns null.");
                }

                if (tool.showTooltip && displayText.text != message) { tool.PulseTip(); }
                displayText.text = message;

                timer = time;
                timerMode = time != 0f;
            }
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
