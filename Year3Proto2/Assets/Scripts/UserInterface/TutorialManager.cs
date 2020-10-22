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
        PlaceLumberMill,
        PlaceMine,
        Storages,
        VillagerAllocation,
        TrainVillager,
        VillagerPriority,
        SelectDefence,
        AllocateDefence,
        FinalMessage,
        End
    }

    public TutorialState State { get; private set; }
    private int tutorialLength;

    [System.Serializable]
    public struct TutorialMessage
    {
        public string heading;
        public string description;
    }

    [SerializeField] private List<TutorialMessage> tutorialMessages = null;

    [Header("Main")]
    [SerializeField] private RectTransform focus = null;
    [SerializeField] private RectTransform focusTransform = null;
    [SerializeField] private UIAnimator messagePanel = null;
    [SerializeField] private RectTransform messageTransform = null;
    [SerializeField] private TMP_Text messageHeading = null;
    [SerializeField] private TMP_Text messageDescription = null;
    [SerializeField] private TMP_Text nextButtonText = null;
    [SerializeField] private Image progressBar = null;
    private bool updateLayout = false;
    private float focusPulseTime = 0f;
    private bool doBeginPulse = true;
    private float beginPulseTime = 0f;

    [Header("Transforms")]
    [SerializeField] private Transform beginButton = null;
    [SerializeField] private RectTransform farmButton = null;
    [SerializeField] private RectTransform woodButton = null;
    [SerializeField] private RectTransform metalButton = null;
    [SerializeField] private RectTransform storageButton = null;
    [SerializeField] private RectTransform defenceButton = null;
    [SerializeField] private RectTransform productionTab = null;
    [SerializeField] private UIAnimator productionTabPanel = null;
    [SerializeField] private RectTransform defenceTab = null;
    [SerializeField] private UIAnimator defenceTabPanel = null;
    [SerializeField] private RectTransform trainVillagerButton = null;
    [SerializeField] private RectTransform villagerPriorityPanel = null;
    [SerializeField] private RectTransform objectivePanel = null;

    private float pulseScaleMagnitude = 0.05f;

    private void Awake()
    {
        instance = this;

        //tutorialMessages = new List<TutorialMessage>();

        tutorialLength = Enum.GetNames(typeof(TutorialState)).Length;

        bool showTutorial = SuperManager.GetInstance().GetShowTutorial();
        doBeginPulse = showTutorial;
        AdvanceTutorialTo(showTutorial ? TutorialState.Start : TutorialState.End, true);
    }

    private void Start()
    {
        SetMessage((int)State);

    }

    public static TutorialManager GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        /*
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
        */

        focusPulseTime += Time.smoothDeltaTime;

        focus.transform.localScale = Vector3.one * (1.0f + pulseScaleMagnitude + Mathf.Sin(focusPulseTime * 6.0f) * pulseScaleMagnitude);
        if (focusTransform != null)
        {
            focus.position = focusTransform.position;
        }

        beginPulseTime += Time.deltaTime;
        if (beginPulseTime >= 1.0f && doBeginPulse)
        {
            beginButton.transform.DOKill(true);
            beginButton.DOPunchPosition(Vector3.right * 4.0f, 0.3f, 1, 0.5f);
            beginPulseTime = 0.0f;
        }

        switch (State)
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

            case TutorialState.PlaceLumberMill:
                if (productionTabPanel.showElement && focusTransform != woodButton)
                {
                    FocusOn(woodButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
                {
                    FocusOn(productionTab);
                }
                break;

            case TutorialState.PlaceMine:
                if (productionTabPanel.showElement && focusTransform != metalButton)
                {
                    FocusOn(metalButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
                {
                    FocusOn(productionTab);
                }
                break;
            case TutorialState.Storages:
                if (productionTabPanel.showElement && focusTransform != storageButton)
                {
                    FocusOn(storageButton);
                }
                if (!productionTabPanel.showElement && focusTransform != productionTab)
                {
                    FocusOn(productionTab);
                }
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
                if (defenceTabPanel.showElement && focusTransform != defenceButton)
                {
                    FocusOn(defenceButton);
                }
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
            focusPulseTime = 0.33f;
        }

        focusTransform = _rTrans;
    }

    public void AdvanceTutorialTo(TutorialState _state, bool _allowBacktrack)
    {
        if (_allowBacktrack || _state >= State)
        {
            State = _state;
            SetMessage((int)State);
        }
    }

    public void GoToNext()
    {
        if ((int)State < tutorialLength - 1)
        {
            State++;
            SetMessage((int)State);
        }

        doBeginPulse = false;
    }

    public void GoToPrevious()
    {
        if (State > 0)
        {
            State--;
            SetMessage((int)State);
        }
    }

    public void StartTutorial()
    {
        State = TutorialState.Start;
        SetMessage((int)State);
    }

    public void EndTutorial()
    {
        State = TutorialState.End;
        SetMessage((int)State);
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

        if (State == TutorialState.Start)
        {
            nextButtonText.text = "Begin";
        }

        if (State == TutorialState.End - 1)
        {
            nextButtonText.text = "End";
        }

        if (State == TutorialState.End)
        {
            messagePanel.SetVisibility(false);
            SuperManager.GetInstance().SetShowTutorial(false);
        }

        float fill = (_message + 1) / (float)tutorialMessages.Count;
        progressBar.DOFillAmount(fill, 0.3f);
    }
}