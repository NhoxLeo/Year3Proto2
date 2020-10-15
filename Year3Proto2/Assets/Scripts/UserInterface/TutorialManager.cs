using DG.Tweening;
using System;
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
        PlaceFarm,
        SelectLumberMill,
        PlaceLumberMill,
        SelectMine,
        PlaceMine,
        VillagerAllocation,
        TrainVillager,
        VillagerPriority,
        SelectDefence,
        AllocateDefence,
        FinalMessage,
        End
    }

    public TutorialState tutorialState;
    private int tutorialLength;

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
    [SerializeField] private TMP_Text nextButtonText;
    [SerializeField] private Image progressBar;
    private bool updateLayout = false;

    [Header("Transforms")]
    [SerializeField] private RectTransform farmButton;
    [SerializeField] private RectTransform woodButton;
    [SerializeField] private RectTransform metalButton;
    [SerializeField] private RectTransform productionTab;
    [SerializeField] private UIAnimator productionTabPanel;
    [SerializeField] private RectTransform defenceTab;
    [SerializeField] private UIAnimator defenceTabPanel;
    [SerializeField] private RectTransform trainVillagerButton;
    [SerializeField] private RectTransform villagerPriorityPanel;
    [SerializeField] private RectTransform objectivePanel;

    private float pulseScaleMagnitude = 0.05f;

    private void Awake()
    {
        instance = this;

        //tutorialMessages = new List<TutorialMessage>();

        tutorialLength = Enum.GetNames(typeof(TutorialState)).Length;

        bool showTutorial = SuperManager.GetInstance().GetShowTutorial();
        GetInstance().AdvanceTutorialTo(showTutorial ? TutorialState.Start : TutorialState.End, true);
    }

    private void Start()
    {
        SetMessage((int)tutorialState);
    }

    public static TutorialManager GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            GoToNext();
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            GoToPrevious();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceTutorialTo(TutorialState.Start, true);
        }

        focus.transform.localScale = Vector3.one * (1.0f + pulseScaleMagnitude + Mathf.Sin(Time.time * 6.0f) * pulseScaleMagnitude);
        if (focusTransform != null)
        {
            focus.position = focusTransform.position;
        }

        switch (tutorialState)
        {
            case TutorialState.Start:
                FocusOn(null);
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
                FocusOn(null);

                break;

            case TutorialState.SelectLumberMill:
                if (productionTabPanel.showElement && focusTransform != woodButton)
                {
                    FocusOn(woodButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
                {
                    FocusOn(productionTab);
                }

                break;

            case TutorialState.PlaceLumberMill:
                FocusOn(null);
                break;

            case TutorialState.SelectMine:
                if (productionTabPanel.showElement && focusTransform != metalButton)
                {
                    FocusOn(metalButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
                {
                    FocusOn(productionTab);
                }
                break;

            case TutorialState.PlaceMine:
                FocusOn(null);
                break;

            case TutorialState.VillagerAllocation:
                FocusOn(null);
                break;

            case TutorialState.TrainVillager:
                FocusOn(trainVillagerButton);
                break;

            case TutorialState.VillagerPriority:
                FocusOn(villagerPriorityPanel);
                break;

            case TutorialState.SelectDefence:
                if (!defenceTabPanel.showElement && focusTransform != defenceTab)
                {
                    FocusOn(defenceTab);
                }
                break;

            case TutorialState.AllocateDefence:
                FocusOn(null);
                break;

            case TutorialState.FinalMessage:
                FocusOn(objectivePanel);
                break;

            case TutorialState.End:
                FocusOn(null);
                messagePanel.SetVisibility(false);
                break;

            default:
                FocusOn(null);

                break;
        }
    }

    private void LateUpdate()
    {
        if (updateLayout)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(messageTransform);
        }
    }

    private void FocusOn(RectTransform _rTrans)
    {
        if (_rTrans == null) 
        {
            focus.gameObject.SetActive(false);
        }
        else if (_rTrans != focusTransform)
        {
            focus.gameObject.SetActive(true);
            focus.sizeDelta = Vector2.one * 480.0f;
            focus.transform.DOKill(true);
            focus.DOSizeDelta(_rTrans.sizeDelta, 0.4f).SetEase(Ease.OutQuint);
        }

        focusTransform = _rTrans;
    }

    public void AdvanceTutorialTo(TutorialState _state, bool _allowBacktrack)
    {
        if (_allowBacktrack || _state >= tutorialState)
        {
            tutorialState = _state;
            SetMessage((int)tutorialState);
        }
    }

    public void GoToNext()
    {
        if ((int)tutorialState < tutorialLength - 1)
        {
            tutorialState++;
            SetMessage((int)tutorialState);
        }
    }

    public void GoToPrevious()
    {
        if (tutorialState > 0)
        {
            tutorialState--;
            SetMessage((int)tutorialState);
        }
    }

    public void StartTutorial()
    {
        tutorialState = TutorialState.Start;
        SetMessage((int)tutorialState);
    }

    public void EndTutorial()
    {
        tutorialState = TutorialState.End;
        SetMessage((int)tutorialState);
    }

    private void SetMessage(int _message)
    {
        if (_message < tutorialMessages.Count)
        {
            nextButtonText.text = "Next";
            messageHeading.text = tutorialMessages[_message].heading;
            messageDescription.text = tutorialMessages[_message].description;

            messagePanel.SetVisibility(!(tutorialMessages[_message].heading == "" && tutorialMessages[_message].description == ""));
            messagePanel.Pulse();

            updateLayout = true;
        }
        else
        {
            messagePanel.SetVisibility(false);
        }

        if (tutorialState == TutorialState.End - 1)
        {
            nextButtonText.text = "End";
        }

        if (tutorialState == TutorialState.End)
        {
            messagePanel.SetVisibility(false);
            SuperManager.GetInstance().SetShowTutorial(false);
        }

        float fill = (_message + 1) / (float)tutorialMessages.Count;
        progressBar.DOFillAmount(fill, 0.3f);
    }
}