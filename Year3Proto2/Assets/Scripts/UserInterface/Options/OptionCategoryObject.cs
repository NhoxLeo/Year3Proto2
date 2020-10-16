using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.d
//
// File Name    : OptionCategoryObject.cs
// Description  : Categories handle specific childs for when they need to be enabled/disabled.
// Author       : Tjeu Vreeburg, David
// Mail         : tjeu.vreeburg@gmail.com

public class OptionCategoryObject : MonoBehaviour
{
    [SerializeField] private Transform panel;
    [SerializeField] private Transform tabIndicator;

    /**************************************
    * Name of the Function: Next
    * @Author: Tjeu Vreeburg (General Code), David (Animation, Tweening)
    * @Parameter: n/a
    * @Return: void
    ***************************************/

    private void Start()
    {
        Transform child = transform.parent.GetChild(0);
        if (child)
        {
            OptionCategoryObject categoryObject = child.GetComponent<OptionCategoryObject>();
            categoryObject.GetPanel().GetComponent<UIAnimator>().showElement = true;
        }
    }

    public void OnClick()
    {
        SwitchTo();
    }

    /**************************************
    * Name of the Function: GetPanel
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: Transform
    ***************************************/
    public Transform GetPanel()
    {
        return panel;
    }

    public void SwitchTo()
    {
        Transform parent = transform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform transform = parent.GetChild(i);
            if (transform)
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
}
