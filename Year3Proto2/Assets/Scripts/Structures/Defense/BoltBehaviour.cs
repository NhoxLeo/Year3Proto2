using UnityEngine;

public class BoltBehaviour : MonoBehaviour
{
    private Vector3 targetPosition;
    private float damage;
    private float speed;
    private bool pierce;
    private Vector3 endPosition;
    private bool endPositionReached = false;
    private bool damageDealt = false;

    public void Initialize(Vector3 _targetPosition, float _damage, float _speed, bool _pierce)
    {
        targetPosition = _targetPosition;
        damage = _damage;
        speed = _speed;
        pierce = _pierce;
    }

    private void Start()
    {
        Vector3 invaderMidPointOffset = new Vector3(0f, 0.065f, 0f);
        transform.LookAt(targetPosition + invaderMidPointOffset);
        if (Physics.Raycast(transform.position, (targetPosition + invaderMidPointOffset - transform.position).normalized, out RaycastHit _hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            endPosition = _hit.point;
        }
        else
        {
            endPosition = (targetPosition + invaderMidPointOffset - transform.position).normalized * 1000f;
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
                        BatteringRam ram = enemy.GetComponent<BatteringRam>();
                        if (ram)
                        {
                            enemy.Damage(damage / 3f);
                        }
                        else
                        {
                            enemy.Damage(damage);
                        }
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
