using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class VillagerAllocation : MonoBehaviour
{
    public Structure target;
    private TMP_Text villagerText;

    void Awake()
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
        pos.y -= 64.0f;
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

        SetInfo();
        FindObjectOfType<HUDManager>().RefreshResources();
        FindObjectOfType<VillagerPriority>().HideCheck();
    }

    public void SetInfo()
    {
        if (target == null)
            return;

        villagerText.text = target.GetAllocated().ToString("0") + "/" + target.GetVillagerCapacity().ToString("0");
    }

    private void OnEnable()
    {
        SetInfo();
    }

}
