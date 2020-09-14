using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Catapult : ProjectileDefenseStructure
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

    public override void Launch(Transform _target)
    {
        Vector3 position = transform.position;
        position.y = 1.5f;

        Transform projectile = Instantiate(projectilePrefab, position, Quaternion.identity, transform);
        Boulder boulder = projectile.GetComponent<Boulder>();
        if (boulder)
        {
            float explosionFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultSuper) ? 1.5f : 1.0f;
            float damageFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.CatapultPower) ? 1.3f : 1.0f;

            boulder.ExplosionRadius = boulder.ExplosionRadius * explosionFactor;
            boulder.SetDamage(boulder.GetDamage() * damageFactor);
            boulder.SetTarget(_target);
        }
    }

    public override void CheckLevel()
    {
        switch (level)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
        }
    }
}
