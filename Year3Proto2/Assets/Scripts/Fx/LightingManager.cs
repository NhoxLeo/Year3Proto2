//
// Bachelor of Creative Technologies
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name        : LightingManager.cs
// Description      : Tweens between different lighting setups
// Author           : David Morris
// Mail             : David.Mor7851@mediadesign.school.nz
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;

public enum Weather
{
    Clear = 0,
    Rainy = 1,
    Snowy = 2,
}

public class LightingManager : MonoBehaviour
{
    [SerializeField] private Volume[] sfVolumes;

    [SerializeField] private Color daylightColor;

    [SerializeField] private Weather weatherCurrent;
    [SerializeField] private Weather weatherTarget;

    private static LightingManager instance = null;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = GetComponent<LightingManager>();
    }

    public void SetWeather(Weather _weatherType)
    {
        weatherTarget = _weatherType;

        if (weatherCurrent != weatherTarget)
        {
            int targetIndex = (int) weatherTarget;
            DOTween.To(() => sfVolumes[targetIndex].weight, y => sfVolumes[targetIndex].weight = y, 1.0f, 5.0f);

            int currentIndex = (int) weatherCurrent;
            DOTween.To(() => sfVolumes[currentIndex].weight, x => sfVolumes[currentIndex].weight = x, 0.0f, 5.0f).SetEase(Ease.InQuad);
        }

        weatherCurrent = weatherTarget;
    }

    public static LightingManager Instance()
    {
        return instance;
    }
}
