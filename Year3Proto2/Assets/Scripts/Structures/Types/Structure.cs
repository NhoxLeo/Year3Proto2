﻿using UnityEngine;
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
    public string structureName;
    public Sprite icon;
    public float sitHeight;
    public string displayName;
    public GameObject attachedTile;

    protected float health = 100.0f;

    protected StructureType structureType;

    protected void StructureStart()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.6f, 1 << LayerMask.NameToLayer("Ground")))
        {
            hit.transform.gameObject.GetComponent<TileBehaviour>().Attach(gameObject, true);
        }
    }

    protected void StructureUpdate()
    {
        if(health <= 0.0f)
        {
            attachedTile.GetComponent<TileBehaviour>().Detach(true);
            Destroy(gameObject);
        }
    }

    public StructureType GetStructureType()
    {
        return structureType;
    }

    public bool IsStructure(string _structureName)
    {
        return _structureName == structureName;
    }

    public string GetStructureName()
    {
        return structureName;
    }

    public void Damage(float amount)
    {
        health -= amount;
    }

    public float GetHealth()
    {
        return health;
    }
}

