using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnvInfo : MonoBehaviour
{
    private bool showInfo;

    private RectTransform rTrans;
    private Tooltip tool;
    private TMP_Text textbox;

    void Start()
    {
        rTrans = GetComponent<RectTransform>();
        tool = GetComponent<Tooltip>();
        tool.SetInteractable(false);
        textbox = transform.Find("PanelMask/Description").GetComponent<TMP_Text>();
    }


    void Update()
    {
        tool.showTooltip = showInfo;

        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowInfo("this is a test tooltip");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            showInfo = !showInfo;
        }
    }

    private void LateUpdate()
    {
        transform.position = Input.mousePosition;

        // Adjust position of info panel if near edge of bounds

        float xPivot = 0.0f;
        float yPivot = 0.5f;
        if (transform.localPosition.y > 400.0f)
        {
            yPivot = 1.0f;
        }
        else if (transform.localPosition.y < -220.0f)
        {
            yPivot = 0.0f;
        }

        if (transform.localPosition.x > 640.0f)
        {
            xPivot = 1.0f;
        }
        // Smooth Lerping motion
        float dt = Time.unscaledDeltaTime;
        Vector2 pivot = new Vector2(Mathf.Lerp(rTrans.pivot.x, xPivot, dt * 10.0f), Mathf.Lerp(rTrans.pivot.y, yPivot, dt * 10.0f));
        rTrans.pivot = pivot;
    }

    public void ShowInfo(string info)
    {
        if (!textbox)
        {
            //Debug.LogError("textbox implicit bool cast returned false");
            return;
        }
        textbox.text = info;
        tool.SetHeight(tool.height);
    }

    public void SetVisibility(bool isVisible)
    {
        showInfo = isVisible;
    }

}
