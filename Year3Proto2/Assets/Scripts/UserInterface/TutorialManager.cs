using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance = null;

    public enum TutorialState
    {
        Start,
        SelectFarm,
        PlaceFarm
    }

    public TutorialState tutorialState;

    [System.Serializable]
    public struct TutorialMessage
    {
        public string heading;
        public string description;
    }

    [SerializeField] private List<TutorialMessage> tutorialMessages;

    [Header("Main")]
    [SerializeField] private RectTransform focus;

    [SerializeField] private RectTransform focusTransform;
    [SerializeField] private UIAnimator messagePanel;
    [SerializeField] private RectTransform messageTransform;
    [SerializeField] private TMP_Text messageHeading;
    [SerializeField] private TMP_Text messageDescription;

    [Header("Transforms")]
    [SerializeField] private RectTransform farmButton;

    [SerializeField] private RectTransform productionTab;
    [SerializeField] private UIAnimator productionTabPanel;

    private float pulseScaleMagnitude = 0.05f;

    private void Awake()
    {
        instance = this;

        //tutorialMessages = new List<TutorialMessage>();
    }

    public static TutorialManager GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceTutorialTo(TutorialState.SelectFarm);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            AdvanceTutorialTo(TutorialState.PlaceFarm);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            AdvanceTutorialTo(TutorialState.Start);
        }

        focus.transform.localScale = Vector3.one * (1.0f + pulseScaleMagnitude + Mathf.Sin(Time.time * 6.0f) * pulseScaleMagnitude);
        if (focusTransform != null)
        {
            //focus.position = Vector3.Lerp(focus.position, focusTransform.position, Time.unscaledDeltaTime * 10.0f);
            focus.position = focusTransform.position;
        }

        switch (tutorialState)
        {
            case TutorialState.Start:

                break;

            case TutorialState.SelectFarm:

                if (productionTabPanel.showElement && focusTransform != farmButton)
                {
                    FocusOn(farmButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
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
        focus.DOSizeDelta(_rTrans.sizeDelta, 0.4f).SetEase(Ease.OutQuint);
        focusTransform = _rTrans;
    }

    public void AdvanceTutorialTo(TutorialState _state)
    {
        tutorialState = _state;

        SetMessage((int)tutorialState);
    }

    private void SetMessage(int _message)
    {
        messageHeading.text = tutorialMessages[_message].heading;
        messageDescription.text = tutorialMessages[_message].description;
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageTransform);

        messagePanel.SetVisibility(!(tutorialMessages[_message].heading == "" && tutorialMessages[_message].description == ""));
        messagePanel.Pulse();
    }
}