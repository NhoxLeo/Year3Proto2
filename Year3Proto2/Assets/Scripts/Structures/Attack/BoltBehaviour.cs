using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltBehaviour : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private GameObject puffEffect;
    private bool pierce;
    private Vector3 endPosition;
    private bool endPositionReached = false;
    private bool damageDealt = false;

    public void Initialize(Transform _target, float _damage, float _speed, GameObject _puffEffect, bool _pierce)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        puffEffect = _puffEffect;
        pierce = _pierce;
    }

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
            if (!damageDealt || pierce)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("EnemyStructureCollider"))
                {
                    Enemy enemy = other.GetComponentInParent<Enemy>();
                    if (enemy)
                    {
                        enemy.Damage(damage);
                        damageDealt = true;
                        if (!pierce)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
            
        }
    }
}
