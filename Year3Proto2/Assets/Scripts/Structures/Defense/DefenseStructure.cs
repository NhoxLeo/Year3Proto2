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
            CapsuleCollider capsule = GetComponentInChildren<TowerRange>().GetComponent<CapsuleCollider>();
            SphereCollider sphere = GetComponentInChildren<TowerRange>().GetComponent<SphereCollider>();
            if (capsule)
            {
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    float distanceFromEnemy = (enemy.transform.position - transform.position).magnitude;
                    if (distanceFromEnemy <= capsule.radius)
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
                    if (distanceFromEnemy <= sphere.radius)
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
    }

    private void OnDestroy()
    {
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
            Transform transform = Instantiate(alertPrefab, canvas.transform);
            Alert alert = transform.GetComponent<Alert>();
            if (alert)
            {
                Vector3 position = this.transform.localPosition;
                position.y = 1.0f;
                alert.SetTarget(position);
                this.alert = alert;
            }
        }
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if (alert) Destroy(alert.gameObject);
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
