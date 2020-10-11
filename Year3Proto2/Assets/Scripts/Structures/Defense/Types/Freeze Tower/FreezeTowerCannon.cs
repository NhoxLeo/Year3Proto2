using System.Collections.Generic;
using UnityEngine;

public class FreezeTowerCannon : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float viewRadius = 5.0f;
    [SerializeField] private float damageDelay = 1.2f;
    [Range(0, 360)] [SerializeField] private float viewAngle = 60.0f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private ParticleSystem particle;
    private bool particlesPlaying = false;

    private readonly List<Transform> targets = new List<Transform>();
    private FreezeTower parentTower = null;

    // Research 
    private float slowAmount = 1.0f;

    private void Start()
    {
        particle.Stop();
    }

    private void Awake()
    {
        parentTower = transform.parent.GetComponent<FreezeTower>();
    }

    private void Update()
    {
        if (parentTower.isPlaced)
        {
            targets.RemoveAll(target => !target);
            if (targets.Count > 0 && !particlesPlaying)
            {
                particle.Play();
                particlesPlaying = true;
            }
            else if (targets.Count == 0 && particlesPlaying)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                particlesPlaying = false;
            }
        }
    } 

    private void OnTriggerEnter(Collider other)
    {
        if (parentTower.isPlaced)
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
                        enemy.Slow(true, slowAmount);
                        targets.Add(other.transform);
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (parentTower.isPlaced)
        {
            if (targets.Contains(other.transform))
            {
                return;
            }
            if ((targetMask.value & (1 << other.gameObject.layer)) != 0)
            {
                Vector3 direction = (other.transform.position - transform.position).normalized;
                // If transform is inside angle based on snow cannons transform
                if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
                {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(true, slowAmount);
                        targets.Add(other.transform);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentTower.isPlaced)
        {
            if ((targetMask.value & (1 << other.gameObject.layer)) != 0)
            {
                if (targets.Contains(other.transform))
                {
                    Enemy enemy = other.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(false, slowAmount);
                        targets.Remove(other.transform);
                    }
                }
            }
        }
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

    public void Setup(float _slowPercentage)
    {
        slowAmount = _slowPercentage;
    }

    public List<Transform> GetTargets()
    {
        return targets;
    }
}
