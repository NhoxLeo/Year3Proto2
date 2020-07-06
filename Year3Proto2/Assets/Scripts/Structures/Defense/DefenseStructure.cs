using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    public float consumptionTime = 2f;
    protected float remainingTime = 2f;

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.defense;
    }

    protected override void Update()
    {
        base.Update();

        // Food consumption
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = consumptionTime;
            if (gameMan.playerResources.CanAfford(new ResourceBundle(0, 0, foodAllocation)))
            {
                gameMan.AddBatch(new ResourceBatch(-foodAllocation, ResourceType.food));
            }
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta -= new Vector3(0f, 0f, foodAllocation / consumptionTime);

        return resourceDelta;
    }
}
