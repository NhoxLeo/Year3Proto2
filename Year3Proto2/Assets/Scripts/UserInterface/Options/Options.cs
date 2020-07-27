using Boo.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct OptionSwitcherData
{
    public int index;
    public string[] values;
    public OptionSwitcherData(int _index, string[] _values)
    {
        index = _index;
        values = _values;
    }
}


public class Options : MonoBehaviour
{
    // TODO: delegates, control values, saving...

    [SerializeField] private Transform[] optionObjects;

    private Dictionary<string, OptionSwitcherData> switchers;
    private Dictionary<string, float> sliders;
    private Dictionary<string, bool> checkBoxes;

    private void Start()
    {
        switchers = new Dictionary<string, OptionSwitcherData>
        {
            { "RESOLUTION", new OptionSwitcherData(0, Screen.resolutions.Select(o => o.ToString()).ToArray()) },
            { "QUALITY", new OptionSwitcherData(0, QualitySettings.names) },
            { "TEXTURE_QUALITY", new OptionSwitcherData(0, QualitySettings.names) },
            { "SHADOW_QUALITY", new OptionSwitcherData(0, QualitySettings.names) },
            { "ANTI_ALIASING", new OptionSwitcherData(0, QualitySettings.names) }
        };

        sliders = new Dictionary<string, float>
        {
            { "MASTER_VOLUME", 0.5f },
            { "MUSIC_VOLUME", 0.5f },
            { "AMBIENT_VOLUME", 0.5f },
            { "SOUND_EFFECTS_VOLUME", 0.5f },
        };

        checkBoxes = new Dictionary<string, bool>
        {
            { "FULL_SCREEN_MODE", true },
            { "V_SYNC", true },
            { "ENEMY_INDICATORS", true},
            { "ENVIRONMENT_TOOLTIPS", true },
            { "AMBIENT_OCCLUSION", true },
            { "WAVE_HORN_START", true }
        };
    }

    public void LoadData()
    {
        for (int i = 0; i < optionObjects.Length; i++)
        {
            Transform optionObject = optionObjects[i];

            OptionSwitcher optionSwitcher = optionObject.GetComponent<OptionSwitcher>();
            if (optionSwitcher)
            {
                if (switchers.TryGetValue(optionSwitcher.GetKey(), out OptionSwitcherData data))
                {
                    optionSwitcher.SetValues(data.values);
                    optionSwitcher.Deserialise(data.index);
                }
            }

            OptionSlider optionSlider = optionObject.GetComponent<OptionSlider>();
            if (optionSlider)
            {
                if (sliders.TryGetValue(optionSlider.GetKey(), out float data)) optionSlider.Deserialise(data);
            }

            OptionCheckBox optionCheckBox = optionObject.GetComponent<OptionCheckBox>();
            if (optionCheckBox)
            {
                if (checkBoxes.TryGetValue(optionCheckBox.GetKey(), out bool data)) optionCheckBox.Deserialise(data);
            }
        }
    }

    public void SaveData()
    {
        // TODO:
        // callbacks
        // storing data
    }
}
