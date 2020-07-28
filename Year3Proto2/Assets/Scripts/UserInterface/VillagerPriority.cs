using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VillagerPriority : MonoBehaviour
{
    public bool showPanel = false;
    private bool panelShown = false;

    private GameObject check;
    private Transform panel;
    private float panelXinitial;
    private Transform toggleButton;
    private float toggleButtonXinitial;

    [SerializeField] private Sprite minimizeSprite;
    [SerializeField] private Sprite expandSprite;

    void Start()
    {
        check = transform.Find("VillagerPriorityPanel/Check").gameObject;
        check.GetComponent<CanvasGroup>().alpha = 0.0f;

        panel = transform.Find("VillagerPriorityPanel");
        panelXinitial = panel.localPosition.x;
        toggleButton = transform.Find("TogglePanelButton");
        toggleButtonXinitial = toggleButton.localPosition.x;
    }


    void Update()
    {
        if (showPanel && !panelShown)
        {
            ShowPanel();
            panelShown = true;
        }

        if (!showPanel && panelShown)
        {
            HidePanel();
            panelShown = false;
        }
    }

    public void TogglePanel()
    {
        showPanel = !showPanel;
    }

    private void ShowPanel()
    {
        panel.DOLocalMoveX(panelXinitial + 231.0f, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.DOLocalMoveX(toggleButtonXinitial + 231.0f - 66.0f, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.transform.Rotate(new Vector3(0, 0, -180));
        toggleButton.GetComponent<Image>().sprite = minimizeSprite;
    }

    private void HidePanel()
    {
        panel.DOLocalMoveX(panelXinitial, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.DOLocalMoveX(toggleButtonXinitial, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.transform.Rotate(new Vector3(0, 0, 180));
        toggleButton.GetComponent<Image>().sprite = expandSprite;
    }

    public void Prioritize(string _type)
    {
        CanvasGroup checkCanvas = check.GetComponent<CanvasGroup>();
        checkCanvas.DOKill();
        checkCanvas.alpha = 1.0f;
        switch (_type)
        {
            case "Food":
                check.transform.DOLocalMoveY(transform.Find("VillagerPriorityPanel/Content").GetChild(0).transform.localPosition.y - 18.0f, 0.0f);

                break;

            case "Wood":
                check.transform.DOLocalMoveY(transform.Find("VillagerPriorityPanel/Content").GetChild(1).transform.localPosition.y - 18.0f, 0.0f);

                break;

            case "Metal":
                check.transform.DOLocalMoveY(transform.Find("VillagerPriorityPanel/Content").GetChild(2).transform.localPosition.y - 18.0f, 0.0f);

                break;

            case "Defence":
                check.transform.DOLocalMoveY(transform.Find("VillagerPriorityPanel/Content").GetChild(3).transform.localPosition.y - 18.0f, 0.0f);

                break;

            default:
                break;
        }

        check.GetComponent<CanvasGroup>().DOFade(0.0f, 2.0f).SetEase(Ease.InQuint);
    }

    public void HideCheck()
    {
        check.GetComponent<CanvasGroup>().alpha = 0.0f;
    }
}
