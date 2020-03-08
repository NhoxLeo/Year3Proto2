using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tooltip : MonoBehaviour
{
    public bool showTooltip;
    private bool tipShown;

    private CanvasGroup canvas;
    private RectTransform rTransform;
    public float width;
    public float height;

    private Sequence pulseSeq;

    void Start()
    {
        rTransform = GetComponent<RectTransform>();
        width = rTransform.rect.width;
        height = rTransform.rect.height;
        rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.0f);

        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = 0.0f;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
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
        PulseTip();

        rTransform.DOSizeDelta(new Vector2(width, height), 0.25f).SetEase(Ease.OutQuint);
        canvas.DOKill(true);
        canvas.DOFade(1.0f, 0.15f);

        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    private void HideTip()
    {
        rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.25f).SetEase(Ease.OutQuint);
        canvas.DOKill(true);
        canvas.DOFade(0.0f, 0.15f).SetEase(Ease.OutQuint);

        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }

    public void PulseTip()
    {
        pulseSeq.Kill(true);
        pulseSeq = DOTween.Sequence()
            .Append(transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f));

        pulseSeq.Play();
    }
}
