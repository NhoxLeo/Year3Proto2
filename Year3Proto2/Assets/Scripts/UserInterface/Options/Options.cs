using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : Options.cs
// Description  : Manager class for options.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

delegate void OptionCallback();

// Data container for switchers.
struct OptionSwitcherData
{
    public int index;
    public string[] values;
    public OptionCallback optionCallback;

    public OptionSwitcherData(int _index, string[] _values, OptionCallback _optionCallback)
    {
        index = _index;
        values = _values;
        optionCallback = _optionCallback;
    }
}

// Data container for sliders.
struct OptionSliderData
{
    public float value;
    public OptionCallback optionCallback;

    public OptionSliderData(float _value, OptionCallback _optionCallback)
    {
        value = _value;
        optionCallback = _optionCallback;
    }
}

// Data container for checkbox.
struct OptionCheckboxData
{
    public bool value;
    public OptionCallback optionCallback;

    public OptionCheckboxData(bool _value, OptionCallback _optionCallback)
    {
        value = _value;
        optionCallback = _optionCallback;
    }
}


public class Options : MonoBehaviour
{
    // TODO: delegates, control values, saving...

    [SerializeField] private Transform[] optionObjects;

    private Dictionary<string, OptionSwitcherData> switchers;
    private Dictionary<string, OptionSliderData> sliders;
    private Dictionary<string, OptionCheckboxData> checkBoxes;


    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Start()
    {
        switchers = new Dictionary<string, OptionSwitcherData>
        {
            { "RESOLUTION", new OptionSwitcherData(0, Screen.resolutions.Select(o => o.ToString()).ToArray(), 
                delegate {
                    OptionSwitcher optionSwitcher = (OptionSwitcher) GetOptionObject("RESOLUTION");
                    if(optionSwitcher)
                    {
                        Resolution resolution = Screen.resolutions[optionSwitcher.GetData()];
                        Screen.SetResolution(resolution.width, resolution.height, true);
                    }
                }) 
            },
            { "QUALITY", new OptionSwitcherData(0, QualitySettings.names, 
                delegate {
                    OptionSwitcher optionSwitcher = (OptionSwitcher) GetOptionObject("QUALITY");
                    if(optionSwitcher)
                    {
                        string[] names = QualitySettings.names;
                        QualitySettings.SetQualityLevel(optionSwitcher.GetData());
                    }
                }) 
            },
            { "TEXTURE_QUALITY", new OptionSwitcherData(0, QualitySettings.names,
                delegate {
                    // TODO
                }) 
            },
            { "SHADOW_QUALITY", new OptionSwitcherData(0, QualitySettings.names, 
                delegate { 
                    // TODO
                }) 
            },
            { "ANTI_ALIASING", new OptionSwitcherData(0, QualitySettings.names,
                delegate { 
                    // TODO
                }) 
            }
        };

        sliders = new Dictionary<string, OptionSliderData>
        {
            { "MASTER_VOLUME", new OptionSliderData(1.0f, 
                delegate { 

                }) 
            },
            { "MUSIC_VOLUME", new OptionSliderData(0.5f, 
                delegate { 

                })
            },
            { "AMBIENT_VOLUME", new OptionSliderData(0.5f,
                delegate { 

                }) 
            },
            { "SOUND_EFFECTS_VOLUME", new OptionSliderData(0.5f, 
                delegate { 

                }) 
            },
        };

        checkBoxes = new Dictionary<string, OptionCheckboxData>
        {
            { "FULL_SCREEN_MODE", new OptionCheckboxData(true,
                delegate { 

                }) 
            },
            { "V_SYNC", new OptionCheckboxData(false, 
                delegate {

                }) 
            },
            { "ENEMY_INDICATORS", new OptionCheckboxData(true, 
                delegate { 

                })
            },
            { "ENVIRONMENT_TOOLTIPS", new OptionCheckboxData(true, 
                delegate {

                })
            },
            { "AMBIENT_OCCLUSION", new OptionCheckboxData(false,
                delegate { 

                }) 
            },
            { "WAVE_HORN_START", new OptionCheckboxData(true, 
                delegate { 

                }) 
            }
        };
    }

    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public OptionObject GetOptionObject(string _key)
    {
        for (int i = 0; i < optionObjects.Length; i++)
        {
            Transform transform = optionObjects[i];
            OptionObject optionObject = transform.GetComponent<OptionObject>();
            if (optionObject && optionObject.GetKey() == _key) return optionObject;
        }
        return null;
    }

    /**************************************
    * Name of the Function: LoadData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
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
                if (sliders.TryGetValue(optionSlider.GetKey(), out OptionSliderData data))
                {
                    optionSlider.Deserialise(data.value);
                }
            }

            OptionCheckBox optionCheckBox = optionObject.GetComponent<OptionCheckBox>();
            if (optionCheckBox)
            {
                if (checkBoxes.TryGetValue(optionCheckBox.GetKey(), out OptionCheckboxData data))
                {
                    optionCheckBox.Deserialise(data.value);
                }
            }
        }
    }

    /**************************************
    * Name of the Function: SaveData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SaveData()
    {
        for (int i = 0; i < optionObjects.Length; i++)
        {
            Transform optionObject = optionObjects[i];

            OptionSwitcher optionSwitcher = optionObject.GetComponent<OptionSwitcher>();
            if (optionSwitcher)
            {
                if (switchers.TryGetValue(optionSwitcher.GetKey(), out OptionSwitcherData data))
                {
                    optionSwitcher.Serialise();
                    data.optionCallback.Invoke();
                }
            }

            OptionSlider optionSlider = optionObject.GetComponent<OptionSlider>();
            if (optionSlider)
            {
                if (sliders.TryGetValue(optionSlider.GetKey(), out OptionSliderData data))
                {
                    optionSlider.Serialise();
                    data.optionCallback.Invoke();
                }
            }

            OptionCheckBox optionCheckBox = optionObject.GetComponent<OptionCheckBox>();
            if (optionCheckBox)
            {
                if (checkBoxes.TryGetValue(optionCheckBox.GetKey(), out OptionCheckboxData data))
                {
                    optionCheckBox.Serialise();
                    data.optionCallback.Invoke();
                }
            }
        }
    }
}
