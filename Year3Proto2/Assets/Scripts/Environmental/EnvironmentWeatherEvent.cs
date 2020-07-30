using UnityEngine;

public abstract class EnvironmentWeatherEvent : EnvironmentEvent
{
    [SerializeField] private Transform weatherPrefab;
    private Transform weather;

    public override void Invoke()
    {
        weather = Instantiate(weatherPrefab, Vector3.zero, Quaternion.identity, transform.parent);
    }

    public Transform GetWeather()
    {
        return weather;
    }
}
