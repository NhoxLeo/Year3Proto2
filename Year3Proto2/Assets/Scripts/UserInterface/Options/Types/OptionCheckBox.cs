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

public class OptionCheckBox : OptionObject, OptionData<bool>
{
    [SerializeField] private bool data;

    /**************************************
    * Name of the Function: Toggle
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Toggle()
    {
        data = data ? false : true;
    }

    /**************************************
    * Name of the Function: Deserialise
    * @Author: Tjeu Vreeburg
    * @Parameter: Boolean
    * @Return: void
    ***************************************/
    public void Deserialise(bool _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) != 0 : _data;
    }

    /**************************************
    * Name of the Function: GetData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: Boolean
    ***************************************/
    public bool GetData()
    {
        return data;
    }

    /**************************************
    * Name of the Function: Serialise
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void Serialise()
    {
        PlayerPrefs.SetInt(key, data ? 1 : 0);
    }

    /**************************************
    * Name of the Function: SetData
    * @Author: Tjeu Vreeburg
    * @Parameter: Boolean
    * @Return: void
    ***************************************/
    public void SetData(bool _data)
    {
        data = _data;
    }
}
