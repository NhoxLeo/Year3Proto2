using UnityEngine;

public class FreezeTower : DefenseStructure
{
    private FreezeTowerCannon[] cannons;
    private float freezeEffect;
    private const float BaseMaxHealth = 300f;
    private Color normalEmissiveColour;

    protected override void Awake()
    {
        // set base stats
        base.Awake();
        structureName = StructureNames.FreezeTower;

        // research
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }

        // set targets
        targetableEnemies.Add(EnemyNames.Invader);
        targetableEnemies.Add(EnemyNames.HeavyInvader);
        targetableEnemies.Add(EnemyNames.Petard);
        targetableEnemies.Add(EnemyNames.BatteringRam);
        normalEmissiveColour = meshRenderer.materials[2].GetColor("_EmissiveColor");
    }

    protected override void Start()
    {
        base.Start();
        SuperManager superManager = SuperManager.GetInstance();
        cannons = GetComponentsInChildren<FreezeTowerCannon>();
        freezeEffect = superManager.GetResearchComplete(SuperManager.FreezeTowerSlowEffect) ? 1.0f : 1.3f;
        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.Setup(
                freezeEffect,
                superManager.GetResearchComplete(SuperManager.FreezeTowerSuper)
            );
        }
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();

        foreach (FreezeTowerCannon cannon in cannons)
        {
            cannon.GetTargets().ForEach(target => {
                if (target)
                {
                    Enemy enemy = target.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemy.Slow(false, 0.0f);
                    }
                }
            });
        }
    }

    public float GetFreezeEffect()
    {
        return 1f - (0.6f * (1f / freezeEffect));
    }

    protected override void OnSetLevel()
    {
        base.OnSetLevel();
        float oldMaxHealth = GetTrueMaxHealth() / SuperManager.ScalingFactor;
        float difference = GetTrueMaxHealth() - oldMaxHealth;
        health += difference;
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // fortification upgrade
        if (SuperManager.GetInstance().GetResearchComplete(SuperManager.ShockwaveTowerFortification))
        {
            maxHealth *= 1.5f;
        }

        // level
        maxHealth *= Mathf.Pow(SuperManager.ScalingFactor, level - 1);

        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    public override void SetColour(Color _colour)
    {
        meshRenderer.materials[0].SetColor("_BaseColor", _colour);
        meshRenderer.materials[1].SetColor("_BaseColor", _colour);
        meshRenderer.materials[2].SetColor("_BaseColor", _colour);
        meshRenderer.materials[2].SetColor("_EmissiveColor", _colour);
        meshRenderer.materials[3].SetColor("_BaseColor", _colour);
        meshRenderer.materials[3].SetColor("_EmissiveColor", _colour);
        if (_colour == Color.white)
        {
            meshRenderer.materials[2].SetColor("_EmissiveColor", normalEmissiveColour);
            meshRenderer.materials[3].SetColor("_EmissiveColor", normalEmissiveColour);
        }
    }
}