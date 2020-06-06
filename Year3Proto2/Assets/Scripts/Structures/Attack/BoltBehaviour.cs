using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltBehaviour : MonoBehaviour
{
    public Transform target;
    public float damage;
    public float speed;
    public GameObject puffEffect;
    public bool pierce;
    private Vector3 endPosition;
    private bool endPositionReached = false;

    private void Start()
    {
        Vector3 invaderMidPointOffset = new Vector3(0f, 0.065f, 0f);
        transform.LookAt(target.position + invaderMidPointOffset);
        if (Physics.Raycast(transform.position, (target.position + invaderMidPointOffset - transform.position).normalized, out RaycastHit _hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            endPosition = _hit.point;
        }
        else
        {
            endPosition = (target.position + invaderMidPointOffset - transform.position).normalized * 1000f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!endPositionReached)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, Time.deltaTime * speed);
            if ((transform.position - endPosition).magnitude <= 0.01f)
            {
                endPositionReached = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!endPositionReached)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("EnemyStructureCollider"))
            {
                Enemy enemy = other.GetComponentInParent<Enemy>();
                if (enemy)
                {
                    enemy.Damage(damage);
                    if (!pierce)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}
