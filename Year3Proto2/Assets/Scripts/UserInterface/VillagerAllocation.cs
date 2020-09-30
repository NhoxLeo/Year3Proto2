using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    private StructureManager structMan;
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

        if (_value >= 0) { autoIndicator.transform.localPosition = allocationButtons.GetChild(_value).localPosition; }
    }

    public void SetManualIndicator(int _value)
    {
        UpdateButtonActive();

        if (_value >= 0) { manualIndicator.transform.localPosition = allocationButtons.GetChild(_value).localPosition; }
    }

    public void SetVisibility(bool _visible)
    {
        // if the caller is trying to turn off the widget...
        if (!_visible)
        {
            // and ShowVillagerWidgets is true (aka showing all widgets currently)
            if (SuperManager.GetInstance().GetShowWidgets())
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
        manualIndicator.SetActive(target.GetManualAllocation());
        autoIndicator.SetActive(!target.GetManualAllocation());
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