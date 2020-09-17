using UnityEngine;

public class ShockWaveTower : ParticleDefenseStructure
{
    protected override void Start()
    {
        base.Start();
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

    public override void OnParticleHit(Transform _target)
    {
        //Possible shockwave damage and stun.
        //Maybe a way to repulse them backwards...

        Rigidbody body = _target.GetComponent<Rigidbody>();
        if(body)
        {
            body.AddForce(-transform.forward * 10.0f);
        }
    }
}
