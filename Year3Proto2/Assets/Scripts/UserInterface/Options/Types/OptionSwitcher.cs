using UnityEngine;
using TMPro;

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

public class OptionSwitcher : OptionObject, OptionData<int>
{
    [SerializeField] private TMP_Text valueName;
    [SerializeField] private int data;
    [SerializeField] private string[] values;

    /**************************************
    * Name of the Function: Next
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Next()
    {
        if (data >= (values.Length - 1))
        {
            data = 0;
        }
        else
        {
            data += 1;
        }
        valueName.text = values[data].ToString();
    }

    /**************************************
    * Name of the Function: Previous
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Previous()
    {
        if (data <= 0)
        {
            data = values.Length - 1;
        }
        else
        {
            data -= 1;
        }
        valueName.text = values[data].ToString();
    }

    /**************************************
    * Name of the Function: GetData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: Integer
    ***************************************/
    public int GetData()
    {
        return data;
    }

    /**************************************
    * Name of the Function: SetData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SetData(int _data)
    {
        PlayerPrefs.SetString(key, data.ToString());
    }

    /**************************************
    * Name of the Function: Deserialise
    * @Author: Tjeu Vreeburg
    * @Parameter: Integer
    * @Return: void
    ***************************************/
    public void Deserialise(int _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : _data;
        valueName.text = values[data].ToString();
    }

    /**************************************
    * Name of the Function: Serialise
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Serialise()
    {
         PlayerPrefs.SetInt(key, data);
    }

    /**************************************
    * Name of the Function: SetValues
    * @Author: Tjeu Vreeburg
    * @Parameter: String Array
    * @Return: void
    ***************************************/
    public void SetValues(string[] _values)
    {
        values = _values;
    }
}
