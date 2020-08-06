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
    [Header("Attributes/Prefabrication")]
    [SerializeField] private Vector2 time = new Vector2(60, 120);
    [SerializeField] private Transform[] ambientEvents;
    [SerializeField] private Transform[] weatherEvents;

    [Header("Current Events")]
    [SerializeField] private EnvironmentAmbientEvent ambientEvent;
    [SerializeField] private EnvironmentWeatherEvent weatherEvent;

    /**************************************
     * Name of the Function: Start
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: void
     ***************************************/
    private void Start()
    {
        InvokeEvent(weatherEvents[0]);
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
        if (weatherEvent.IsCompleted())
        {
            Destroy(weatherEvent.gameObject);
            InvokeEvent(weatherEvents[Random.Range(0, weatherEvents.Length)]);
        }


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
        if (environmentWeatherEvent)
        {
            weatherEvent = environmentWeatherEvent;
            weatherEvent.Invoke();
            weatherEvent.SetTime(Random.Range(time.x, time.y));
        }

        EnvironmentAmbientEvent environmentAmbientEvent = transform.GetComponent<EnvironmentAmbientEvent>();
        if (environmentAmbientEvent)
        {
            ambientEvent = environmentAmbientEvent;
            ambientEvent.Invoke();
        }
    }
}
