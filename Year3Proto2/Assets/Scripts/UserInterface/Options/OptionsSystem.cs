using System;
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
// File Name    : OptionsSystem.cs
// Description  : Manager class for options.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class OptionsSystem : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private Transform displayPanel;
    [SerializeField] private Transform graphicsPanel;
    [SerializeField] private Transform audioPanel;
    [SerializeField] private Transform controlsPanel;

    [Header("Prefabrication")]
    [SerializeField] private Transform togglePrefab;
    [SerializeField] private Transform sliderPrefab;
    [SerializeField] private Transform switcherPrefab;

    private List<OptionObject> optionObjects = new List<OptionObject>();
    private void Awake()
    {

        // SWITCHERS
        OptionSwitcherData optionSwitcherData = new OptionSwitcherData(0, 0, Screen.resolutions.Select(o => o.ToString()).ToArray());
        optionSwitcherData.CallBack(() => 
        {
            Resolution resolution = Screen.resolutions[optionSwitcherData.value];
            Screen.SetResolution(resolution.width, resolution.height, true);
        });
        optionObjects.Add(InstantiateOption("RESOLUTION", optionSwitcherData, switcherPrefab, displayPanel));

        optionSwitcherData = new OptionSwitcherData(0, 0, new string[] { "High", "Medium", "Low" });
        optionSwitcherData.CallBack(() => QualitySettings.masterTextureLimit = optionSwitcherData.value);
        optionObjects.Add(InstantiateOption("TEXTURE_QUALITY", optionSwitcherData, switcherPrefab, graphicsPanel));


        optionSwitcherData = new OptionSwitcherData(0, 0, Enum.GetNames(typeof(ShadowResolution)));
        optionSwitcherData.CallBack(() => QualitySettings.shadowResolution = (ShadowResolution)Enum.ToObject(typeof(ShadowResolution), (byte)optionSwitcherData.value));
        optionObjects.Add(InstantiateOption("SHADOW_QUALITY", optionSwitcherData, switcherPrefab, graphicsPanel));

        // SLIDERS
        OptionSliderData optionSliderData = new OptionSliderData(0.5f, 0.5f);
        optionSliderData.CallBack(() => AudioListener.volume = optionSliderData.value);
        optionObjects.Add(InstantiateOption("MASTER_VOLUME", optionSliderData, sliderPrefab, audioPanel));

        optionSliderData = new OptionSliderData(0.5f, 0.5f);
        optionSliderData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("SOUND_EFFECTS_VOLUME", optionSliderData, sliderPrefab, audioPanel));

        optionSliderData = new OptionSliderData(0.5f, 0.5f);
        optionSliderData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("AMBIENT_EFFECTS_VOLUME", optionSliderData, sliderPrefab, audioPanel));

        optionSliderData = new OptionSliderData(0.5f, 0.5f);
        optionSliderData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("CAMERA_ZOOM_SENSITIVITY", optionSliderData, sliderPrefab, controlsPanel));

        optionSliderData = new OptionSliderData(0.5f, 0.5f);
        optionSliderData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("CAMERA_MOVEMENT_SENSITIVITY", optionSliderData, sliderPrefab, controlsPanel));


        // TOGGLES

        OptionToggleData optionToggleData = new OptionToggleData(true, false);
        optionToggleData.CallBack(() => Screen.fullScreenMode = optionToggleData.value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        optionObjects.Add(InstantiateOption("FULL_SCREEN_MODE", optionToggleData, togglePrefab, displayPanel));

        optionToggleData = new OptionToggleData(true, false);
        optionToggleData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("MOUSE_EDGE_CAMERA_CONTROL", optionToggleData, togglePrefab, controlsPanel));

        optionToggleData = new OptionToggleData(false, false);
        optionToggleData.CallBack(() => QualitySettings.vSyncCount = optionToggleData.value ? 1 : 0);
        optionObjects.Add(InstantiateOption("V_SYNC", optionToggleData, togglePrefab, displayPanel));
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

    private void Start()
    {
        optionObjects.ForEach(optionObject => optionObject.Deserialize());
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
    * Name of the Function: SaveData
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    public void SaveData()
    {
        optionObjects.ForEach(optionObject => optionObject.Serialize());
        PlayerPrefs.Save();
    }
}
