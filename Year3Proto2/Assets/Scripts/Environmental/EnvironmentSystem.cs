using UnityEngine;

public class EnvironmentSystem : MonoBehaviour
{
    [SerializeField] private Transform[] ambientEvents;
    [SerializeField] private Transform[] weatherEvents;

    private EnvironmentEvent ambientEvent;
    private EnvironmentEvent weatherEvent;

    // Todo Reference to the post processing volume

    private void Update()
    {

        if (weatherEvent.IsCompleted()) InvokeEvent(weatherEvents[Random.Range(0, weatherEvents.Length)]);
        if (ambientEvent.IsCompleted()) InvokeEvent(ambientEvents[Random.Range(0, ambientEvents.Length)]);
    }

    private void InvokeEvent(Transform _transform)
    {
        Transform transform = Instantiate(_transform, Vector3.zero, Quaternion.identity, this.transform);

        EnvironmentWeatherEvent environmentWeatherEvent = transform.GetComponent<EnvironmentWeatherEvent>();
        if(environmentWeatherEvent)
        {
            environmentWeatherEvent.Invoke();
            weatherEvent = environmentWeatherEvent;
        }

        EnvironmentAmbientEvent environmentAmbientEvent = transform.GetComponent<EnvironmentAmbientEvent>();
        if (environmentAmbientEvent)
        {
            environmentAmbientEvent.Invoke();
            ambientEvent = environmentAmbientEvent;
        }
    }
}
