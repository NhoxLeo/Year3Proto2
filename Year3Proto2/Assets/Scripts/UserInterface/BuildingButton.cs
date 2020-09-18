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

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void SetInfo(BuildPanel.Buildings _buildings)
    {
        buildings = _buildings;
        //buildingName = StructureManager.StructureNames[buildings];
        description = StructureManager.StructureDescriptions[buildings];

        FindIcon(buildingName);
    }

    public void SetSelectedBuilding()
    {
        buildPanel.SelectBuilding((int)buildings);
    }


    public void SetHoveredBuilding()
    {
        if (buildPanel == null) { return; }

        //buildPanel.tooltipSelected = buildings;
        if (buildings != BuildPanel.Buildings.None)
        {
            buildPanel.hoveredButton = this;
        }
        else
        {
            buildPanel.hoveredButton = null;
        }

        buildPanel.SetTooltip((int)buildings);
    }

    public void SetHoverNone()
    {
        buildPanel.SetTooltip(0);
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