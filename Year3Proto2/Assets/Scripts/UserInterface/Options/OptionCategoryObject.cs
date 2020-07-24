using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OptionCategoryObject : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private Transform tabIndicator;

    public void OnClick()
    {
        Transform parent = transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform transform = parent.GetChild(i);
            if(transform)
            {
                OptionCategoryObject categoryObject = transform.GetComponent<OptionCategoryObject>();
                categoryObject.GetPanel().GetComponent<UIAnimator>().showElement = false;
            }
        }

        // Animation do tween.
        // 
        panel.GetComponent<UIAnimator>().showElement = true;
        tabIndicator.DOLocalMoveX(transform.localPosition.x, 0.2f).SetEase(Ease.OutBack);
    }

    public Transform GetPanel()
    {
        return panel;
    }
}
