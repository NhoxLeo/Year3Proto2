using System;
using UnityEngine.UI;
using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.d
//
// File Name    : OptionCheckBox.cs
// Description  : The main class to handle checkbox data.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

[Serializable]
public class OptionToggleData : OptionData
{
    public bool defaultValue;
    public bool value;

    public OptionToggleData(bool _defaultValue, bool _value)
    {
        value = _value;
        defaultValue = _defaultValue;
    }
}


public class OptionToggle : OptionObject, OptionDataBase
{
    [SerializeField] private OptionToggleData data;
    [SerializeField] private Toggle toggle;
    /**************************************
    * Name of the Function: Toggle
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Toggle()
    {
        data.value = !data.value;
        data.GetCallback().Invoke();
    }

    public override void Deserialize()
    {
        data.value = PlayerPrefs.GetInt(key, data.defaultValue ? 1 : 0) != 0;
        toggle.isOn = data.value;
        data.GetCallback().Invoke();
    }

    public override void Serialize()
    {
        PlayerPrefs.SetInt(key, data.value ? 1 : 0);
    }

    public override OptionData GetData()
    {
        return data;
    }

    public override void SetData(OptionData _data)
    {
        data = (OptionToggleData) _data;
    }
}
