using UnityEngine;

public class LightningTower : ProjectileDefenseStructure
{
    [SerializeField] private int projectileAmount = 3;

    public override void CheckResearch()
    {
        throw new System.NotImplementedException();
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
