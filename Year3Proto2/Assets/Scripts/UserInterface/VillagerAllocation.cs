using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    private StructureManager structMan;
    [SerializeField] private GameObject autoIndicator;
    [SerializeField] private GameObject manualIndicator;
    [SerializeField] private Transform allocationButtons;
    private UIAnimator uiAnimator;

    private void Awake()
    {
        uiAnimator = GetComponent<UIAnimator>();
    }

    private void LateUpdate()
    {
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
        //Debug.Log("Allocation for " + target.ToString() + " is being set to " + _value);

        // Allocate Villager Function here

        target.HandleAllocation(_value);

        
    }

    public void SetAutoIndicator(int _value)
    {
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
}
