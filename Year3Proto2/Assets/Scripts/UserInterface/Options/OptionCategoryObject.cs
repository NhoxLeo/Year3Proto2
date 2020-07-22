using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCategoryObject : MonoBehaviour
{
    private Button button;

    public void SetButton(Button _button)
    {
        button = _button;
    }

    public Button GetButton()
    {
        return button;
    }
}
