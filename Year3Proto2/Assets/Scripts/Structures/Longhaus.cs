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

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.longhaus;
        structureName = "Longhaus";
        maxHealth = 400f;
        health = maxHealth;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (health > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
                gameMan.AddBatch(new ResourceBatch(3, ResourceType.metal));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.wood));
                gameMan.AddBatch(new ResourceBatch(7, ResourceType.food));
            }
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta += new Vector3(7f / productionTime, 3f / productionTime, 7f / productionTime);

        return resourceDelta;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        Debug.LogError("Food Allocation should not be called for " + structureName);
    }
}
