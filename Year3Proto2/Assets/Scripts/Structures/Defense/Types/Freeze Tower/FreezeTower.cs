﻿public class FreezeTower : DefenseStructure
{
    protected override void Start()
    {
        base.Start();
        structureName = StructureNames.FreezeTower;

        attackCost = new ResourceBundle(0, 4, 0);
        //attackCost = new ResourceBundle(0, SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerEfficiency) ? 4 : 8, 0);

        // Freeze Range
        /*if (SuperManager.GetInstance().GetResearchComplete(SuperManager.FreezeTowerRange))
        {
            GetComponentInChildren<TowerRange>().transform.localScale *= 1.25f;
            GetComponentInChildren<SpottingRange>().transform.localScale *= 1.25f;
        }
        */
    }
}