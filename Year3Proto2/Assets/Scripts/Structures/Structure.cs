using UnityEngine;
public enum StructureType
{
    RESOURCE,
    ENVIRONMENT,
    ATTACK,
    STORAGE,
    DEFENSE
};

public abstract class Structure : MonoBehaviour
{
    public Sprite icon;
    public string displayName;
    public GameObject attachedTile;

    private StructureType structureType;
    public Structure(StructureType structureType)
    {
        this.structureType = structureType;
    }

    protected void StructureStart()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1<< LayerMask.NameToLayer("Ground")))
        {
            attachedTile = hit.transform.gameObject;
        }
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

