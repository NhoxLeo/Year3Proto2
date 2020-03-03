using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    //private GameManager game;
    private string foodText;
    private string woodText;
    private string metalText;

    public int foodTotal = 250;
    public int foodDelta = 5;

    public int woodTotal = 830;
    public int woodDelta = 15;

    public int metalTotal = 128;
    public int metalDelta = 4;

    void Start()
    {
        foodText = transform.Find("ResourceBar/Food/FoodText").GetComponent<TMP_Text>().text;
        woodText = transform.Find("ResourceBar/Wood/WoodText").GetComponent<TMP_Text>().text;
        metalText = transform.Find("ResourceBar/Metal/MetalText").GetComponent<TMP_Text>().text;
    }


    void Update()
    {
        foodText = foodTotal.ToString();


    }
}
