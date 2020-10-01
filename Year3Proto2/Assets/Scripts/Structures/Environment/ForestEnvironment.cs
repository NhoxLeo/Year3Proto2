using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForestEnvironment : EnvironmentStructure
{
    private bool exploitedState = false;
    private bool playable = false;

    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Wood;
        structureName = StructureNames.LumberEnvironment;
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

    public override void SetOpacity(float _opacity)
    {
        if (_opacity == 1f)
        {
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        else
        {
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        Color colour = meshRenderer.materials[1].color;
        colour.a = _opacity;
        meshRenderer.materials[1].color = colour;
    }
}
