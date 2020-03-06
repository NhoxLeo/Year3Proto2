using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tooltip : MonoBehaviour
{
    public bool showTooltip;
    private bool tipShown;

    private RectTransform rTransform;
    private float width;
    private float height;

    private CanvasGroup canvas;

    void Start()
    {
        rTransform = GetComponent<RectTransform>();
        width = rTransform.rect.width;
        height = rTransform.rect.height;
        rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.0f);

        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = 0.0f;
    }


    void Update()
    {
        if (showTooltip && !tipShown)
        {
            ShowTip();
            tipShown = true;
        }

        if (!showTooltip && tipShown)
        {
            HideTip();
            tipShown = false;
        }

    }

    private void ShowTip()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f);
        rTransform.DOSizeDelta(new Vector2(width, height), 0.25f).SetEase(Ease.OutQuint);
        canvas.DOKill(true);
        canvas.DOFade(1.0f, 0.15f);
    }

    private void HideTip()
    {
        rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.25f).SetEase(Ease.OutQuint);
        canvas.DOKill(true);
        canvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);
    }

    public void PulseTip()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f);
    }
}
