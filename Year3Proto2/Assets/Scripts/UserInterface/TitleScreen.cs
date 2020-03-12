using UnityEngine;
using DG.Tweening;

public class TitleScreen : MonoBehaviour
{
    private GameObject gameLogo;
    private GameObject buttonStart;
    private GameObject buttonTut;
    private GameObject buttonExit;
    private GameObject divider;
    private GameObject decorLeft;
    private GameObject decorRight;

    void Start()
    {
        GetComponent<Tooltip>().showTooltip = true;

        gameLogo = transform.Find("GameLogo").gameObject;
        float gameLogoY = gameLogo.transform.localPosition.y;
        gameLogo.transform.DOLocalMoveY(-16.0f, 0.0f);
        gameLogo.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        buttonStart = transform.Find("ButtonStart").gameObject;
        buttonStart.transform.localScale = new Vector3(0, 0, 0);

        buttonTut = transform.Find("ButtonCredits").gameObject;
        buttonTut.transform.localScale = new Vector3(0, 0, 0);

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

        Sequence titleSequence = DOTween.Sequence();
        titleSequence.Insert(1.5f, gameLogo.transform.DOLocalMoveY(gameLogoY, 1.0f).SetEase(Ease.InOutBack));
        titleSequence.Insert(1.5f, gameLogo.transform.DOScale(1.0f, 1.0f).SetEase(Ease.InOutBack));

        titleSequence.Insert(2.3f, buttonStart.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
        titleSequence.Insert(2.5f, buttonTut.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
        titleSequence.Insert(2.7f, buttonExit.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));

        titleSequence.Insert(2.8f, decorLeft.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f).SetEase(Ease.OutSine));
        titleSequence.Insert(2.8f, decorRight.GetComponent<CanvasGroup>().DOFade(1.0f, 1.0f).SetEase(Ease.OutSine));
        titleSequence.Insert(2.8f, decorLeft.transform.DOLocalMoveX(decorLeftX, 1.0f).SetEase(Ease.OutQuint));
        titleSequence.Insert(2.8f, decorRight.transform.DOLocalMoveX(decorRightX, 1.0f).SetEase(Ease.OutQuint));

        titleSequence.Insert(2.95f, divider.transform.DOScaleX(1.0f, 1.0f).SetEase(Ease.OutQuint));
    }
}
