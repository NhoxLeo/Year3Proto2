using UnityEngine;

public class BallistaTower : AttackStructure
{
    public const int CostArrowBase = 4;
    public GameObject arrow;
    public GameObject ballista;
    public static bool arrowPierce;
    private float arrowDamage = 10f;
    private float arrowSpeed = 12.5f;
    private float fireRate = 0f;
    private float fireDelay = 0f;
    private float fireCooldown = 0f;

    public float GetFirerate()
    {
        return fireRate;
    }

    protected override void Awake()
    {
        base.Awake();
        structureName = StructureNames.Ballista;
    }

    protected override void Start()
    {
        base.Start();
        SetFirerate();
        SuperManager superMan = SuperManager.GetInstance();
        if (superMan.GetResearchComplete(SuperManager.BallistaRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
        }
        if (superMan.GetResearchComplete(SuperManager.BallistaRange))
        {
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
        bool efficiencyUpgrade = superMan.GetResearchComplete(SuperManager.BallistaEfficiency);
        int woodCost = efficiencyUpgrade ? (CostArrowBase / 2) : CostArrowBase;
        attackCost = new ResourceBundle(0, woodCost, 0);
        arrowPierce = superMan.GetResearchComplete(SuperManager.BallistaSuper);
        if (superMan.GetResearchComplete(SuperManager.BallistaPower))
        {
            arrowDamage *= 1.3f;
        }
    }

    protected override void Update()
    {
        base.Update();
        SetFirerate();
        if (target && isPlaced)
        {
            Vector3 ballistaPosition = ballista.transform.position;
            Vector3 targetPosition = target.transform.position;

            Vector3 difference = ballistaPosition - targetPosition;
            difference.y = 0;

            Quaternion rotation = Quaternion.LookRotation(difference);
            ballista.transform.rotation = Quaternion.Slerp(ballista.transform.rotation, rotation * Quaternion.AngleAxis(90, Vector3.up), Time.deltaTime * 2.5f);

        }
    }

    public override void Attack(GameObject target)
    {
        fireCooldown += Time.deltaTime;
        if (fireCooldown >= fireDelay)
        {
            if (GameManager.GetInstance().playerResources.AttemptPurchase(attackCost))
            {
                Fire();
            }
        }
    }

    void Fire()
    {
        fireCooldown = 0;
        GameObject newArrow = Instantiate(arrow, ballista.transform.position, Quaternion.identity, transform);
        BoltBehaviour arrowBehaviour = newArrow.GetComponent<BoltBehaviour>();
        arrowBehaviour.Initialize(target.transform, arrowDamage, arrowSpeed, arrowPierce);
        GameManager.CreateAudioEffect("arrow", transform.position, 0.6f);
    }

    void SetFirerate()
    {
        switch (allocatedVillagers)
        {
            case 0:
                fireRate = 0f;
                break;
            case 1:
                fireRate = 0.5f;
                break;
            case 2:
                fireRate = 1f;
                break;
            case 3:
                fireRate = 2f;
                break;
        }
        fireDelay = 1f / fireRate;
    }

    public override Vector3 GetResourceDelta()
    {
        Vector3 resourceDelta = base.GetResourceDelta();
        if (target)
        {
            resourceDelta -= attackCost * fireRate;
        }
        return resourceDelta;
    }

    public override float GetBaseMaxHealth()
    {
        return 350f;
    }

    public override float GetTrueMaxHealth()
    {
        return GetBaseMaxHealth();
    }
}
