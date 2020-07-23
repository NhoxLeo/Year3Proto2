﻿using UnityEngine;
using UnityEngine.UI;

public class OptionSlider : OptionObject
{
    [SerializeField] private Slider slider;
    private float value = 0.0f;

    private void Start()
    {
        slider.onValueChanged.AddListener(value=> this.value = value); 
    }

    public float GetValue()
    {
        return value;
    }
}