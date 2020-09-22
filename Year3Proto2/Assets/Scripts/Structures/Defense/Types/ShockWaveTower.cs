using UnityEngine;

public class ShockWaveTower : DefenseStructure
{
    [Header("Shockwave Tower")]
    [SerializeField] private float delay = 4.0f;
    [SerializeField] private float startDelay = 1.2f;
    [SerializeField] private Transform particle;
    private float time = 0.0f;


    protected override void Awake()
    {
        base.Awake();
        structureName = StructureNames.ShockwaveTower;
        attackCost = new ResourceBundle(0, 4, 0);
        maxHealth = 400.0f;
        health = maxHealth;
    }

    protected override void Start()
    {
        base.Start();
        time = startDelay;
    }

    protected override void Update()
    {
        base.Update();
        if(isPlaced && enemies.Count > 0)
        {
            time -= Time.deltaTime;
            if(time <= 0.0f)
            {
                time = delay;

                Instantiate(particle, transform.position, particle.rotation);
                enemies.ForEach(transform => {
                    Enemy enemy = transform.GetComponent<Enemy>();
                    if(enemy)
                    {
                        enemy.Stun();
                    }
                });
            }
        } else
        {
            time = startDelay;
        }
    }
}
