using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTower : AttackStructure
{
    public GameObject arrow;

    private GameObject spawnedArrow;
    private const float arrowSpeed = 2.5f;
    public float arrowDamage = 5.0f;

    void Start()
    {
        AttackStart();
        structureName = "Archer Tower";
    }

    private void Update()
    {
        AttackUpdate();

        if (target == null && spawnedArrow != null) Destroy(spawnedArrow);
    }

    public override void Attack(GameObject target)
    {
        if (spawnedArrow != null)
        {
            if(target == null) Destroy(spawnedArrow.gameObject);

            spawnedArrow.transform.LookAt(target.transform);

            Vector3 arrowPosition = spawnedArrow.transform.position;
            Vector3 targetPosition = target.transform.position;

            spawnedArrow.transform.position = Vector3.MoveTowards(arrowPosition, targetPosition, Time.deltaTime * arrowSpeed);

            if(Vector3.Distance(arrowPosition, targetPosition) <= 0.05f)
            {
                if (target.GetComponent<Enemy>().health <= 0.0f)
                {
                    enemies.Remove(target);
                    Destroy(target);
                }
                else
                {
                    target.GetComponent<Enemy>().health -= arrowDamage;
                }

                Destroy(spawnedArrow.gameObject);
            }
        }
        else
        {
            spawnedArrow = Instantiate(arrow, transform.position + new Vector3(0.0f, transform.localScale.y / 2.0f, 0.0f), Quaternion.identity, transform);
            GameManager.CreateAudioEffect("arrow", transform.position);
        }
    }
}
