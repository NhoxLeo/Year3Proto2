using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Options : MonoBehaviour
{
    //string filePath = Application.persistentDataPath + "/options.dat";

    public void Save()
    {
        OptionObject[] optionObjects = FindObjectsOfType<OptionObject>();

        for (int i = 0; i < optionObjects.Length; i++)
        {
            Transform transform = optionObjects[i].transform;

            string key = transform.name.Replace(" ", "_");
            string value = null;

            OptionSwitcher optionSwitcher = transform.GetComponent<OptionSwitcher>();
            if (optionSwitcher) value = optionSwitcher.GetValues().ToString();

            OptionCheckBox optionCheckBox = transform.GetComponent<OptionCheckBox>();
            if (optionCheckBox) value = optionCheckBox.IsTicked().ToString();

            OptionSlider optionSlider = transform.GetComponent<OptionSlider>();
            if (optionSlider) value = optionSlider.GetValue().ToString();

            PlayerPrefs.SetString(key.ToUpper(), value);
            Debug.Log(key + " = " + value);
        }
    }

    public void Load()
    {

    }
}
