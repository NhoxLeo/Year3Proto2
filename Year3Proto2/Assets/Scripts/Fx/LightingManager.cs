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

public class LightingManager : MonoBehaviour
{
    [SerializeField] private Volume[] sfVolumes;
    private Volume currentVolume;

    [SerializeField] private Color daylightColor;
    [SerializeField] private Color nightColor;

    public enum Weather
    {
        DayClear = 0,
        DayFoggy = 1,
        DayRainy = 2,
        DayStormy = 3,
        Night = 4
    }
    [SerializeField] private Weather weatherCurrent;
    [SerializeField] private Weather weatherTarget;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeather(Weather.DayClear);
            //DOTween.To(() => sfVolumes[1].weight, x => sfVolumes[1].weight = x, 0.0f, 0.2f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeather(Weather.DayFoggy);
            //DOTween.To(() => sfVolumes[1].weight, x => sfVolumes[1].weight = x, 1.0f, 0.2f);
        }
    }

    public void SetWeather(Weather _weatherType)
    {
        weatherTarget = _weatherType;

        if (weatherCurrent != weatherTarget)
        {
            // Fade in target weather type
            int volIndexTar = (int)weatherTarget;
            DOTween.To(() => sfVolumes[volIndexTar].weight, y => sfVolumes[volIndexTar].weight = y, 1.0f, 5.0f);
            //Debug.Log((int)weatherTarget);

            // Fade out current weather type
            int volIndexCur = (int)weatherCurrent;
            DOTween.To(() => sfVolumes[volIndexCur].weight, x => sfVolumes[volIndexCur].weight = x, 0.0f, 5.0f).SetEase(Ease.InQuad);
            Debug.Log((int)weatherCurrent);
        }
        weatherCurrent = weatherTarget;
    }
}
