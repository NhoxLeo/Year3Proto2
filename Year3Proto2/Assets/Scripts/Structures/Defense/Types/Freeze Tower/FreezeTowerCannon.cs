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

    private List<Enemy> previousTargets = new List<Enemy>();
    private readonly List<Enemy> targets = new List<Enemy>();

    private const float interval = 3.0f;
    private float time = 0.0f;

    private void Update()
    {
        time -= Time.deltaTime;
        if(time <= 0.0f)
        {
            time = interval;

            previousTargets = targets;

            targets.Clear();

            Collider[] transforms = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform target = transforms[i].transform;
                Vector3 direction = (target.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(true);
                        targets.Add(enemy);
                    }
                }
            }

            previousTargets.RemoveAll(target => !target);
            for (int i = 0; i < previousTargets.Count; i++)
            {
                Enemy target = previousTargets[i];
                if (!targets.Contains(target))
                {
                    target.Slow(false);
                }
            }

            material.color = targets.Count > 0 ? Color.red : Color.white;
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
}
