using UnityEngine;

public class Icicle : PhysicalProjectile
{
    [SerializeField] private float stunDuration;

    protected override void OnDestination(Vector3 _location)
    {
        Enemy enemy = target.GetComponent<Enemy>();
    }

    protected override void OnDisplacement(Vector3 _heading, Vector3 _direction, float distance)
    {
        transform.position += _direction * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(_heading);
    }
     
    public void SetStunDuration(float _stunDuration)
    {
        stunDuration = _stunDuration;
    }

    protected override void OnGroundHit()
    {

    }
}
