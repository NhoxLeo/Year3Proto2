using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnvironmentEvent : MonoBehaviour
{
    private protected float time = 0.0f;
    private protected bool completed = false;

    public abstract void Invoke();

    public bool IsCompleted()
    {
        return completed && time <= 0.0f;
    }
}
