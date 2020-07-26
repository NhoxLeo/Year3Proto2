using UnityEngine;
using UnityEngine.UI;

public class OptionSlider : OptionObject, OptionData<float>
{
    [SerializeField] private Slider slider;
    private float data;

    public void Deserialise(float _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : _data;
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

    private void Start()
    {
        slider.onValueChanged.AddListener(value => data = value); 
    }
}
