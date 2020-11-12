using UnityEngine;

public class TowerRange : MonoBehaviour
{
    private DefenseStructure defenseParent = null;
    private Barracks barracksParent = null;
    private int enemyStructureColliderLayer;

    private void Start()
    {
        GetParent();
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
        if (defenseParent.GetStructureName() == StructureNames.Barracks)
        {
            barracksParent = defenseParent.GetComponent<Barracks>();
        }
    }

    private void GetParent()
    {
        defenseParent = transform.parent.GetComponent<DefenseStructure>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                Enemy enemy = other.transform.parent.GetComponent<Enemy>();
                if (enemy)
                {
                    if (!defenseParent) 
                    { 
                        GetParent(); 
                    }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.GetName()))
                    {
                        if (!defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Add(other.transform.parent);
                            if (barracksParent)
                            {
                                barracksParent.AlertSoldiers();
                            }

                            if (!defenseParent.isPlaced) return;
                            if (defenseParent.GetAlert()) return;
                            if (defenseParent.GetAllocated() > 0) return;

                            if (defenseParent.GetStructureName() != StructureNames.FrostTower)
                            {
                                defenseParent.Alert();
                                MessageBox.GetInstance().ShowMessage(defenseParent.GetStructureName() + " has no allocated villagers.", 3.0f);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == enemyStructureColliderLayer)
        {
            if (other.transform.parent)
            {
                Enemy enemy = other.transform.parent.GetComponent<Enemy>();
                if (enemy)
                {
                    if (!defenseParent)
                    { 
                        GetParent(); 
                    }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.GetName()))
                    {
                        if (defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Remove(other.transform.parent);
                            if (defenseParent.GetEnemies().Count == 0)
                            {
                                Alert alert = defenseParent.GetAlert();
                                if (alert)
                                {
                                    Destroy(alert.gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
