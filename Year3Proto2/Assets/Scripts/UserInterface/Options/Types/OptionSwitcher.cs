using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using System.Runtime.InteropServices;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.d
//
// File Name    : OptionSwitcher.cs
// Description  : The main class for handling switcher data
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class OptionSwitcherData : OptionData
{
    public int defaultValue;
    public int value;
    public string[] values;

    public OptionSwitcherData(int _defaultValue, int _value, string[] _values)
    {
        value = _value;
        values = _values;
        defaultValue = _defaultValue;
    }
}

public class OptionSwitcher : OptionObject, OptionDataBase
{
    [SerializeField] private OptionSwitcherData data;
    [SerializeField] private TMP_Text value;

    /**************************************
    * Name of the Function: Next
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Next()
    {
        if (data != null && value)
        {
            if (data.value >= (data.values.Length - 1))
            {
                data.value = 0;
            }
            else
            {
                data.value += 1;
            }
            value.text = data.values[data.value].ToString();

            OptionCallback optionCallback = data.GetCallback();
            if (optionCallback != null)
            {
                optionCallback.Invoke();
            }
        }
    }

    /**************************************
    * Name of the Function: Previous
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Previous()
    {
        if (data != null && value)
        {
            if (data.value <= 0)
            {
                data.value = data.values.Length - 1;
            }
            else
            {
                data.value -= 1;
            }
            value.text = data.values[data.value].ToString();

            OptionCallback optionCallback = data.GetCallback();
            if (optionCallback != null)
            {
                optionCallback.Invoke();
            }
        }
    }

    public override void Deserialize()
    {
        if (data != null && value)
        {
            data.value = PlayerPrefs.GetInt(key, data.defaultValue);
            value.text = data.values[data.value].ToString();

            OptionCallback optionCallback = data.GetCallback();
            if (optionCallback != null)
            {
                optionCallback.Invoke();
            }
        }
    }

    public override void Serialize()
    {
        PlayerPrefs.SetInt(key, data.value);
    }

    public override OptionData GetData()
    {
        return data;
    }

    public override void SetData(OptionData _data)
    {
        data = (OptionSwitcherData) _data;
    }
}
