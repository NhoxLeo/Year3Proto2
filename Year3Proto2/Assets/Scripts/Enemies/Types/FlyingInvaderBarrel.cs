using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInvaderBarrel : MonoBehaviour
{
    private float damage;
    private float explosionRadius = 0.25f;
    private float accelerationRate = 0.175f;
    private float speed = 0.0f;
    private float maxSpeed = 1.0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (speed < maxSpeed)
        {
            speed += accelerationRate * Time.fixedDeltaTime;
        }
        else if (speed > maxSpeed)
        {
            speed = maxSpeed;
        }
        Vector3 newPosition = transform.position - new Vector3(0f, speed, 0f);
        transform.position = newPosition;

        if (transform.position.y <= 0.51f)
        {
            SetOff();
        }
    }

    public void Initialize(float _damage)
    {
        damage = _damage;
    }

    public void SetOff(Structure _hitStructure = null)
    {
        RaycastHit[] hitStructures = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0f, LayerMask.GetMask("Structure"));
        GameObject explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
        explosion.transform.localScale *= 2f * explosionRadius;
        foreach (RaycastHit structureHit in hitStructures)
        {
            Structure structure = structureHit.transform.GetComponent<Structure>();
            if (structure)
            {
                if (structure.GetStructureType() == StructureType.Environment)
                {
                    continue;
                }
                if (structure == _hitStructure)
                {
                    continue;
                }
                float damageToThisStructure = damage * (transform.position - structure.transform.position).magnitude / explosionRadius;
                float clamped = Mathf.Clamp(damageToThisStructure, damage * 0.3f, damage);
                structure.Damage(clamped);
            }
        }
        _hitStructure.Damage(damage);
        GameManager.CreateAudioEffect("Explosion", transform.position, SoundType.SoundEffect, 0.6f);
        Destroy(gameObject);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Structure"))
        {
            SetOff(collision.gameObject.GetComponent<Structure>());
        }
    }
}
