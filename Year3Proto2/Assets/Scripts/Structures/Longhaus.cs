using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Longhaus : Structure
{
    [SerializeField]
    static public int foodStorage = 500;

    [SerializeField]
    static public int woodStorage = 500;

    [SerializeField]
    static public int metalStorage = 500;

    public float productionTime = 3f;
    protected float remainingTime = 3f;

    private bool longhausDead;

    // Start is called before the first frame update
    void Start()
    {
        StructureStart();
        structureType = StructureType.longhaus;
        structureName = "Longhaus";
        longhausDead = false;
        maxHealth = 400f;
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        StructureUpdate();
        if (health > 0)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                GameManager game = FindObjectOfType<GameManager>();
                game.AddBatch(new Batch(3, ResourceType.metal));
                game.AddBatch(new Batch(7, ResourceType.wood));
                game.AddBatch(new Batch(7, ResourceType.food));
            }
        }
    }
}
