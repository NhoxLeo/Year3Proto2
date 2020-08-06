using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Healthbar : MonoBehaviour
{
    public GameObject target;
    public float fillAmount = 1.0f;
    private float fillBefore;

    private Image bar;

    void Start()
    {
        bar = transform.Find("Bar").GetComponent<Image>();
    }

    private void Update()
    {
        if (fillAmount != fillBefore)
        {
            PulseTip();
            fillBefore = fillAmount;
        }

        bar.fillAmount = fillAmount;
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
        pos.y += 50.0f;
        transform.position = pos;
    }

    private void PulseTip()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.0f), 0.15f, 1, 0.5f);
    }
}
