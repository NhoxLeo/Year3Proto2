using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : EnvironmentAmbientEvent.cs
// Description  : Inherited event to simulate ambient events.
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public abstract class EnvironmentAmbientEvent : EnvironmentEvent
{
    [SerializeField] protected Transform ambientPrefab;
    protected Transform ambient;

    /**************************************
     * Name of the Function: GetAmbient
     * @Author: Tjeu Vreeburg
     * @Parameter: n/a
     * @Return: Transform
     ***************************************/
    public Transform GetAmbient()
    {
        return ambient;
    }
}
