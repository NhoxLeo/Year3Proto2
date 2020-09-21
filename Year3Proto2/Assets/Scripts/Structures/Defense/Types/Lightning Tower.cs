using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTower : DefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private Transform lightning;
    [SerializeField] private float lightningAmount;
    [SerializeField] private float lightningDelay = 1.0f;

    private float time;

    protected override void Start()
    {
        base.Start();
        time = lightningDelay;
    }

    protected override void Update()
    {
        base.Update();

        time -= Time.deltaTime;
        if(time <= 0.0f)
        {
            time = lightningDelay;
            StartCoroutine(Strike(0.5f));
        }
    }

    IEnumerator Strike(float seconds)
    {
        List<Transform> enemies = GetEnemies();
        for (int i = 0; i < lightningAmount; i++)
        {
            yield return new WaitForSeconds(seconds);
            Transform transform = enemies[i];
            Enemy enemy = transform.GetComponent<Enemy>();
            if(enemy)
            {
                //Lightning 
            }
        }

        yield return null;
    }
}
