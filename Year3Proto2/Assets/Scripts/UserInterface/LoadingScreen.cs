using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LoadingScreen : MonoBehaviour
{
    private GameObject loadingIcon;

    void Start()
    {
        loadingIcon = transform.Find("LoadingIcon").gameObject;
        loadingIcon.SetActive(GlobalData.isLoadingIn);
    }

    void Update()
    {
        loadingIcon.transform.Rotate(0.0f, 0.0f, -270.0f * Time.fixedDeltaTime, Space.Self);
    }
}
