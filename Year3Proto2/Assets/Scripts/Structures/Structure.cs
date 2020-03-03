using UnityEngine;
public enum StructureType
{
    RESOURCE,
    ENVIRONMENT,
    ATTACK,
    DEFENSE
};

public abstract class Structure : MonoBehaviour
{
    public Sprite icon;
    public string displayName;

    private StructureType structureType;
    public Structure(StructureType structureType)
    {
        this.structureType = structureType;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetType() == typeof(Structure)) Check(other.gameObject);
    }

    public abstract void Check(GameObject gameobject);

    public StructureType GetStructureType()
    {
        return structureType;
    }
}

