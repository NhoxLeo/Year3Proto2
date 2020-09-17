using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public BuildPanel.Buildings buildings;
    public string buildingName;
    public string description;
    public int woodCost;
    public int foodCost;
    public bool locked;
    public bool canAfford;

    [SerializeField] private BuildPanel buildPanel;
    private Image thumb;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void SetInfo(BuildPanel.Buildings _buildings)
    {
        buildings = _buildings;
        buildingName = StructureManager.StructureNames[buildings];
        description = StructureManager.StructureDescriptions[buildings];

        FindIcon(buildingName);
    }

    public void SetSelectedBuilding()
    {

    }

    public void SetHoveredBuilding()
    {

    }

    private Sprite FindIcon(string _name)
    {
        Sprite buildingSprite = null;

        if (_name != null)
        {
            string formattedName = _name.Replace(" ", "");
            formattedName = "uiBuilding" + formattedName;
            buildingSprite = Resources.Load<Sprite>("BuildingThumbs/" + formattedName);
        }

        if (buildingSprite == null)
        {
            buildingSprite = Resources.Load<Sprite>("BuildingThumbs/uiBuildingPlaceholder");
        }

        return buildingSprite;
    }
}
