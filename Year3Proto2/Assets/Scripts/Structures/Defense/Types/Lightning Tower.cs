using UnityEngine;

public class LightningTower : ParticleDefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private float damage = 1.2f;
    [SerializeField] private int projectileAmount = 3;

    public override void CheckResearch()
    {
        throw new System.NotImplementedException();
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
        Enemy enemy = _target.GetComponent<Enemy>();
        if(enemy)
        {
            enemy.Damage(damage);
        }
    }
}
