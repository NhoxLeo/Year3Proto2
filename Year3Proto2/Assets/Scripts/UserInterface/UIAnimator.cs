using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIAnimator : MonoBehaviour
{
    public bool interactable = true;
    public bool showElement = true;
    bool elementShown;

    private CanvasGroup canvas;
    private RectTransform rTransform;
    public float width;
    public float height;

    private Sequence entranceSequence;
    private Sequence pulseSeq;
    private bool initialized;

    public enum EntranceAnimation
    {
        Window,
        PopUp,
        PopDown
    }

    public enum PulseAnimation
    {
        Pop,
        VerticalShake,
        HorizontalShake
    }

    public EntranceAnimation entrance;
    public PulseAnimation pulse;

    public bool playAudio;
    public AudioClip toolSound;
    private AudioSource audioSource;
    private bool soundSet;


    void Start()
    {

        rTransform = GetComponent<RectTransform>();
        width = rTransform.rect.width;
        height = rTransform.rect.height;
        //rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.0f);

        canvas = GetComponent<CanvasGroup>();
        canvas.interactable = interactable;
        canvas.blocksRaycasts = interactable;

        if (!showElement)
        {
            EntranceInitialize();
        }
    }


    void Update()
    {
        if (showElement && !elementShown)
        {
            Entrance();
            elementShown = true;
        }

        if (!showElement && elementShown)
        {
            Exit();
            elementShown = false;
        }

    }

    private void EntranceInitialize()
    {
        switch (entrance)
        {
            case EntranceAnimation.Window:
                transform.DOScale(0.8f, 0.0f);
                canvas.alpha = 0.0f;
                canvas.interactable = false;
                canvas.blocksRaycasts = false;
                break;
            case EntranceAnimation.PopUp:
                break;
            case EntranceAnimation.PopDown:
                break;
            default:
                break;
        }

    }

    private void Entrance()
    {
        entranceSequence.Kill(true);

        switch (entrance)
        {
            case EntranceAnimation.Window:

                entranceSequence = DOTween.Sequence();
                entranceSequence.Insert(0.0f, transform.DOScale(1.0f, 0.4f)).SetEase(Ease.OutBack);
                entranceSequence.Insert(0.0f, canvas.DOFade(1.0f, 0.3f)).SetEase(Ease.OutQuint);

                pulseSeq.Play();

                canvas.interactable = interactable;
                canvas.blocksRaycasts = interactable;
                break;
            case EntranceAnimation.PopUp:
                break;
            case EntranceAnimation.PopDown:
                break;
            default:
                break;
        }
    }

    private void Exit()
    {
        entranceSequence.Kill(true);

        switch (entrance)
        {
            case EntranceAnimation.Window:

                entranceSequence = DOTween.Sequence();
                entranceSequence.Insert(0.0f, transform.DOScale(0.8f, 0.3f)).SetEase(Ease.OutQuint);
                entranceSequence.Insert(0.0f, canvas.DOFade(0.0f, 0.3f)).SetEase(Ease.OutQuint);

                pulseSeq.Play();

                canvas.interactable = false;
                canvas.blocksRaycasts = false;
                break;
            case EntranceAnimation.PopUp:
                break;
            case EntranceAnimation.PopDown:
                break;
            default:
                break;
        }
    }


}
