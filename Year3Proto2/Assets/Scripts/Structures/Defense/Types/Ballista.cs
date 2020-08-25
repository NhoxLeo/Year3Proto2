using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ballista : DefenseStructure
{
    private const int CostArrowBase = 4;

    protected override void Start()
    {
        base.Start();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Ballista];
    }


    public override void CheckResearch()
    {
        if (superMan.GetResearchComplete(SuperManager.BallistaRange)) { GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f; }
        if (superMan.GetResearchComplete(SuperManager.BallistaRange)) { GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f; }
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.BallistaFortification)) { health = maxHealth *= 1.5f; }

        bool efficiencyUpgrade = superMan.GetResearchComplete(SuperManager.BallistaEfficiency);
        int woodCost = efficiencyUpgrade ? (CostArrowBase / 2) : CostArrowBase;
        attackCost = new ResourceBundle(woodCost, 0, 0);
    }

    public override bool Launch()
    {
        Vector3 position = transform.position;
        position.y = 1.0f;

        Transform projectile = Instantiate(projectilePrefab, position, Quaternion.identity, transform);
        Arrow arrow = projectile.GetComponent<Arrow>();
        if(arrow)
        {
            float damageFactor = superMan.GetResearchComplete(SuperManager.BallistaPower) ? 1.3f : 1.0f;

            arrow.SetPierce(superMan.GetResearchComplete(SuperManager.BallistaSuper));
            arrow.SetDamage(arrow.GetDamage() * damageFactor);
            arrow.SetTarget(enemy);
            arrow.Launch();

            return true;
        }
        return false;
    }

    public override void SetFoodAllocationGlobal(int _allocation)
    {
        // TODO?
        throw new System.NotImplementedException();
    }
}
