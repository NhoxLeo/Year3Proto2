using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    [SerializeField] private GameObject autoIndicator;
    [SerializeField] private GameObject manualIndicator;
    [SerializeField] private Transform allocationButtons;

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

    public void AllocateVillager(int amount)
    {
        if (target == null)
            return;

        if (amount > 0) { target.AllocateVillager(); }
        else { target.DeallocateVillager(); }

        FindObjectOfType<HUDManager>().RefreshResources();
        FindObjectOfType<VillagerPriority>().HideCheck();
        FindObjectOfType<BuildingInfo>().SetInfo();

    }

    public void SetAllocation(int _value)
    {
        Debug.Log("Allocation for " + target.ToString() + " is being set to " + _value);

        // Allocate Villager Function here
    }

    public void SetAutoIndicator(int _value)
    {
        if (_value >= 0)
        {
            autoIndicator.SetActive(true);
            Vector3 pos = autoIndicator.transform.position;
            pos.x = allocationButtons.GetChild(_value).position.x;
            autoIndicator.transform.position = pos;
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
            Vector3 pos = manualIndicator.transform.position;
            pos.x = allocationButtons.GetChild(_value).position.x;
            manualIndicator.transform.position = pos;
        }
        else
        {
            manualIndicator.SetActive(false);
        }
    }

}
