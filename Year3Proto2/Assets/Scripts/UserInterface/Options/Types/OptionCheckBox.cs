using UnityEngine;
public class OptionCheckBox : OptionObject, OptionData<bool>
{
    [SerializeField] private bool data;

    public void Deserialise(bool _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) != 0 : _data;
    }

    public bool GetData()
    {
        return data;
    }

    public void Serialise()
    {
        PlayerPrefs.SetInt(key, data ? 1 : 0);
    }

    public void SetData(bool _data)
    {
        data = _data;
    }
}
