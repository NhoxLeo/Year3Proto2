using UnityEngine;

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
                    weatherIndex = Random.Range(0, weatherEvents.Length);
                    InvokeWeather();
                }
            }
            else
            {
                InvokeWeather();
            }

            if (ambientEvent)
            {
                if (ambientEvent.IsCompleted())
                {
                    Destroy(ambientEvent.gameObject);
                    InvokeAmbient();
                }
            }
            else
            {
                InvokeWeather();
            }
        }
    }

    private void InvokeAmbient()
    {
        ambientIndex = Random.Range(0, ambientEvents.Length);
        ambientEvent = Instantiate(ambientEvents[ambientIndex], transform)
            .Invoke(false) as EnvironmentAmbientEvent;
    }

    private void InvokeWeather()
    {
        weatherIndex = Random.Range(0, weatherEvents.Length);
        weatherEvent = Instantiate(weatherEvents[weatherIndex], transform)
            .Invoke(false) as EnvironmentWeatherEvent;
    }

    public void LoadData(SuperManager.MatchSaveData _data)
    {
        if(_data.environmentAmbientData.Equals(default(EnvironmentAmbientData)))
        {
            InvokeAmbient();
        } 
        else
        {
            ambientEvent = Instantiate(ambientEvents[_data.environmentAmbientData.index], transform)
                .LoadData(_data.environmentAmbientData);
        }


        if (_data.environmentWeatherData.Equals(default(EnvironmentWeatherData)))
        {
            InvokeWeather();
        }
        else
        {
            Instantiate(weatherEvents[_data.environmentWeatherData.index], transform)
                .LoadData(_data.environmentWeatherData);
        }

        loaded = true;
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
}
