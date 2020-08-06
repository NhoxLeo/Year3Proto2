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
    private Transform weather;

    /**************************************
     * Name of the Function: Invoke
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: override void
     ***************************************/
    public override void Invoke()
    {
        weather = Instantiate(weatherPrefab, Vector3.zero, Quaternion.identity, transform.parent);
    }

    /**************************************
     * Name of the Function: GetWeather
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: Transform
     ***************************************/
    public Transform GetWeather()
    {
        return weather;
    }
}
