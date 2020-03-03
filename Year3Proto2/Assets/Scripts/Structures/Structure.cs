using UnityEngine;
public enum StructureType
{
    RESOURCE,
    ENVIRONMENT,
    ATTACK,
    DEFENSE
};

public class Structure : MonoBehaviour
{
    public Sprite icon;
    public string displayName;

    private StructureType structureType;

    public Structure(StructureType structureType)
    {
        this.structureType = structureType;
    }

    public StructureType GetStructureType()
    {
        return structureType;
    }
}
