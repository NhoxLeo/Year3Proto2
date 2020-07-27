using UnityEngine;
using UnityEngine.UI;

public class OptionSlider : OptionObject, OptionData<float>
{
    private float data;

    public void Deserialise(float _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : _data;
    }

    public void UpdateValue(Slider slider)
    {
        data = slider.value;
    }

    public float GetData()
    {
        return data;
    }

    public void Serialise()
    {
        PlayerPrefs.SetFloat(key, data);
    }

    public void SetData(float _data)
    {
        data = _data;
    }
}
