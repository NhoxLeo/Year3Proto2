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

public class EnvironmentSystem : MonoBehaviour
{
    [SerializeField] private Transform[] ambientEvents;
    [SerializeField] private Transform[] weatherEvents;

    private EnvironmentEvent ambientEvent;
    private EnvironmentEvent weatherEvent;

    /**************************************
     * Name of the Function: Start
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: void
     ***************************************/
    private void Start()
    {
        //InvokeEvent(weatherEvents[0]);
        InvokeEvent(ambientEvents[0]);
    }

    /**************************************
     * Name of the Function: Update
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: void
     ***************************************/
    private void Update()
    {
        //if (weatherEvent.IsCompleted()) InvokeEvent(weatherEvents[Random.Range(0, weatherEvents.Length)]);
        if (ambientEvent.IsCompleted())
        {
            Destroy(ambientEvent.gameObject);
            InvokeEvent(ambientEvents[Random.Range(0, ambientEvents.Length)]);
        }
    }

    /**************************************
     * Name of the Function: InvokeEvent
     * @Author: Tjeu Vreeburg
     * @Parameter: Transform
     * @Return: void
     ***************************************/
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
