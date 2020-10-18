using UnityEngine;
using UnityEngine.UI;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : OptionSlider.cs
// Description  : Slider element for options.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class OptionSliderData : OptionData
{
    public float defaultValue;
    public float value;

    public Vector2 range;

    public OptionSliderData(Vector2 _range, float _defaultValue)
    {
        range = _range;
        defaultValue = _defaultValue;
    }
}

public class OptionSlider : OptionObject, OptionDataBase
{
    [SerializeField] private OptionSliderData data;
    [SerializeField] private Slider slider;

    public override void Deserialize()
    {
        data.value = PlayerPrefs.GetFloat(key, data.defaultValue);

        slider.minValue = data.range.x;
        slider.maxValue = data.range.y;
        slider.value = data.value;

        data.GetCallback().Invoke();

        deserialised = true;
    }

    public override OptionData GetData()
    {
        return data;
    }

    public override void Serialize()
    {
        PlayerPrefs.SetFloat(key, data.value);
    }

    public override void SetData(OptionData _data)
    {
        data = (OptionSliderData) _data;
    }

    public void Refresh()
    {
        if (deserialised)
        {
            data.value = slider.value;
            data.GetCallback().Invoke();
        }
    }
}
