using System.Collections;
using System.Collections.Generic;
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
        gameLogo = transform.Find("GameLogo").gameObject;
        float gameLogoY = gameLogo.transform.localPosition.y;
        gameLogo.transform.DOLocalMoveY(-16.0f, 0.0f);

        buttonStart = transform.Find("ButtonStart").gameObject;
        buttonStart.transform.localScale = new Vector3(0, 0, 0);

        buttonTut = transform.Find("ButtonTut").gameObject;
        buttonTut.transform.localScale = new Vector3(0, 0, 0);

        buttonExit = transform.Find("ButtonExit").gameObject;
        buttonExit.transform.localScale = new Vector3(0, 0, 0);

        divider = transform.Find("Divider").gameObject;
        divider.transform.localScale = new Vector3(0, 1, 1);

        decorLeft = transform.Find("Background/DecorLeft").gameObject;
        decorLeft.transform.DOLocalMoveX(-100f, 0.0f);
        decorLeft.GetComponent<CanvasGroup>().DOFade(0.0f, 0.0f);

        decorRight = transform.Find("Background/DecorRight").gameObject;
        decorRight.transform.DOLocalMoveX(100f, 0.0f);
        decorRight.GetComponent<CanvasGroup>().DOFade(0.0f, 0.0f);

        Sequence titleSequence = DOTween.Sequence();
        titleSequence.Insert(1.5f, gameLogo.transform.DOLocalMoveY(gameLogoY, 1.0f).SetEase(Ease.InOutBack));

        titleSequence.Insert(2.3f, buttonStart.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
        titleSequence.Insert(2.5f, buttonTut.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));
        titleSequence.Insert(2.7f, buttonExit.transform.DOScale(new Vector3(1, 1, 1), 0.4f).SetEase(Ease.OutBack));

        //titleSequence.Insert
        //gameLogo.transform.DOLocalMoveY(gameLogoY, 1.0f);
    }


    void Update()
    {
        
    }
}
