using UnityEngine;

public class InvaderUnit : Unit
{
    protected override void Start()
    {
        base.Start();

        unitType = UnitType.ENEMY;
        unitTarget = UnitTarget.STRUCTURE;

        structureTypes =  new StructureType[] 
        {
            StructureType.attack,
            StructureType.resource,
            StructureType.storage,
            StructureType.longhaus
        };
    }

    public override void SetScale(float _scale)
    {
        unitProperties.scale = _scale;
        transform.localScale *= _scale;
        unitProperties.damage = _scale * 2.0f;
        unitProperties.health = _scale * 7.5f;
        unitProperties.speed = 0.4f + (1f / _scale) / 10.0f;
        if (!animator) { animator = GetComponent<Animator>(); }
        animator.SetFloat("AttackSpeed", 1f / _scale);
    }
}
