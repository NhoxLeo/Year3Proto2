using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : OptionsSystem.cs
// Description  : Manager class for options.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class OptionsSystem : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private Transform display;
    [SerializeField] private Transform graphics;

    [Header("Prefabrication")]
    [SerializeField] private Transform togglePrefab;
    [SerializeField] private Transform sliderPrefab;
    [SerializeField] private Transform switcherPrefab;

    private List<OptionObject> optionObjects = new List<OptionObject>();
    private void Awake()
    {
        optionObjects.Add(InstantiateOption("RESOLUTION",
            new OptionSwitcherData(0, 0, Screen.resolutions.Select(o => o.ToString()).ToArray(),
                delegate
                {
                    OptionSwitcher optionSwitcher = (OptionSwitcher)optionObjects.First(o => o.GetKey() == "RESOLUTION");
                    if (optionSwitcher)
                    {
                        OptionSwitcherData optionSwitcherData = (OptionSwitcherData)optionSwitcher.GetData();
                        Resolution resolution = Screen.resolutions[optionSwitcherData.value];
                        Screen.SetResolution(resolution.width, resolution.height, true);
                    }
                }
            ), switcherPrefab, display)
        );

        optionObjects.Add(InstantiateOption("QUALITY",
            new OptionSwitcherData(0, 0, QualitySettings.names.ToArray(),
               delegate
               {
                   OptionSwitcher optionSwitcher = (OptionSwitcher)optionObjects.First(o => o.GetKey() == "QUALITY");
                   if (optionSwitcher)
                   {
                       OptionSwitcherData optionSwitcherData = (OptionSwitcherData)optionSwitcher.GetData();
                       string[] names = QualitySettings.names;
                       QualitySettings.SetQualityLevel(optionSwitcherData.value);
                   }
               }
            ), switcherPrefab, graphics)
        );

        optionObjects.Add(InstantiateOption("TEXTURE_QUALITY",
            new OptionSwitcherData(0, 0, QualitySettings.names.ToArray(),
               delegate
               {
                   OptionSwitcher optionSwitcher = (OptionSwitcher)optionObjects.First(o => o.GetKey() == "TEXTURE_QUALITY");
                   if (optionSwitcher)
                   {
                       OptionSwitcherData optionSwitcherData = (OptionSwitcherData)optionSwitcher.GetData();
                       string[] names = QualitySettings.names;
                       QualitySettings.SetQualityLevel(optionSwitcherData.value);
                   }
               }
            ), switcherPrefab, graphics)
        );

        optionObjects.Add(InstantiateOption("SHADOW_QUALITY",
            new OptionSwitcherData(0, 0, QualitySettings.names.ToArray(),
               delegate
               {
                   OptionSwitcher optionSwitcher = (OptionSwitcher)optionObjects.First(o => o.GetKey() == "SHADOW_QUALITY");
                   if (optionSwitcher)
                   {
                       OptionSwitcherData optionSwitcherData = (OptionSwitcherData)optionSwitcher.GetData();
                       string[] names = QualitySettings.names;
                       QualitySettings.SetQualityLevel(optionSwitcherData.value);
                   }
               }
            ), switcherPrefab, graphics)
        );



        /* 
        [Switchers]

        RESOLUTION
        QUALITY
        TEXTURE_QUALITY
        SHADOW_QUALITY
        ANTI_ALIASING

        [Sliders]

        MASTER_VOLUME
        MUSIC_VOLUME
        AMBIENT_VOLUME
        SOUND_EFFECTS_VOLUME
        

        pToggles]

        FULL_SCREEN_MODE
        V_SYNC
        ENEMY_INDICATORS
        ENEMY_TOOLTIPS
        AMBIENT_OCCLUSION
        WAVE_HORN_START

         */
    }

    public OptionObject InstantiateOption(string _key, OptionData _optionData, Transform _prefab, Transform _parent)
    {
        Transform transform = Instantiate(_prefab, _parent);
        OptionObject optionObject = transform.GetComponent<OptionObject>();
        if (optionObject)
        {
            optionObject.SetData(_optionData);
            optionObject.SetKey(_key);
            return optionObject;
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
        optionObjects.ForEach(optionObject => optionObject.Deserialize());
    }

    /**************************************
    * Name of the Function: SaveData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SaveData()
    {
        optionObjects.ForEach(optionObject =>
        {
            optionObject.Serialize();
            optionObject.GetData().GetCallback().Invoke();
        });

        PlayerPrefs.Save();
    }
}
