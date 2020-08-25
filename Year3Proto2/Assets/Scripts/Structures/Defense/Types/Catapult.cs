using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Catapult : DefenseStructure
{
    protected override void Start()
    {
        base.Start();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Catapult];
    }

    public override void CheckResearch()
    {
        attackCost = new ResourceBundle(0, SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultEfficiency) ? 4 : 8, 0);

        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
    }

    public override bool Launch()
    {
        Transform projectile = Instantiate(projectilePrefab, transform);
        Boulder boulder = projectile.GetComponent<Boulder>();
        if(boulder)
        {
            float explosionFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultSuper) ? 1.5f : 1.0f;
            float damageFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultPower) ? 1.3f : 1.0f;

            boulder.SetExplosionRadius(boulder.GetExplosionRadius() * explosionFactor);
            boulder.SetDamage(boulder.GetDamage() * damageFactor);
            boulder.SetTarget(enemy);
            boulder.Launch();
            return true;
        }
        return false;
    }
}
