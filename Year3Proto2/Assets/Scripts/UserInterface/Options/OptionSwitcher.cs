using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionSwitcher : OptionObject
{
    [SerializeField] private TMP_Text valueName;
    private int[] values = new int[] { 1, 2};

    private int index;

    private void Start()
    {
        valueName.text = values[0].ToString();
    }
    

    public void Next()
    {
        if (index >= (values.Length - 1))
        {
            index = 0;
        }
        else
        {
            index += 1;
        }
        valueName.text = values[index].ToString();
    }

    public void Previous()
    {
        if (index <= 0)
        {
            index = values.Length - 1;
        }
        else
        {
            index -= 1;
        }
        valueName.text = values[index].ToString();
    }

    public int[] GetValues()
    {
        return values;
    }
}
