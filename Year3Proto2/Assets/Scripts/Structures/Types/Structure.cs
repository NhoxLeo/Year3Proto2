using UnityEngine;
public enum StructureType
{
    resource,
    environment,
    attack,
    storage,
    defense,
    longhaus
};

public abstract class Structure : MonoBehaviour
{
    public Sprite icon;
    public float sitHeight;
    public string displayName;
    public GameObject attachedTile;

    protected StructureType structureType;

    protected void StructureStart()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1 << LayerMask.NameToLayer("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(gameObject, true);
        }
    }

    public StructureType GetStructureType()
    {
        return structureType;
    }

    public virtual bool IsStructure(string _structureName)
    {
        return false;
    }
}

