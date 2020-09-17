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
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        environmentType = EnvironmentType.forest;
        structureName = StructureNames.LumberEnvironment;
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
