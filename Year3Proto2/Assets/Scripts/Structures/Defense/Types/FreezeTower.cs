using UnityEngine;
public class FreezeTower : ProjectileDefenseStructure
{
    private const int CostIcicleBase = 3;

    protected override void Start()
    {
        base.Start();
        structureName = StructureManager.StructureNames[BuildPanel.Buildings.Catapult];
    }

    public override void CheckResearch()
    {
        attackCost = new ResourceBundle(0, SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerEfficiency) ? 4 : 8, 0);

        // Freeze Range
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
    }

    public override void Launch(Transform _target)
    {
        Transform projectile = Instantiate(projectilePrefab, transform);
        Icicle icicle = projectile.GetComponent<Icicle>();
        if (icicle)
        {
            float durationFactor = SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerStunDuration) ? 1.6f : 1.0f;

            // Duration of stun 
            icicle.SetStunDuration(durationFactor);
            icicle.SetTarget(_target);
        }
    }
}