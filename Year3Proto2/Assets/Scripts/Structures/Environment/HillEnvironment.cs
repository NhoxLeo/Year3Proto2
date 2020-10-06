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

    protected override void Start()
    {
        base.Start();
        SetMaterials(SuperManager.GetInstance().GetSnow());
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
        string colourReference = "_BaseColor";
        if (snowMatActive)
        {
            colourReference = "_Color";
        }
        Color colour = meshRenderer.material.GetColor(colourReference);
        colour.a = _opacity;
        meshRenderer.materials[0].SetColor(colourReference, colour);
        meshRenderer.materials[1].SetColor(colourReference, colour);
    }

    public override void SetColour(Color _colour)
    {
        throw new System.NotImplementedException();
    }
}
