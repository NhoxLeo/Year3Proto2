using UnityEngine;
using DG.Tweening;
using TMPro;

public class TitleScreen : MonoBehaviour
{
    private GameObject gameLogo;
    private GameObject buttonStart;
    private GameObject buttonMap;
    private GameObject buttonRes;
    private GameObject buttonCred;
    private GameObject buttonExit;
    private GameObject divider;
    private GameObject decorLeft;
    private GameObject decorRight;

    private Sequence titleSequence;

    [SerializeField] TMP_Text version;
    [SerializeField] UIAnimator gameEndscreen = null;

    private int loadingFrameCounter = 0;

    void Start()
    {
        Time.timeScale = 1.0f;

        if (GlobalData.gameEnd)
        {
            gameEndscreen.SetVisibility(true);
            GlobalData.gameEnd = false;
        }
        else
        {
            GetComponent<UIAnimator>().SetVisibility(true);
        }

        if (!SuperManager.TitleScreenAnimPlayed)
        {
            gameLogo = transform.Find("GameLogo").gameObject;
            float gameLogoY = gameLogo.transform.localPosition.y;
            gameLogo.transform.DOLocalMoveY(-16.0f, 0.0f);
            gameLogo.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            buttonStart = transform.Find("ButtonStart").gameObject;
            buttonStart.transform.localScale = new Vector3(0, 0, 0);

            buttonMap = transform.Find("ButtonMap").gameObject;
            buttonMap.transform.localScale = new Vector3(0, 0, 0);

            buttonRes = transform.Find("ButtonResearch").gameObject;
            buttonRes.transform.localScale = new Vector3(0, 0, 0);

            buttonCred = transform.Find("ButtonOptions").gameObject;
            buttonCred.transform.localScale = new Vector3(0, 0, 0);

            buttonExit = transform.Find("ButtonExit").gameObject;
            buttonExit.transform.localScale = new Vector3(0, 0, 0);

            divider = transform.Find("Divider").gameObject;
            divider.transform.localScale = new Vector3(0, 1, 1);

            decorLeft = transform.Find("DecorLeft").gameObject;
            float decorLeftX = decorLeft.transform.localPosition.x;
            decorLeft.transform.DOLocalMoveX(-32f, 0.0f);
            decorLeft.GetComponent<CanvasGroup>().alpha = 0.0f;

            decorRight = transform.Find("DecorRight").gameObject;
            float decorRightX = decorRight.transform.localPosition.x;
            decorRight.transform.DOLocalMoveX(32f, 0.0f);
            decorRight.GetComponent<CanvasGroup>().alpha = 0.0f;

            titleSequence = DOTween.Sequence();
            titleSequence.Insert(1.5f, gameLogo.transform.DOLocalMoveY(gameLogoY, 1.0f).SetEase(Ease.InOutBack));
            titleSequence.Insert(1.5f, gameLogo.transform.DOScale(1.0f, 1.0f).SetEase(Ease.InOutBack));

            titleSequence.Insert(2.3f, buttonStart.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
            titleSequence.Insert(2.5f, buttonMap.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
            titleSequence.Insert(2.7f, buttonRes.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
            titleSequence.Insert(2.9f, buttonCred.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
            titleSequence.Insert(3.1f, buttonExit.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));

            titleSequence.Insert(3.1f, decorLeft.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f).SetEase(Ease.OutSine));
            titleSequence.Insert(3.1f, decorRight.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f).SetEase(Ease.OutSine));
            titleSequence.Insert(3.1f, decorLeft.transform.DOLocalMoveX(decorLeftX, 1.0f).SetEase(Ease.OutQuint));
            titleSequence.Insert(3.1f, decorRight.transform.DOLocalMoveX(decorRightX, 1.0f).SetEase(Ease.OutQuint));

            titleSequence.Insert(2.95f, divider.transform.DOScaleX(1.0f, 1.0f).SetEase(Ease.OutQuint));
        }

        SuperManager.TitleScreenAnimPlayed = true;

        TMP_Text startText = transform.Find("ButtonStart/Text").GetComponent<TMP_Text>();
        startText.text = SuperManager.GetInstance().GetSavedMatch().match ? "CONTINUE" : "NEW GAME";
        //Debug.Log(SuperManager.GetInstance().GetCurrentLevel());

        version.text = "v" + Application.version;

        //SceneSwitcher switcher = FindObjectOfType<SceneSwitcher>();
        //if (switcher.GetLoadingScreenIsActive())
        //{
        //    switcher.EndLoad();
        //}
    }

    private void LateUpdate()
    {
        if (loadingFrameCounter < 20)
        {
            loadingFrameCounter++;
            if (loadingFrameCounter == 20)
            {
                SceneSwitcher switcher = FindObjectOfType<SceneSwitcher>();
                if (switcher.GetLoadingScreenIsActive())
                {
                    switcher.EndLoad();
                }
            }
        }
    }

    public void PlayButton()
    {
        SuperManager superMan = SuperManager.GetInstance();
        if (!superMan.GetSavedMatch().match)
        {
            superMan.PlayLevel(0);
        }
        else
        {
            superMan.PlayLevel(superMan.GetSavedMatch().levelID);
        }
    }
}
