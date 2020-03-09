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
        tool = GetComponent<Tooltip>();
        displayText = transform.GetChild(1).GetComponent<TMP_Text>();
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

        if (Input.GetKeyDown(KeyCode.M))
        {
            ShowMessage("You pressed M, thus triggering a test messege!", 3.5f);
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
