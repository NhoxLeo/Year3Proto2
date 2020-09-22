using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTowerCannon : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float viewRadius = 5.0f;
    [SerializeField] private float damage = 0.02f;
    [Range(0, 360)] [SerializeField] private float viewAngle = 60.0f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Material material;
    [SerializeField] private ParticleSystem particle;

    private readonly List<Transform> targets = new List<Transform>();

    private void Start()
    {
        particle.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) != 0)
        {
            Vector3 direction = (other.transform.position - transform.position).normalized;
            // If transform is inside angle based on snow cannons transform
            if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy)
                {
                    enemy.Slow(true);
                    targets.Add(other.transform);
                }

            }
        }

        if (targets.Count > 0) particle.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        if ((targetMask.value & (1 << other.gameObject.layer)) != 0)
        {
            if (targets.Contains(other.transform))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy)
                {
                    enemy.Slow(false);
                    targets.Remove(other.transform);
                }
            }
        }

        if (targets.Count < 0) particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    private Vector3 DirectionFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 viewAngleA = DirectionFromAngle(-viewAngle / 2.0f);
        Vector3 viewAngleB = DirectionFromAngle(viewAngle / 2.0f);

        Vector3 lineA = transform.position + viewAngleA * viewRadius;
        Vector3 lineB = transform.position + viewAngleB * viewRadius;

        Gizmos.DrawLine(transform.position, lineA);
        Gizmos.DrawLine(transform.position, lineB);
    }

    public List<Transform> GetTargets()
    {
        return targets;
    }
}
