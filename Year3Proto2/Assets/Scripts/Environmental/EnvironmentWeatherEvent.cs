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

public abstract class EnvironmentWeatherEvent : EnvironmentEvent
{
    [SerializeField] private Transform weatherPrefab;
    [SerializeField] private Weather weather;
    private Transform weatherObject;

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

    /**************************************
     * Name of the Function: Invoke
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: override void
     ***************************************/
    public override void Invoke()
    {
        if(weatherPrefab != null)
        {
            weatherObject = Instantiate(weatherPrefab, Vector3.zero, Quaternion.identity, transform.parent);
        }

        LightingManager.Instance().SetWeather(weather, this);
    }

    /**************************************
     * Name of the Function: GetWeather
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: Transform
     ***************************************/
    public Transform GetWeather()
    {
        return weatherObject;
    }
}