using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    private GameManager game;
    private TMP_Text foodText;
    private TMP_Text woodText;
    private TMP_Text metalText;

    public int foodTotal = 12;
    public int foodDelta = 1;

    public int woodTotal = 12;
    public int woodDelta = 1;

    public int metalTotal = 5;
    public int metalDelta = 1;

    void Start()
    {
        game = FindObjectOfType<GameManager>();
        foodText = transform.Find("ResourceBar/Food/FoodText").GetComponent<TMP_Text>();
        woodText = transform.Find("ResourceBar/Wood/WoodText").GetComponent<TMP_Text>();
        metalText = transform.Find("ResourceBar/Metal/MetalText").GetComponent<TMP_Text>();
    }


    void Update()
    {
        string foodSign = (Mathf.Sign(foodDelta) == 1) ? "+" : "";
        foodText.text = game.playerData.GetResource(ResourceType.food).ToString() + " (" + foodSign + foodDelta.ToString() + ")";

        float woodVel = game.GetWoodVelocity(1);
        string woodSign = (Mathf.Sign(woodVel) == 1) ? "+" : "";
        float woodVelDP = Mathf.Round(woodVel * 10f) * .1f;
        woodText.text = game.playerData.GetResource(ResourceType.wood).ToString() + " (" + woodSign + woodVelDP.ToString() + ")";

        string metalSign = (Mathf.Sign(metalDelta) == 1) ? "+" : "";
        metalText.text = game.playerData.GetResource(ResourceType.metal).ToString() + " (" + metalSign + metalDelta.ToString() + ")";


    }
}
