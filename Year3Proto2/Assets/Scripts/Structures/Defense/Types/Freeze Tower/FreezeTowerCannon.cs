using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeTowerCannon : MonoBehaviour
{
    // TODO:
    // Eyes changing color. (Material change)

    [Header("Attributes")]
    [SerializeField] private float viewRadius = 5.0f;
    [SerializeField] private float damage = 0.02f;
    [Range(0, 360)] [SerializeField] private float viewAngle = 60.0f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private Material material;
    [SerializeField] private ParticleSystem particle;

    private List<Transform> targets = new List<Transform>();

    private void Start()
    {
        StartCoroutine(FindTargets(3.0f));
    }

    private void Update()
    {
        material.color = targets.Count > 0 ? Color.red : Color.white; 
    }

    IEnumerator FindTargets(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            targets.Clear();

            Collider[] transforms = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform target = transforms[i].transform;
                Vector3 direction = (target.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
                {
                    targets.Add(target);
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
}
