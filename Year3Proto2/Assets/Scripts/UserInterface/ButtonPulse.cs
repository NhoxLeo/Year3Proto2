using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ButtonPulse : MonoBehaviour
{
    public void ButtonClick()
    {
        transform.DOKill(true);
        transform.DOPunchScale(new Vector3(-0.1f, -0.1f, 0.0f), 0.2f, 1, 0.0f);
    }
}
