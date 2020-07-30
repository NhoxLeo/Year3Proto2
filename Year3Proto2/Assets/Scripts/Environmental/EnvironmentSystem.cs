using UnityEngine;

public class EnvironmentSystem : MonoBehaviour
{
    [SerializeField] private Transform[] environmentEvents;

    private EnvironmentEvent currentEvent;

    private void Start()
    {
        InvokeNewEvent();
    }

    private void Update()
    {
        if (!currentEvent) return;
        if (currentEvent.IsCompleted()) InvokeNewEvent();
    }

    private void InvokeNewEvent()
    {
        Transform randomEvent = Instantiate(environmentEvents[Random.Range(0, environmentEvents.Length)], Vector3.zero, Quaternion.identity, transform);
        EnvironmentEvent environmentEvent = randomEvent.GetComponent<EnvironmentEvent>();
        if (environmentEvent)
        {
            currentEvent = environmentEvent;
            currentEvent.Invoke();
        }
    }
}
