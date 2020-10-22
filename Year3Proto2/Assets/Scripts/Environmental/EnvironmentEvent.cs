using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : EnvironmentEvent.cs
// Description  : Base class to create functionality for different types of events.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com
public abstract class EnvironmentEvent : MonoBehaviour
{
    [SerializeField] protected float time = 0.0f;
    protected bool completed = false;

    /**************************************
     * Name of the Function: Invoke
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: abstract void
     ***************************************/
    public abstract void Invoke(bool _data);

    /**************************************
     * Name of the Function: Update
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: boolean
     ***************************************/
    public bool IsCompleted()
    {
        return completed && time <= 0.0f;
    }
    
    public void SetTime(float _time)
    {
        time = _time;
    }
}
