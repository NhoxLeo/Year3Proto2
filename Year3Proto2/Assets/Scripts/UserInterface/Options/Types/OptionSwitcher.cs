using UnityEngine;
using TMPro;

public class OptionSwitcher : OptionObject, OptionData<int>
{
    [SerializeField] private TMP_Text valueName;
    [SerializeField] private int data;
    [SerializeField] private string[] values;

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

    public int GetData()
    {
        return data;
    }

    public void SetData(int _data)
    {
        PlayerPrefs.SetString(key, data.ToString());
    }

    public void Deserialise(int _data)
    {
        data = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : _data;
    }

    public void Serialise()
    {
         PlayerPrefs.SetInt(key, data);
    }

    public void SetValues(string[] _values)
    {
        values = _values;
    }
}
