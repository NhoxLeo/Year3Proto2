using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour
{
    private Rigidbody body;
    private Enemy target = null;
    private Animator animator;
    [HideInInspector]
    public GameObject puffEffect;
    [HideInInspector]
    public float health = 30.0f;
    [HideInInspector]
    public float maxHealth = 30.0f;
    [HideInInspector]
    public float damage = 5.0f;
    [HideInInspector]
    public float movementSpeed = 0.5f;
    [HideInInspector]
    public bool canHeal = false;
    public float healRate = 0.5f;
    public Barracks home;
    public int state = 0; // 0 idle, 1 moving, 2 attacking

    private void LookAtPosition(Vector3 _position)
    {
        transform.LookAt(_position);
        // fixing animation problems
        transform.forward = transform.right;
    }

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
    }

    // Update is called once per frame
    private void Update()
    {
        animator.SetInteger("State", state);
        if (canHeal)
        {
            if (health != maxHealth)
            {
                health += healRate * Time.deltaTime;
                if (health > maxHealth) { health = maxHealth; }
            }
        }
        switch (state)
        {
            case 0:
                // idle
                break;
            case 1:
                // running
                break;
            case 2:
                // attacking
            default:
                break;
        }
    }
}
