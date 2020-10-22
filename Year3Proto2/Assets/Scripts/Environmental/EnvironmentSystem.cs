using System.Collections;
using UnityEngine;
using DG.Tweening;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : EnvironmentSystem.cs
// Description  : Handles ambient and weather events.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

[System.Serializable]
public struct EnvironmentAmbientData
{
    public int index;
    public SuperManager.SaveVector3 position;
    public SuperManager.SaveVector3 destination;

    public EnvironmentAmbientData(int _index, Vector3 _position, Vector3 _destination)
    {
        index = _index;
        position = new SuperManager.SaveVector3(_position);
        destination = new SuperManager.SaveVector3(_destination);
    }
}

[System.Serializable]
public struct EnvironmentWeatherData
{
    public int index;
    public int weather;
    public float time;

    public EnvironmentWeatherData(int _index, int _weather, float _time)
    {
        index = _index;
        weather = _weather;
        time = _time;
    }
}

public class EnvironmentSystem : MonoBehaviour
{
    private static EnvironmentSystem instance;

    [Header("Prefabrication")]
    [SerializeField] private EnvironmentAmbientEvent[] ambientEvents;
    [SerializeField] private EnvironmentWeatherEvent[] weatherEvents;

    [Header("Current Events")]
    [SerializeField] private EnvironmentAmbientEvent ambientEvent;
    [SerializeField] private EnvironmentWeatherEvent weatherEvent;

    private int weatherIndex;
    private int ambientIndex;
    private bool loaded = false;
    private ParticleSystem currentParticles;

    private void Awake()
    {
        instance = this;
    }

    /**************************************
     * Name of the Function: Update
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: void
     ***************************************/
    private void Update()
    {
        if (loaded)
        {
            if (weatherEvent)
            {
                if (weatherEvent.IsCompleted())
                {
                    Destroy(weatherEvent.gameObject);

                    if(currentParticles)
                    {
                        currentParticles.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                    }

                    InvokeWeather(true);
                }
            }

            if (ambientEvent)
            {
                if (ambientEvent.IsCompleted())
                {
                    Destroy(ambientEvent.gameObject);
                    InvokeAmbient();
                }
            }
        }
        else
        {
            if(!SuperManager.GetInstance().GetSavedMatch().match)
            {
                InvokeWeather(false);
                InvokeAmbient();
                loaded = true;
            }
        }
    }

    private void InvokeAmbient()
    {
        ambientIndex = Random.Range(0, ambientEvents.Length);
        ambientEvent = Instantiate(ambientEvents[ambientIndex], transform);
        ambientEvent.Invoke(false);
    }

    private void InvokeWeather(bool _random)
    {
        weatherIndex = Random.Range(0, 4);

        weatherIndex = (weatherIndex < 1) ? (SuperManager.GetInstance().GetSnow() ? 2 : 1) : 0;
        weatherIndex = _random ? weatherIndex : 0;
        weatherEvent = Instantiate(weatherEvents[weatherIndex], transform);
        weatherEvent.Invoke(false);

        StartCoroutine(SimulateWeather());
    }

    public void LoadData(SuperManager.MatchSaveData _data)
    {
        if(!_data.environmentAmbientData.Equals(default(EnvironmentAmbientData)))
        {
            ambientIndex = _data.environmentAmbientData.index;
            ambientEvent = Instantiate(ambientEvents[ambientIndex], transform);
            ambientEvent.LoadData(_data.environmentAmbientData);
            ambientEvent.Invoke(true);
        }

        if (!_data.environmentWeatherData.Equals(default(EnvironmentWeatherData)))
        { 
            weatherIndex = _data.environmentWeatherData.index;
            weatherEvent = Instantiate(weatherEvents[weatherIndex], transform);
            weatherEvent.LoadData(_data.environmentWeatherData);
            weatherEvent.Invoke(true);
        }

        loaded = true;

        StartCoroutine(SimulateWeather());
    }

    public void SaveSystemToData(ref SuperManager.MatchSaveData _data)
    {
        if (weatherEvent) _data.environmentWeatherData = weatherEvent.SaveData(weatherIndex);
        if (ambientEvent) _data.environmentAmbientData = ambientEvent.SaveData(ambientIndex);
    }

    public static EnvironmentSystem GetInstance()
    {
        return instance;
    }

    IEnumerator SimulateWeather()
    {
        ParticleSystem particleSystem = currentParticles;

        currentParticles = weatherEvent.GetWeather();

        if(currentParticles)
        {
            currentParticles.Play();
        }

        yield return new WaitForSeconds(3);

        if(particleSystem)
        {
            Destroy(particleSystem.gameObject);
        }

        if (weatherEvent.GetData().weather == 1)
        {
            SuperManager.GetInstance().GetRainAudio()
                .DOFade(SuperManager.AmbientVolume, 3.0f)
                .OnComplete(() => SuperManager.raining = true);
        }
        else
        {
            SuperManager.GetInstance().GetRainAudio()
                .DOFade(0, 3.0f)
                .OnComplete(() => SuperManager.raining = false);
        }

        yield return null;
    }
}
