//
// Bachelor of Creative Technologies
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name        : UIAnimator.cs
// Description      : Reusable class that handles Entrance Exit and Pulse UI animations
// Author           : David Morris
// Mail             : David.Mor7851@mediadesign.school.nz
//

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
    //private RectTransform rTransform;
    public float width;
    public float height;

    private Sequence entranceSequence;
    private Sequence pulseSeq;
    private bool initialized;

    public enum EntranceAnimation
    {
        Window,
        Window2,
        PopUp,
        PopDown
    }

    public enum PulseAnimation
    {
        Pop,
        VerticalShake,
        HorizontalShake,
        RightShake
    }

    public EntranceAnimation entrance;
    public PulseAnimation pulse;

    public bool playAudio;
    public AudioClip toolSound;
    private AudioSource audioSource;
    private bool soundSet;


    void Awake()
    {
        //rTransform = GetComponent<RectTransform>();
        //width = rTransform.rect.width;
        //height = rTransform.rect.height;
        //rTransform.DOSizeDelta(new Vector2(64.0f, height), 0.0f);

        canvas = GetComponent<CanvasGroup>();
        canvas.interactable = interactable;
        canvas.blocksRaycasts = interactable;

    }

    private void Start()
    {
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

    public void SetVisibility(bool _visible)
    {
        showElement = _visible;
    }

    public void EntranceInitialize()
    {
        canvas = GetComponent<CanvasGroup>();
        canvas.interactable = interactable;
        canvas.blocksRaycasts = interactable;

        switch (entrance)
        {
            case EntranceAnimation.Window:
                transform.DOScale(0.8f, 0.0f);
                canvas.alpha = 0.0f;
                canvas.interactable = false;
                canvas.blocksRaycasts = false;
                break;
            case EntranceAnimation.Window2:
                transform.DOScale(1.2f, 0.0f);
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
            case EntranceAnimation.Window2:

                entranceSequence = DOTween.Sequence();
                entranceSequence.Insert(0.0f, transform.DOScale(1.0f, 0.4f)).SetEase(Ease.OutBack);
                entranceSequence.Insert(0.0f, canvas.DOFade(1.0f, 0.3f)).SetEase(Ease.OutQuad);

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
            case EntranceAnimation.Window2:

                entranceSequence = DOTween.Sequence();
                entranceSequence.Insert(0.0f, transform.DOScale(1.2f, 0.3f)).SetEase(Ease.OutQuint);
                entranceSequence.Insert(0.0f, canvas.DOFade(0.0f, 0.3f)).SetEase(Ease.OutQuad);

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

    public void Pulse()
    {
        switch (pulse)
        {
            case PulseAnimation.Pop:
                pulseSeq.Kill(true);
                pulseSeq = DOTween.Sequence()
                    .Append(transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f));

                pulseSeq.Play();
                break;
            case PulseAnimation.VerticalShake:
                break;
            case PulseAnimation.HorizontalShake:
                break;
            case PulseAnimation.RightShake:
                pulseSeq.Kill(true);
                pulseSeq = DOTween.Sequence()
                    .Append(transform.DOPunchPosition(Vector3.right * 12.0f, 0.25f, 3, 1.0f));

                pulseSeq.Play();
                break;
            default:
                break;
        }


    }

    private void OnDestroy()
    {
        entranceSequence.Kill(true);
        pulseSeq.Kill(true);
    }

}
