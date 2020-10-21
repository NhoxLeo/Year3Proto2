using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : EnvironmentWeatherEvent.cs
// Description  : Inherited event to simulate weather events.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class EnvironmentWeatherEvent : EnvironmentEvent
{
    [SerializeField] private Transform weatherPrefab;
    [SerializeField] private EnvironmentWeatherData environmentWeatherData;
    private ParticleSystem weatherObject;

    private void Update()
    {
        if (time > 0.0f)
        {
            time -= Time.deltaTime;
        }
        else
        {
            completed = true;
        }
    }

    public EnvironmentWeatherData SaveData(int _index)
    {
        environmentWeatherData.time = time;
        environmentWeatherData.index = _index;
        return environmentWeatherData;
    }

    public EnvironmentWeatherEvent LoadData(EnvironmentWeatherData _environmentWeatherData)
    {
        time = _environmentWeatherData.time;
        environmentWeatherData = _environmentWeatherData;
        Invoke(true);
        return this;
    }

    /**************************************
     * Name of the Function: Invoke
     * @Author: Tjeu Vreeburg
     * @Parameter: n/aW
     * @Return: override void
     ***************************************/
    public override EnvironmentEvent Invoke(bool _data)
    {
        if(weatherPrefab != null)
        {
            Transform weatherTransform = Instantiate(weatherPrefab, Vector3.zero, Quaternion.identity);
            ParticleSystem weatherObject = weatherTransform.GetComponent<ParticleSystem>();
            if(weatherObject)
            {
                this.weatherObject = weatherObject;
            }
        }
        LightingManager.Instance().SetWeather((Weather)environmentWeatherData.weather);
        return this;
    }

    /**************************************
     * Name of the Function: GetWeather
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: Transform
     ***************************************/
    public ParticleSystem GetWeather()
    {
        return weatherObject;
    }

    public EnvironmentWeatherData GetData()
    {
        return environmentWeatherData;
    }
}