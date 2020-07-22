
using UnityEngine;
using TMPro;

public enum OptionType
{
    CheckBox,
    Slider,
    Switcher
}

[SerializeField]
public class OptionObject : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private Transform element;
    [SerializeField] private OptionElement optionElement;
    [SerializeField] protected OptionType optionType;

    public OptionObject Initialise(OptionElement _optionElement)
    {
        optionElement = _optionElement;

        displayName.text = optionElement.displayName;
        optionType = optionElement.optionType;

        return this;
    }
}
