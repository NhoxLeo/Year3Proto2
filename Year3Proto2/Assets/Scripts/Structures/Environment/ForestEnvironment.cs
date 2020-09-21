using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestEnvironment : EnvironmentStructure
{
    private bool exploitedState = false;
    private MeshRenderer meshRenderer;

    protected override void Awake()
    {
        base.Awake();
        environmentType = EnvironmentType.forest;
        structureName = StructureNames.LumberEnvironment;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected override void Update()
    {
        base.Update();
        if (exploited && !exploitedState)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            meshRenderer.enabled = false;
            exploitedState = true;
        }
        if (!exploited && exploitedState)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            meshRenderer.enabled = true;
            exploitedState = false;
        }
    }
}
