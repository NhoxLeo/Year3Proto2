using UnityEngine;

public class LightningTower : ParticleDefenseStructure
{
    [Header("Lightning Tower")]
    [SerializeField] private float damage = 1.2f;
    [SerializeField] private int projectileAmount = 3;

    public override void OnParticleHit(Transform _target)
    {
        Enemy enemy = _target.GetComponent<Enemy>();
        if(enemy)
        {
            enemy.Damage(damage);
        }
    }
}
