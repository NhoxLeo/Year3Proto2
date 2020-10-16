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
    [SerializeField] private EnvironmentAmbientData environmentAmbientData;
    protected Transform ambient;
    protected Vector3 destination;

    public EnvironmentAmbientData SaveData(int _index)
    {
        environmentAmbientData.index = _index;
        environmentAmbientData.position = new SuperManager.SaveVector3(ambient.position);
        environmentAmbientData.destination = new SuperManager.SaveVector3(destination);
        return environmentAmbientData;
    }

    public EnvironmentAmbientEvent LoadData(EnvironmentAmbientData _environmentAmbientData)
    {
        destination = _environmentAmbientData.destination;
        transform.position = _environmentAmbientData.position;
        Invoke(true);
        return this;
    }

    public Vector3 GetDestination ()
    {
        return destination;
    }


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
