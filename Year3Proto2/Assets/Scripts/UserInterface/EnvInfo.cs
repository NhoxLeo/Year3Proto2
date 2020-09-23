using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnvInfo : MonoBehaviour
{
    private bool showInfo;

    private RectTransform rTrans;
    private Tooltip tool;
    private TMP_Text textbox;

    void Start()
    {
        rTrans = GetComponent<RectTransform>();
        DefineTool();
        DefineTextBox();
    }

    void DefineTool()
    {
        tool = GetComponent<Tooltip>();
        tool.SetInteractable(false);
    }

    void DefineTextBox()
    {
        textbox = transform.Find("PanelMask/Description").GetComponent<TMP_Text>();
    }

    void Update()
    {
        tool.showTooltip = showInfo;
        /*
        if (Input.GetKeyDown(KeyCode.P))
        {
            ShowInfo("this is a test tooltip");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            showInfo = !showInfo;
        }
        */
    }

    private void LateUpdate()
    {
        transform.position = Input.mousePosition;

        // Adjust position of info panel if near edge of bounds

        float xPivot = 0.0f;
        float yPivot = 0.5f;
        if (transform.localPosition.y > 400.0f)
        {
            yPivot = 1.0f;
        }
        else if (transform.localPosition.y < -220.0f)
        {
            yPivot = 0.0f;
        }

        if (transform.localPosition.x > 640.0f)
        {
            xPivot = 1.0f;
        }
        // Smooth Lerping motion
        float dt = Time.unscaledDeltaTime;
        Vector2 pivot = new Vector2(Mathf.Lerp(rTrans.pivot.x, xPivot, dt * 10.0f), Mathf.Lerp(rTrans.pivot.y, yPivot, dt * 10.0f));
        rTrans.pivot = pivot;
    }

    public void SetInfoByStructure(Structure _structure)
    {
        SetVisibility(true);
        switch (_structure.GetStructureName())
        {
            case StructureNames.Longhaus:
                ShowInfo("The Longhaus is your base of operations, protect it at all costs! The Longhaus generates a small amount of wood & food and an even smaller amount of metal.");
                break;
            case StructureNames.LumberEnvironment:
                ShowInfo("Placing a Lumber Mill (LM) on this tile will destroy the forest, and provide a bonus to the LM. Placing a LM adjacent to this tile with provide a bonus to the LM.");
                break;
            case StructureNames.MetalEnvironment:
                ShowInfo("Placing a Mine on this tile will destroy the hill, and provide a bonus to the Mine. Placing a Mine adjacent to this tile with provide a bonus to the Mine.");
                break;
            case StructureNames.FoodEnvironment:
                ShowInfo("Placing a Farm on this tile will destroy the field, and provide a bonus to the Farm. Placing a Farm adjacent to this tile with provide a bonus to the Farm.");
                break;
            case StructureNames.FoodResource:
                ShowInfo("The Farm generates Food. It gains a bonus from all plains tiles surrounding it, and an additional bonus if placed on a plains tile.");
                break;
            case StructureNames.LumberResource:
                ShowInfo("The Lumber Mill generates Wood. It gains a bonus from all forest tiles surrounding it, and an additional bonus if placed on a forest tile.");
                break;
            case StructureNames.MetalResource:
                ShowInfo("The Mine generates Metal. It gains a bonus from all hill tiles surrounding it, and an additional bonus if placed on a hill tile.");
                break;
            case StructureNames.FoodStorage:
                ShowInfo("The Granary stores Food. If it is broken, you will lose the additional capacity it gives you, and any excess Food you have will be lost.");
                break;
            case StructureNames.LumberStorage:
                ShowInfo("The Lumber Pile stores Wood. If it is broken, you will lose the additional capacity it gives you, and any excess Wood you have will be lost.");
                break;
            case StructureNames.MetalStorage:
                ShowInfo("The Metal Storehouse stores Metal. If it is broken, you will lose the additional capacity it gives you, and any excess Metal you have will be lost.");
                break;
            case StructureNames.Ballista:
                ShowInfo("The Ballista Tower fires bolts at enemy units.");
                break;
            case StructureNames.Catapult:
                ShowInfo("The Catapult fires explosive fireballs at enemy units.");
                break;
            case StructureNames.Barracks:
                ShowInfo("The Barracks spawns soldiers which attack enemy units automatically.");
                break;
        }
    }

    public void ShowInfo(string info)
    {
        if (!textbox)
        {
            DefineTextBox();
        }
        textbox.text = info;
        if (!tool)
        {
            DefineTool();
        }
        tool.SetHeight(tool.height);
    }

    public void SetVisibility(bool isVisible)
    {
        showInfo = isVisible;
    }

}
