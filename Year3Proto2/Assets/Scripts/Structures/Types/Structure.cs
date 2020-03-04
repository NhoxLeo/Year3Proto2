using UnityEngine;
public enum StructureType
{
    resource,
    environment,
    attack,
    storage,
    defense
};

public abstract class Structure : MonoBehaviour
{
    public Sprite icon;
    public string displayName;
    public GameObject attachedTile;

    protected StructureType structureType;

    protected void StructureStart()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1<< LayerMask.NameToLayer("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(gameObject, true);
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

