using System.Collections.Generic;
using UnityEngine;

public abstract class DefenseStructure : Structure
{
    [Header("Alert")]
    [SerializeField] private Transform alertPrefab;
    private Alert alert;

    protected ResourceBundle attackCost;
    protected List<Transform> enemies = new List<Transform>();
    protected List<string> targetableEnemies = new List<string>();

    private Transform attackingRange;
    private Transform spottingRange = null;
    protected int level = 1;

    protected override void Awake()
    {
        base.Awake();
        structureType = StructureType.Defense;
        attackingRange = transform.Find("Range");
        spottingRange = transform.Find("SpottingRange");
    }

    protected override void Start()
    {
        base.Start();
        if (isPlaced)
        {
            TowerRange range = GetComponentInChildren<TowerRange>();
            CapsuleCollider capsule = range.GetComponent<CapsuleCollider>();
            SphereCollider sphere = range.GetComponent<SphereCollider>();
            if (capsule)
            {
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
                    if (distanceFromEnemy <= capsule.radius * capsule.transform.localScale.x)
                    {
                        if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
                    }
                }
            }
            else
            {
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
                    if (distanceFromEnemy <= sphere.radius * sphere.transform.localScale.x)
                    {
                        if (!enemies.Contains(enemy.transform)) { enemies.Add(enemy.transform); }
                    }
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        enemies.RemoveAll(enemy => !enemy);
        if (alert)
        {
            if (allocatedVillagers >= 1 || enemies.Count <= 0)
            {
                Destroy(alert.gameObject);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (alert)
        {
            Destroy(alert.gameObject);
        }
    }

    public void Alert()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas)
        {
            Alert newAlert = Instantiate(alertPrefab, canvas.transform).GetComponent<Alert>();
            if (newAlert)
            {
                Vector3 position = transform.localPosition;
                position.y = 1.0f;
                newAlert.SetTarget(position);
                alert = newAlert;
            }
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (alert)
        {
            Destroy(alert.gameObject);
        }
    }

    public override void ShowRangeDisplay(bool _active)
    {
        base.ShowRangeDisplay(_active);
        spottingRange.GetChild(0).gameObject.SetActive(_active);
        attackingRange.GetChild(0).gameObject.SetActive(_active);
    }

    public List<Transform> GetEnemies()
    {
        enemies.RemoveAll(enemy => !enemy);
        return enemies;
    }

    public List<string> GetTargetableEnemies()
    {
        return targetableEnemies;
    }

    public void SetLevel(int _level)
    {
        level = _level;
        OnSetLevel();
    }

    public void LevelUp()
    {
        SetLevel(level + 1);
    }

    public int GetLevel()
    {
        return level;
    }

    public Alert GetAlert()
    {
        return alert;
    }

    protected virtual void OnSetLevel()
    {

    }
}
