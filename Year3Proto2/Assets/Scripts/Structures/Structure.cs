using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum StructureType
{
    WOODCUTTING,
    MINING,
    ARCHER_TOWER,
    CATAPULT_TOWER
};

public class Structure : MonoBehaviour
{
    public StructureType structureType;
    public float productionTime = 10.0f;
    private float remainingTime;

    public TMP_Text structureName;
    public TMP_Text remaining;
    public Slider slider;

    private void Start()
    {
        structureName.text = structureType.ToString();
        remainingTime = productionTime;

        slider.maxValue = productionTime;
    }

    private void Update()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0.0f) remainingTime = productionTime;

        slider.value = remainingTime;
        remaining.text = ((int)remainingTime).ToString() + " Seconds Until Production";
    }
}
