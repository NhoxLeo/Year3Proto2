using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    private float updateInterval = 0.667f;
    private float updateTimer;

    public Color gainColour;
    public Color lossColour;
    public Color fullColour;
    private GameManager game;
    private StructureManager structMan;
    private TMP_Text foodText;
    private TMP_Text woodText;
    private TMP_Text metalText;

    private float foodDeltaTimer;
    private Tooltip foodDeltaTip;
    private TMP_Text foodDeltaText;

    private float woodDeltaTimer;
    private Tooltip woodDeltaTip;
    private TMP_Text woodDeltaText;

    private float metalDeltaTimer;
    private Tooltip metalDeltaTip;
    private TMP_Text metalDeltaText;

    private EnemySpawner spawner;
    private TMP_Text waveText;

    void Start()
    {
        game = FindObjectOfType<GameManager>();
        structMan = FindObjectOfType<StructureManager>();
        spawner = FindObjectOfType<EnemySpawner>();

        foodText = transform.Find("ResourceBar/FoodText").GetComponent<TMP_Text>();
        woodText = transform.Find("ResourceBar/WoodText").GetComponent<TMP_Text>();
        metalText = transform.Find("ResourceBar/MetalText").GetComponent<TMP_Text>();
        waveText = transform.Find("InfoBar/Waves").GetComponent<TMP_Text>();

        foodDeltaTip = transform.Find("ResourceBar/FoodText/FoodIcon/FoodDelta").GetComponent<Tooltip>();
        foodDeltaText = transform.Find("ResourceBar/FoodText/FoodIcon/FoodDelta/FoodDeltaText").GetComponent<TMP_Text>();

        woodDeltaTip = transform.Find("ResourceBar/WoodText/WoodIcon/WoodDelta").GetComponent<Tooltip>();
        woodDeltaText = transform.Find("ResourceBar/WoodText/WoodIcon/WoodDelta/WoodDeltaText").GetComponent<TMP_Text>();

        metalDeltaTip = transform.Find("ResourceBar/MetalText/MetalIcon/MetalDelta").GetComponent<Tooltip>();
        metalDeltaText = transform.Find("ResourceBar/MetalText/MetalIcon/MetalDelta/MetalDeltaText").GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        updateTimer -= Time.unscaledDeltaTime;
        if (updateTimer <= 0)
        {
            RefreshResources();
            updateTimer = updateInterval;
        }
        

        // Resource Deltas
        // Food
        if (foodDeltaTimer > 0.0f)
        {
            foodDeltaTimer -= Time.unscaledDeltaTime;
            foodDeltaTip.showTooltip = true;
        }
        else
        {
            foodDeltaTip.showTooltip = false;
        }
        // Wood
        if (woodDeltaTimer > 0.0f)
        {
            woodDeltaTimer -= Time.unscaledDeltaTime;
            woodDeltaTip.showTooltip = true;
        }
        else
        {
            woodDeltaTip.showTooltip = false;
        }
        // Metal
        if (metalDeltaTimer > 0.0f)
        {
            metalDeltaTimer -= Time.unscaledDeltaTime;
            metalDeltaTip.showTooltip = true;
        }
        else
        {
            metalDeltaTip.showTooltip = false;
        }


        // Info Bar

        int wavesSurvived = Mathf.Clamp(spawner.GetWaveCurrent() - 1, 0, 999);
        string plural = (wavesSurvived == 1) ? "" : "s";
        waveText.text = wavesSurvived.ToString() + " Invasion" + plural + " Survived";
    }

    private void RefreshResources()
    {
        // Reesources

        float foodVel = game.GetFoodVelocity(1);
        string foodSign = (Mathf.Sign(foodVel) == 1) ? "+" : "";
        float foodVelDP = Mathf.Round(foodVel * 10f) * .1f;
        foodText.text = game.playerData.GetResource(ResourceType.food).ToString() + "/" + game.playerData.GetResourceMax(ResourceType.food).ToString() + " (" + foodSign + foodVelDP.ToString() + "/s)";
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
        woodText.text = game.playerData.GetResource(ResourceType.wood).ToString() + "/" + game.playerData.GetResourceMax(ResourceType.wood).ToString() + " (" + woodSign + woodVelDP.ToString() + "/s)";
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
        metalText.text = game.playerData.GetResource(ResourceType.metal).ToString() + "/" + game.playerData.GetResourceMax(ResourceType.metal).ToString() + " (" + metalSign + metalVelDP.ToString() + "/s)";
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

    public void ShowResourceDelta(int _food, int _wood, int _metal)
    {
        if (_food != 0)
        {
            foodDeltaTimer = 1.5f;
            string foodDeltaSign = (_food > 0) ? "+" : "";
            foodDeltaText.text = foodDeltaSign + _food.ToString();
            foodDeltaText.color = (_food > 0) ? gainColour : lossColour;
            foodDeltaTip.PulseTip();
        }

        if (_wood != 0)
        {
            woodDeltaTimer = 1.5f;
            string woodDeltaSign = (_wood > 0) ? "+" : "";
            woodDeltaText.text = woodDeltaSign + _wood.ToString();
            woodDeltaText.color = (_wood > 0) ? gainColour : lossColour;
            woodDeltaTip.PulseTip();
        }

        if (_metal != 0)
        {
            metalDeltaTimer = 1.5f;
            string metalDeltaSign = (_metal > 0) ? "+" : "";
            metalDeltaText.text = metalDeltaSign + _metal.ToString();
            metalDeltaText.color = (_metal > 0) ? gainColour : lossColour;
            metalDeltaTip.PulseTip();
        }


        RefreshResources();
    }

    public void SetOverUI(bool isOver)
    {
        if (structMan == null)
            return;

        structMan.SetIsOverUI(isOver);
    }
}
