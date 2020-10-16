using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

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

    private void Start()
    {
        Volume volume = FindObjectOfType<Volume>();
        Light light = FindObjectOfType<Light>();

        // TOGGLES
        OptionToggleData fullscreenData = new OptionToggleData(true);
        fullscreenData.CallBack(() => Screen.fullScreen = fullscreenData.value);

        OptionToggleData waveHornData = new OptionToggleData(true);
        waveHornData.CallBack(() => SuperManager.waveHornStart = waveHornData.value);

        OptionToggleData messageBoxData = new OptionToggleData(true);
        messageBoxData.CallBack(() => SuperManager.messageBox = messageBoxData.value);

        OptionToggleData vSyncData = new OptionToggleData(true);
        vSyncData.CallBack(() => QualitySettings.vSyncCount = vSyncData.value ? 1 : 0);

        // SWITCHERS
        OptionSwitcherData resolutionData = new OptionSwitcherData(Screen.resolutions.Length - 1, Screen.resolutions.Select(o => o.ToString()).ToArray());
        resolutionData.CallBack(() =>
        {
            Resolution resolution = Screen.resolutions[resolutionData.value];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        });

        OptionSwitcherData shadowQualityData = new OptionSwitcherData(2, new string[] { "Low", "Medium", "High", "Ultra" });
        shadowQualityData.CallBack(() => light.shadowResolution = (LightShadowResolution)Enum.ToObject(typeof(LightShadowResolution), shadowQualityData.value));

        OptionSwitcherData antiAliasingData = new OptionSwitcherData(2, new string[] { "SMAA", "MSAA", "TXAA", "FXAA" });
        antiAliasingData.CallBack(() => { });


        OptionSwitcherData ambienOcclusionData = new OptionSwitcherData(2, Enum.GetNames(typeof(ScalableSettingLevelParameter.Level)));
        ambienOcclusionData.CallBack(() => {
            AmbientOcclusion ambientOcclusion;
            if(volume.profile.TryGet(out ambientOcclusion)) {
                ambientOcclusion.quality.SetValue(new ScalableSettingLevelParameter(ambienOcclusionData.value, true));
            }
        });

        // SLIDERS
        OptionSliderData masterVolumeData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.5f);
        masterVolumeData.CallBack(() => AudioListener.volume = masterVolumeData.value);

        OptionSliderData soundEffectData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.25f);
        soundEffectData.CallBack(() => SuperManager.EffectsVolume = soundEffectData.value);

        OptionSliderData ambientEffectsData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.25f);
        ambientEffectsData.CallBack(() => SuperManager.AmbientVolume = ambientEffectsData.value);

        OptionSliderData musicEffectsData = new OptionSliderData(new Vector2(0.0f, 1.0f), 0.25f);
        musicEffectsData.CallBack(() => SuperManager.MusicVolume = musicEffectsData.value);

        OptionSliderData cameraMovementData = new OptionSliderData(new Vector2(2.0f, 6.0f), 4.0f);
        cameraMovementData.CallBack(() => SuperManager.CameraSensitivity = cameraMovementData.value);

        // DISPLAY OPTIONS

        optionObjects.Add(InstantiateOption("RESOLUTION", resolutionData, switcherPrefab, displayPanel));
        optionObjects.Add(InstantiateOption("FULL_SCREEN_MODE", fullscreenData, togglePrefab, displayPanel));
        optionObjects.Add(InstantiateOption("MESSAGE_BOX", messageBoxData, togglePrefab, displayPanel));
        optionObjects.Add(InstantiateOption("V_SYNC", vSyncData, togglePrefab, displayPanel));

        // Graphics


        optionObjects.Add(InstantiateOption("ANTI_ALIASING", antiAliasingData, switcherPrefab, graphicsPanel));
        optionObjects.Add(InstantiateOption("SHADOW_QUALITY", shadowQualityData, switcherPrefab, graphicsPanel));
        optionObjects.Add(InstantiateOption("AMBIENT_OCLUSION", shadowQualityData, switcherPrefab, graphicsPanel));

        // AUDIO

        optionObjects.Add(InstantiateOption("MASTER_VOLUME", masterVolumeData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("MUSIC_VOLUME", musicEffectsData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("SOUND_EFFECTS_VOLUME", soundEffectData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("AMBIENT_EFFECTS_VOLUME", ambientEffectsData, sliderPrefab, audioPanel));
        optionObjects.Add(InstantiateOption("WAVE_HORN_START", waveHornData, togglePrefab, audioPanel));

        // CONTROLS
        optionObjects.Add(InstantiateOption("CAMERA_SENSITIVITY", cameraMovementData, sliderPrefab, controlsPanel));

        optionObjects.ForEach(optionObject => optionObject.Deserialize());
    }

    public OptionObject InstantiateOption(string _key, OptionData _optionData, Transform _prefab, Transform _parent)
    {
        Transform transform = Instantiate(_prefab, _parent);
        OptionObject optionObject = transform.GetComponent<OptionObject>();
        if (optionObject)
        {
            optionObject.SetKey(_key);
            optionObject.SetData(_optionData);
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
