using UnityEngine;

public class TowerRange : MonoBehaviour
{
    private DefenseStructure defenseParent;
    private int enemyStructureColliderLayer;

    private void Start()
    {
        GetParent();
        enemyStructureColliderLayer = LayerMask.NameToLayer("EnemyStructureCollider");
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
                    if (!defenseParent) { GetParent(); }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.enemyName))
                    {
                        if (!defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Add(other.transform.parent);
                            if (defenseParent.isPlaced)
                            {
                                if (!defenseParent.GetAlert() && defenseParent.GetAllocated() <= 0)
                                {
                                    defenseParent.Alert();
                                    MessageBox messageBox = FindObjectOfType<MessageBox>();
                                    messageBox.ShowMessage(defenseParent.GetStructureName() + " has no allocated villagers.", 3.0f);
                                }
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
                    if (!defenseParent) { GetParent(); }
                    if (defenseParent.GetTargetableEnemies().Contains(enemy.enemyName))
                    {
                        if (defenseParent.GetEnemies().Contains(other.transform.parent))
                        {
                            defenseParent.GetEnemies().Remove(other.transform.parent);
                            if(defenseParent.GetEnemies().Count < 0)
                            {
                                Destroy(defenseParent.GetAlert().gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
