using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingInvaderBarrel : MonoBehaviour
{
    private float damage = 25f;
    private float accelerationRate = 0.175f;
    private float explosionRadius = 0.25f;
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
                    float damageToThisStructure = damage * (transform.position - structure.transform.position).magnitude / explosionRadius;
                    structure.Damage(damageToThisStructure);
                }
            }
            GameManager.CreateAudioEffect("Explosion", transform.position);
            Destroy(gameObject);
        }
    }

    public void Initialize(float _damage, float _accelerationRate, float _explosionRadius)
    {
        damage = _damage;
        accelerationRate = _accelerationRate;
        explosionRadius = _explosionRadius;
    }
}
