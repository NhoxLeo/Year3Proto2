using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillEnvironment : EnvironmentStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Metal;
        structureName = StructureNames.MetalEnvironment;
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
        Color colour = meshRenderer.material.GetColor("_BaseColor");
        colour.a = _opacity;
        meshRenderer.materials[0].SetColor("_BaseColor", colour);
        meshRenderer.materials[1].SetColor("_BaseColor", colour);
    }
}
