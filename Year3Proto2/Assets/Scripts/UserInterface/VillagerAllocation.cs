﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    private StructureManager structMan;
    private int allocationLevel;
    private bool hovering;
    private bool collapseWidget;
    private bool widgetCollapsed;
    public List<Vector3> buttonPos;

    [SerializeField] private GameObject autoIndicator;
    [SerializeField] private GameObject manualIndicator;
    [SerializeField] private Transform allocationButtons;
    private UIAnimator uiAnimator;

    private void Awake()
    {
        structMan = StructureManager.GetInstance();
        uiAnimator = GetComponent<UIAnimator>();
        for (int i = 0; i < 4; i++)
        {
            buttonPos.Add(allocationButtons.GetChild(i).localPosition);
        }
    }

    private void LateUpdate()
    {
        collapseWidget = !structMan.StructureIsSelected(target) && !hovering;

        if (collapseWidget && !widgetCollapsed)
        {
            Collapse();
            widgetCollapsed = true;
        }

        if (!collapseWidget && widgetCollapsed)
        {
            Expand();
            widgetCollapsed = false;
        }

        SetPosition();
        if (!target)
        {
            //gets rid of phantom widgets
            Destroy(gameObject);
        }
    }

    private void SetPosition()
    {
        if (target == null)
            return;

        // Position info panel near target building
        Vector3 pos = Camera.main.WorldToScreenPoint(target.transform.position);
        transform.position = pos;
    }

    public void SetTarget(Structure _target)
    {
        target = _target;
    }

    public void SetAllocation(int _value)
    {
        target.HandleAllocation(_value);
    }

    public void SetAutoIndicator(int _value)
    {
        UpdateButtonActive();

        if (_value >= 0)
        {
            autoIndicator.SetActive(true);
            autoIndicator.transform.localPosition = allocationButtons.GetChild(_value).localPosition;
        }
        else
        {
            autoIndicator.SetActive(false);
        }
    }

    public void SetManualIndicator(int _value)
    {
        UpdateButtonActive();

        if (_value >= 0)
        {
            manualIndicator.SetActive(true);
            manualIndicator.transform.localPosition = allocationButtons.GetChild(_value).localPosition;
        }
        else
        {
            manualIndicator.SetActive(false);
        }
    }

    public void SetVisibility(bool _visible)
    {
        // if the caller is trying to turn off the widget...
        if (!_visible)
        {
            // and the HUDMan is not in build mode (aka showing all widgets currently)
            if (!HUDManager.GetInstance().buildMode)
            {
                // don't set the visibility.
                return;
            }
        }
        // if we got this far, set it.
        uiAnimator.SetVisibility(_visible);
    }

    private void UpdateButtonActive()
    {
        for (int i = 0; i < 4; i++)
        {
            allocationButtons.GetChild(i).gameObject.SetActive((target.GetAllocated() == i) || !collapseWidget);
        }
    }

    public void SetHovering(bool _hovering)
    {
        hovering = _hovering;
    }

    private void Collapse()
    {
        UpdateButtonActive();

        for (int i = 0; i < 4; i++)
        {
            allocationButtons.GetChild(i).DOLocalMoveX(0.0f, 0.2f);
            manualIndicator.transform.DOLocalMoveX(0.0f, 0.2f);
            autoIndicator.transform.DOLocalMoveX(0.0f, 0.2f);
        }
    }

    private void Expand()
    {
        for (int i = 0; i < 4; i++)
        {
            allocationButtons.GetChild(i).gameObject.SetActive(true);
            allocationButtons.GetChild(i).DOLocalMoveX(buttonPos[i].x, 0.2f);
            manualIndicator.transform.DOLocalMoveX(buttonPos[target.GetAllocated()].x, 0.2f);
            autoIndicator.transform.DOLocalMoveX(buttonPos[target.GetAllocated()].x, 0.2f);
        }
    }
}
