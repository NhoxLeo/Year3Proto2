using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherTower : AttackStructure
{
    public GameObject arrow;
    public GameObject ballista;
    public float arrowDamage = 5.0f;

    private List<GameObject> spawnedArrows = new List<GameObject>();
    private const float arrowSpeed = 2.5f;

    void Start()
    {
        AttackStart();
        structureName = "Archer Tower";
    }

    private void Update()
    {
        AttackUpdate();

        if(target != null)
        {
            Vector3 ballistaPosition = ballista.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = ballistaPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            ballista.transform.rotation = Quaternion.Slerp(ballista.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * arrowSpeed);

        }
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

            if (Vector3.Distance(arrowPosition, targetPosition) <= 0.05f)
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

            spawnedArrow = Instantiate(arrow, ballista.transform.position, Quaternion.identity, transform);
            GameManager.CreateAudioEffect("arrow", transform.position);
        }
    }
}
