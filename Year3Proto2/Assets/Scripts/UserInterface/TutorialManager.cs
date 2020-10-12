using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance = null;

    [Header("Main")]
    [SerializeField] private RectTransform focus;
    [SerializeField] private RectTransform focusTransform;

    [Header("Transforms")]
    [SerializeField] private RectTransform farmButton;
    [SerializeField] private RectTransform productionTab;
    [SerializeField] private UIAnimator productionTabPanel;

    private float pulseScaleMagnitude = 0.05f;

    public enum TutorialState
    {
        Start,
        SelectFarm,
        PlaceFarm
    }
    public TutorialState tutorialState;

    private void Awake()
    {
        instance = this;
    }

    public static TutorialManager GetInstance()
    {
        return instance;
    }

    void Start()
    {
        
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceTutorialTo(TutorialState.SelectFarm);
        }

        focus.transform.localScale = Vector3.one * (1.0f + pulseScaleMagnitude + Mathf.Sin(Time.time * 6.0f) * pulseScaleMagnitude);

        switch (tutorialState)
        {
            case TutorialState.Start:
                break;

            case TutorialState.SelectFarm:
                if (productionTabPanel.showElement && focusTransform != farmButton)
                {
                    FocusOn(farmButton);
                }
                else
                {
                    FocusOn(productionTab);
                }
                break;

            case TutorialState.PlaceFarm:
                break;
            default:
                break;
        }
    }

    private void FocusOn(RectTransform _rTrans)
    {
        focus.sizeDelta = Vector2.one * 480.0f;
        focus.transform.DOKill(true);
        focus.DOMove(_rTrans.position, 0.4f).SetEase(Ease.OutQuint);
        focus.DOSizeDelta(_rTrans.sizeDelta, 0.4f).SetEase(Ease.OutQuint);
        focusTransform = _rTrans;
    }

    public void AdvanceTutorialTo(TutorialState _state)
    {
        tutorialState = _state;


    }

}
