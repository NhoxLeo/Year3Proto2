using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileTime;
    [SerializeField] private float projectileDelay;
    [SerializeField] private float projectileRate;

    protected ResourceBundle attackCost;
    protected Transform target;

    protected override void Start()
    {
        base.Start();
        structureType = StructureType.Defense;
        projectileTime = projectileDelay;
        
        /*villagerWidget = Instantiate(structMan.villagerWidgetPrefab, structMan.canvas.transform.Find("HUD/VillagerAllocationWidgets")).GetComponent<VillagerAllocation>();
        villagerWidget.SetTarget(this);*/
    }

    private void Update()
    {
        if(attachedTile)
        {
            projectileTime += Time.deltaTime;
            if(projectileTime >= projectileDelay && gameMan.playerResources.AttemptPurchase(attackCost))
            {
                Projectile projectile = Instantiate(projectilePrefab, transform);
                if (projectile)
                {
                    projectile.Launch();
                    projectileTime = 0.0f;
                }
            }
        }
    }

    public void SetProjectileRate(float _projectileRate)
    {
        projectileRate = allocatedVillagers * 0.5f;
        projectileDelay = 1.0f / projectileRate;
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();
        if (target)
        {
            resourceDelta -= attackCost * projectileRate;
        }
        return resourceDelta;
    }


    public override void OnSelected()
    {
        base.OnSelected();
        //FindObjectOfType<HUDManager>().ShowOneVillagerWidget(villagerWidget);
    }

    public override void OnDeselected()
    {
        base.OnDeselected();
        //FindObjectOfType<HUDManager>().HideAllVillagerWidgets();
    }
}
