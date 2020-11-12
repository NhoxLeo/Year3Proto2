using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Longhaus : Structure
{
    [SerializeField]
    static public int foodStorage = 500;
    [SerializeField]
    static public int woodStorage = 500;
    [SerializeField]
    static public int metalStorage = 500;

    public static float productionTime = 3f;
    protected float remainingTime = 3f;

    private const float BaseMaxHealth = 500f;

    private const int foodGen = 16;
    private const int lumberGen = 6;
    private const int metalGen = 3;

    private float attackWarningInterval = 10.0f;
    private float attackWarningTimer = 0.0f;


    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Longhaus;
        structureName = StructureNames.Longhaus;
    }

    protected override void Start()
    {
        base.Start();
        SetMaterials(SuperManager.GetInstance().GetSnow());
    }

    // Update is called once per frame
    protected override void Update()
    {
        GameManager gameMan = GameManager.GetInstance();
        base.Update();
        if (health > 0f)
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = productionTime;
            }

        }
        /*
        if (Input.GetKeyDown(KeyCode.N) && StructureManager.GetInstance().StructureIsSelected(this))
        {
            VillagerManager.GetInstance().TrainVillager();
        }
        */

        attackWarningTimer -= Time.deltaTime;
    }

    protected override void ShowAttackWarning()
    {
        base.ShowAttackWarning();
        if (attackWarningTimer <= 0.0f)
        {
            MessageBox.GetInstance().ShowMessage("Your Longhaus is under attack!", 3.0f);
            attackWarningTimer = attackWarningInterval;
        }
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();

        resourceDelta += new Vector3(foodGen / productionTime - VillagerManager.GetInstance().GetFoodConsumptionPerSec(), lumberGen / productionTime, metalGen / productionTime);

        return resourceDelta;
    }

    public override float GetBaseMaxHealth()
    {
        return BaseMaxHealth;
    }

    public override float GetTrueMaxHealth()
    {
        // get base health
        float maxHealth = GetBaseMaxHealth();

        // poor timber multiplier
        maxHealth *= SuperManager.GetInstance().GetPoorTimberFactor();

        return maxHealth;
    }

    protected override void OnDestroyed()
    {
        base.OnDestroyed();
        GlobalData.longhausDead = true;
    }

    public override void SetColour(Color _colour)
    {
        /*
        meshRenderer.materials[0].SetColor("_BaseColor", _colour);
        meshRenderer.materials[1].SetColor("_BaseColor", _colour);
        */
    }

    public static float GetFoodProductionPerSec()
    {
        return foodGen / productionTime;
    }
}
