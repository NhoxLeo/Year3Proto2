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
    DayClear = 0,
    DayFoggy = 1,
    DayRainy = 2,
    DayStormy = 3,
    Night = 4
}

public class LightingManager : MonoBehaviour
{
    [SerializeField] private Volume[] sfVolumes;

    [SerializeField] private Color daylightColor;
    [SerializeField] private Color nightColor;

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
            int volIndexTar = (int)weatherTarget;
            DOTween.To(() => sfVolumes[volIndexTar].weight, y => sfVolumes[volIndexTar].weight = y, 1.0f, 5.0f);

            int volIndexCur = (int)weatherCurrent;
            DOTween.To(() => sfVolumes[volIndexCur].weight, x => sfVolumes[volIndexCur].weight = x, 0.0f, 5.0f).SetEase(Ease.InQuad);
        }

        weatherCurrent = weatherTarget;
    }

    public static LightingManager Instance()
    {
        return instance;
    }
}
