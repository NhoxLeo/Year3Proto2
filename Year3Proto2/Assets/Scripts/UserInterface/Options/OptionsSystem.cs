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

    private readonly List<OptionObject> optionObjects = new List<OptionObject>();
    private void Awake()
    {
        // TOGGLES

        OptionToggleData fullscreenData = new OptionToggleData(true, true);
        fullscreenData.CallBack(() => Screen.fullScreenMode = fullscreenData.value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        /*
        OptionToggleData mouseCameraControlData = new OptionToggleData(true, false);
        mouseCameraControlData.CallBack(() => { });
        optionObjects.Add(InstantiateOption("MOUSE_EDGE_CAMERA_CONTROL", mouseCameraControlData, togglePrefab, controlsPanel));
        */

        /*OptionToggleData vSyncData = new OptionToggleData(false, false);
        vSyncData.CallBack(() => QualitySettings.vSyncCount = vSyncData.value ? 1 : 0);
        */

        // SWITCHERS
        OptionSwitcherData resolutionData = new OptionSwitcherData(Screen.resolutions.Length - 1, 0, Screen.resolutions.Select(o => o.ToString()).ToArray());
        resolutionData.CallBack(() => 
        {
            Resolution resolution = Screen.resolutions[resolutionData.value];
            Screen.SetResolution(resolution.width, resolution.height, fullscreenData.value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        });

        /*
        OptionSwitcherData textureQualityData = new OptionSwitcherData(0, 0, new string[] { "High", "Medium", "Low" });
        textureQualityData.CallBack(() => QualitySettings.masterTextureLimit = textureQualityData.value);
        optionObjects.Add(InstantiateOption("TEXTURE_QUALITY", textureQualityData, switcherPrefab, graphicsPanel));

        OptionSwitcherData shadowQualityData = new OptionSwitcherData(0, 0, Enum.GetNames(typeof(ShadowResolution)));
        shadowQualityData.CallBack(() => QualitySettings.shadowResolution = (ShadowResolution)Enum.ToObject(typeof(ShadowResolution), (byte)shadowQualityData.value));
        optionObjects.Add(InstantiateOption("SHADOW_QUALITY", shadowQualityData, switcherPrefab, graphicsPanel));
        */

        // SLIDERS
        OptionSliderData masterVolumeData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.5f, 0.5f);
        masterVolumeData.CallBack(() => AudioListener.volume = masterVolumeData.value);
        
        OptionSliderData soundEffectData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.5f, 0.5f);
        soundEffectData.CallBack(() => SuperManager.EffectsVolume = soundEffectData.value);

        OptionSliderData ambientEffectsData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.5f, 0.5f);
        ambientEffectsData.CallBack(() => SuperManager.AmbientVolume = ambientEffectsData.value);

        OptionSliderData musicEffectsData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.5f, 0.5f);
        ambientEffectsData.CallBack(() => SuperManager.MusicVolume = musicEffectsData.value);

        OptionSliderData cameraMovementData = new OptionSliderData(new Vector2(2.0f, 6.0f), 0.5f, 0.5f);
        cameraMovementData.CallBack(() => SuperManager.CameraSensitivity = cameraMovementData.value);


        // DISPLAY OPTIONS

        optionObjects.Add(InstantiateOption("RESOLUTION", resolutionData, switcherPrefab, displayPanel));
        optionObjects.Add(InstantiateOption("FULL_SCREEN_MODE", fullscreenData, togglePrefab, displayPanel));
        //optionObjects.Add(InstantiateOption("V_SYNC", vSyncData, togglePrefab, displayPanel));

        // AUDIO

        optionObjects.Add(InstantiateOption("MASTER_VOLUME", masterVolumeData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("MUSIC_VOLUME", ambientEffectsData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("SOUND_EFFECTS_VOLUME", soundEffectData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("AMBIENT_EFFECTS_VOLUME", ambientEffectsData, sliderPrefab, audioPanel));

        // CONTROLS
        optionObjects.Add(InstantiateOption("CAMERA_SENSITIVITY", cameraMovementData, sliderPrefab, controlsPanel));

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
