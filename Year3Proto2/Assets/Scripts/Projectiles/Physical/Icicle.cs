using UnityEngine;

public class Icicle : PhysicalProjectile
{
    [SerializeField] private float stunDuration;

    protected override void OnDestination(Vector3 _location)
    {
        throw new System.NotImplementedException();
    }

    protected override void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance)
    {
        throw new System.NotImplementedException();
    }
     
    public void SetStunDuration(float _stunDuration)
    {
        stunDuration = _stunDuration;
    }
}
