using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonPulse : MonoBehaviour, IPointerEnterHandler
{
    private Button button;

    public void ButtonClick()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0.0f), 0.2f, 1, 0.0f);
        if (Time.timeSinceLevelLoad > 1f)
        {
            SuperManager.UIClickSound();
        }
    }

    public void OnPointerEnter(PointerEventData _eventData)
    {
        /*
        if (!button)
        {
            button = GetComponent<Button>();
            Debug.Log("Getting Button");
        }
        
        if (button)
        {
            if (button.interactable)
            {
                SuperManager.UITapSound();
            }
        }
        */
    }
}
