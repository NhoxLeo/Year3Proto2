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

public class OptionSlider : OptionObject, OptionData<float>
{
    private float data;

    /**************************************
    * Name of the Function: Deserialise
    * @Author: Tjeu Vreeburg
    * @Parameter: Float
    * @Return: void
    ***************************************/
    public void Deserialise(float _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : _data;
    }

    /**************************************
    * Name of the Function: UpdateValue
    * @Author: Tjeu Vreeburg
    * @Parameter: Slider
    * @Return: void
    ***************************************/
    public void UpdateValue(Slider slider)
    {
        data = slider.value;
    }

    /**************************************
    * Name of the Function: GetData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: float
    ***************************************/
    public float GetData()
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
        PlayerPrefs.SetFloat(key, data);
    }

    /**************************************
    * Name of the Function: SetData
    * @Author: Tjeu Vreeburg
    * @Parameter: float
    * @Return: void
    ***************************************/
    public void SetData(float _data)
    {
        data = _data;
    }
}
