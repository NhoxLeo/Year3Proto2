using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    public Color gainColour;
    public Color lossColour;
    public Color fullColour;
    private GameManager game;
    private TMP_Text foodText;
    private TMP_Text woodText;
    private TMP_Text metalText;

    void Start()
    {
        game = FindObjectOfType<GameManager>();
        foodText = transform.Find("ResourceBar/Food/FoodText").GetComponent<TMP_Text>();
        woodText = transform.Find("ResourceBar/Wood/WoodText").GetComponent<TMP_Text>();
        metalText = transform.Find("ResourceBar/Metal/MetalText").GetComponent<TMP_Text>();
    }

    void Update()
    {
        float foodVel = game.GetFoodVelocity(1);
        string foodSign = (Mathf.Sign(foodVel) == 1) ? "+" : "";
        float foodVelDP = Mathf.Round(foodVel * 10f) * .1f;
        foodText.text = game.playerData.GetResource(ResourceType.food).ToString() + " (" + foodSign + foodVelDP.ToString() + "/s)";
        if (Mathf.Sign(foodVel) == 1)
        {
            foodText.color = gainColour;
        }
        else
        {
            foodText.color = lossColour;
        }
        if (game.playerData.ResourceIsFull(ResourceType.food))
        {
            foodText.color = fullColour;
        }

        float woodVel = game.GetWoodVelocity(1);
        string woodSign = (Mathf.Sign(woodVel) == 1) ? "+" : "";
        float woodVelDP = Mathf.Round(woodVel * 10f) * .1f;
        woodText.text = game.playerData.GetResource(ResourceType.wood).ToString() + " (" + woodSign + woodVelDP.ToString() + "/s)";
        if (Mathf.Sign(woodVel) == 1)
        {
            woodText.color = gainColour;
        }
        else
        {
            woodText.color = lossColour;
        }
        if (game.playerData.ResourceIsFull(ResourceType.wood))
        {
            woodText.color = fullColour;
        }

        float metalVel = game.GetMetalVelocity(1);
        string metalSign = (Mathf.Sign(metalVel) == 1) ? "+" : "";
        float metalVelDP = Mathf.Round(metalVel * 10f) * .1f;
        metalText.text = game.playerData.GetResource(ResourceType.metal).ToString() + " (" + metalSign + metalVelDP.ToString() + "/s)";
        if (Mathf.Sign(metalVel) == 1)
        {
            metalText.color = gainColour;
        }
        else
        {
            metalText.color = lossColour;
        }
        if (game.playerData.ResourceIsFull(ResourceType.metal))
        {
            metalText.color = fullColour;
        }
    }
}
