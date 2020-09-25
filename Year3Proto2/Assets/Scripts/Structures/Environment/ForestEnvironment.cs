using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestEnvironment : EnvironmentStructure
{
    private bool exploitedState = false;
    private MeshRenderer meshRenderer;
    private bool playable = false;

    protected override void Awake()
    {
        base.Awake();
        environmentType = EnvironmentType.forest;
        structureName = StructureNames.LumberEnvironment;
        meshRenderer = GetComponent<MeshRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        playable = attachedTile.GetPlayable();
        /*
        if (!playable)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            meshRenderer.enabled = false;
        }
        */
    }

    protected override void Update()
    {
        base.Update();
        if (playable)
            {
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

    public override float GetBaseMaxHealth()
    {
        return 100f;
    }

    public override float GetTrueMaxHealth()
    {
        return GetBaseMaxHealth();
    }
}
