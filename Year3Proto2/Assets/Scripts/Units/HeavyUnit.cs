using UnityEngine;

public class HeavyUnit : Unit
{
    private bool[] equipment = new bool[4];

    protected override void Start()
    {
        base.Start();
        Randomize();
        UpdateEquipment();

        unitType = UnitType.ENEMY;
        unitTarget = UnitTarget.STRUCTURE;

        structureTypes = new StructureType[] {
            StructureType.attack,
            StructureType.resource,
            StructureType.storage,
            StructureType.longhaus
        };
    }

    public bool[] GetEquipment()
    {
        return equipment;
    }

    public void SetEquipment(bool[] _equipment)
    {
        equipment = new bool[4]
        {
            _equipment[0],
            _equipment[1],
            _equipment[2],
            _equipment[3]
        };
    }

    private void Randomize()
    {
        equipment[0] = Random.Range(0f, 1f) > 0.5f;
        equipment[1] = Random.Range(0f, 1f) > 0.5f;
        equipment[2] = Random.Range(0f, 1f) > 0.5f;
        equipment[3] = Random.Range(0f, 1f) > 0.5f;
    }

    private void UpdateEquipment()
    {
        Transform lowPoly = transform.GetChild(1);
        if (equipment[0]) // if sword
        {
            unitProperties.damage = 10f;
            animator.SetFloat("AttackSpeed", 1.2f);
            // disable axe
            lowPoly.GetChild(1).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        else // !sword means axe
        {
            unitProperties.damage = 12f;
            animator.SetFloat("AttackSpeed", 1.0f);
            // disable sword
            lowPoly.GetChild(2).GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        lowPoly.GetChild(0).GetComponent<SkinnedMeshRenderer>().enabled = equipment[1];
        lowPoly.GetChild(3).GetComponent<SkinnedMeshRenderer>().enabled = equipment[2];
        lowPoly.GetChild(4).GetComponent<SkinnedMeshRenderer>().enabled = equipment[3];
        unitProperties.health = 65f;
        unitProperties.speed = 0.25f;

        if (equipment[2]) { unitProperties.health += 10f; unitProperties.speed -= 0.03f; }
        if (equipment[3]) { unitProperties.health += 5f; unitProperties.speed -= 0.015f; }
    }
}
