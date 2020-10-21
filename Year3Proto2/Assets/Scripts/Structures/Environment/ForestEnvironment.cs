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
        bonusHighlightSitHeight = SuperManager.GetInstance().GetSnow() ? 0.6f : 0.4f;
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
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }

    protected override void Update()
    {
        base.Update();
        if (playable)
        {
            if (exploited && !exploitedState)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(1).gameObject.SetActive(false);
                meshRenderer.enabled = false;
                exploitedState = true;
            }
            if (!exploited && exploitedState)
            {
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(true);
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

    public override void SetColour(Color _colour)
    {
        throw new System.NotImplementedException();
    }

    public override void SetMaterials(bool _snow)
    {
        base.SetMaterials(_snow);
        transform.GetChild(0).GetComponent<MeshRenderer>().materials = StructureMaterials.Fetch(structureName + StructureNames.Alt, _snow).ToArray();
    }


}
