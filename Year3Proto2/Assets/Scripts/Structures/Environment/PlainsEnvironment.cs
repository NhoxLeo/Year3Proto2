using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlainsEnvironment : EnvironmentStructure
{
    protected override void Awake()
    {
        base.Awake();
        resourceType = ResourceType.Food;
        structureName = StructureNames.FoodEnvironment;
        bonusHighlightSitHeight = SuperManager.GetInstance().GetSnow() ? 0.45f : 0.3f;
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
        Color colour = meshRenderer.material.color;
        colour.a = _opacity;
        meshRenderer.material.color = colour;
    }

    public override void SetColour(Color _colour)
    {
        throw new System.NotImplementedException();
    }
}
