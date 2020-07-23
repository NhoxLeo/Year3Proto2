﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    private TMP_Text villagerText;

    void Start()
    {
        villagerText = transform.Find("VillagerBox/VillagerValue").GetComponent<TMP_Text>();
        SetInfo();
    }

    private void Update()
    {
        SetInfo();
    }

    private void LateUpdate()
    {
        SetPosition();
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

        if (amount > 0) { target.IncreaseFoodAllocation(); }
        else { target.DecreaseFoodAllocation(); }

        SetInfo();
    }

    public void SetInfo()
    {
        if (target == null)
            return;

        villagerText.text = target.GetFoodAllocation().ToString("0") + "/" + Structure.foodAllocationMax.ToString("0");
    }

    private void OnEnable()
    {
        SetInfo();
        Debug.Log("dsaads");
    }

}