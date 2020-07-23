using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionCheckBox : OptionObject
{
    private bool ticked = true;

    public void SetTicked()
    {
        ticked = ticked ? false : true;
        Debug.Log(ticked);
    }

    public bool IsTicked()
    {
        return ticked;
    }
}
