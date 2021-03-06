﻿//
// Bachelor of Creative Technologies
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name        : VillagerPriority.cs
// Description      : Controls the display and functionality of the Villager Priority Panel
// Author           : David Morris
// Mail             : David.Mor7851@mediadesign.school.nz
//

using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillagerPriority : MonoBehaviour
{
    private bool panelShown = false;
    public bool reorderCards = false;

    [Header("Panel Window Controls")]
    [SerializeField] private Transform panel;

    [SerializeField] private Transform toggleButton;
    [SerializeField] private Sprite minimizeSprite;
    [SerializeField] private Sprite expandSprite;

    [Header("Resource Priority Text")]
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text woodText;
    [SerializeField] private TMP_Text metalText;

    [Header("Resource Priority Cards")]
    [SerializeField] private GameObject foodCard;
    [SerializeField] private GameObject woodCard;
    [SerializeField] private GameObject metalCard;

    [SerializeField] private int foodPriority = 0;
    [SerializeField] private int woodPriority = 1;
    [SerializeField] private int metalPriority = 2;

    private List<float> posSlot;

    public string draggedCard;
    private bool dragging;
    public bool useDragOffset;
    private Vector3 dragPotentialStart;
    private Vector3 dragOffset;

    private float panelYinitial;

    private void Awake()
    {
        posSlot = new List<float>
        {
            foodCard.transform.localPosition.x,
            woodCard.transform.localPosition.x,
            metalCard.transform.localPosition.x
        };
    }

    private void Start()
    {
        panelYinitial = panel.localPosition.y;

        // Set Text
        foodText.text = (foodPriority + 1).ToString() + ".";
        woodText.text = (woodPriority + 1).ToString() + ".";
        metalText.text = (metalPriority + 1).ToString() + ".";


        ApplyCards();
    }

    private void Update()
    {
        if (SuperManager.GetInstance().GetShowPriority() && !panelShown)
        {
            ShowPanel();
            panelShown = true;
        }

        if (!SuperManager.GetInstance().GetShowPriority() && panelShown)
        {
            HidePanel();
            panelShown = false;
        }
    }

    private void OnGUI()
    {
        if (dragging && reorderCards)
        {
            switch (draggedCard)
            {
                case "Food":
                    foodCard.transform.position = Input.mousePosition + dragOffset;
                    foodCard.transform.SetAsLastSibling();
                    break;

                case "Wood":
                    woodCard.transform.position = Input.mousePosition + dragOffset;
                    woodCard.transform.SetAsLastSibling();
                    break;

                case "Metal":
                    metalCard.transform.position = Input.mousePosition + dragOffset;
                    metalCard.transform.SetAsLastSibling();
                    break;
            }


            // Food
            if (foodCard.transform.localPosition.x < woodCard.transform.localPosition.x && foodPriority >= woodPriority)
            {
                foodPriority = woodPriority;
                woodPriority = foodPriority + 1;
                ApplyCards();
            }
            if (foodCard.transform.localPosition.x < metalCard.transform.localPosition.x && foodPriority >= metalPriority)
            {
                foodPriority = metalPriority;
                metalPriority = foodPriority + 1;
                ApplyCards();
            }

            // Wood
            if (woodCard.transform.localPosition.x < foodCard.transform.localPosition.x && woodPriority >= foodPriority)
            {
                woodPriority = foodPriority;
                foodPriority = woodPriority + 1;
                ApplyCards();
            }
            if (woodCard.transform.localPosition.x < metalCard.transform.localPosition.x && woodPriority >= metalPriority)
            {
                woodPriority = metalPriority;
                metalPriority = woodPriority + 1;
                ApplyCards();
            }

            // Metal
            if (metalCard.transform.localPosition.x < foodCard.transform.localPosition.x && metalPriority >= foodPriority)
            {
                metalPriority = foodPriority;
                foodPriority = metalPriority + 1;
                ApplyCards();
            }
            if (metalCard.transform.localPosition.x < woodCard.transform.localPosition.x && metalPriority >= woodPriority)
            {
                metalPriority = woodPriority;
                woodPriority = metalPriority + 1;
                ApplyCards();
            }
        }
    }

    public void TogglePanel()
    {
        SuperManager.GetInstance().ToggleShowPriority();
    }

    private void ShowPanel()
    {
        panel.DOLocalMoveY(panelYinitial + 125.0f, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.transform.Rotate(new Vector3(0, 0, -180));
        toggleButton.GetComponent<Image>().sprite = minimizeSprite;
    }

    private void HidePanel()
    {
        panel.DOLocalMoveY(panelYinitial, 0.4f).SetEase(Ease.OutQuint);
        toggleButton.transform.Rotate(new Vector3(0, 0, 180));
        toggleButton.GetComponent<Image>().sprite = expandSprite;
    }

    public void Prioritize(string _type)
    {
        if (!dragging)
        {
            switch (_type)
            {
                case "Food":
                    if (foodPriority == 1)
                    {
                        foodPriority = 0;
                        if (woodPriority == foodPriority) { woodPriority++; }
                        if (metalPriority == foodPriority) { metalPriority++; }
                    }
                    else if (foodPriority == 2)
                    {
                        foodPriority = 0;
                        woodPriority++;
                        metalPriority++;
                    }
                    foodCard.transform.SetAsLastSibling();
                    break;

                case "Wood":
                    if (woodPriority == 1)
                    {
                        woodPriority = 0;
                        if (foodPriority == woodPriority) { foodPriority++; }
                        if (metalPriority == woodPriority) { metalPriority++; }
                    }
                    else if (woodPriority == 2)
                    {
                        woodPriority = 0;
                        foodPriority++;
                        metalPriority++;
                    }
                    woodCard.transform.SetAsLastSibling();
                    break;

                case "Metal":
                    if (metalPriority == 1)
                    {
                        metalPriority = 0;
                        if (foodPriority == metalPriority) { foodPriority++; }
                        if (woodPriority == metalPriority) { woodPriority++; }
                    }
                    else if (metalPriority == 2)
                    {
                        metalPriority = 0;
                        foodPriority++;
                        woodPriority++;
                    }
                    metalCard.transform.SetAsLastSibling();
                    break;
            }
        }

        ApplyCards();
        ApplyPriority();
    }

    private void ApplyCards()
    {
        // Set Text
        foodText.text = (foodPriority + 1).ToString() + ".";
        woodText.text = (woodPriority + 1).ToString() + ".";
        metalText.text = (metalPriority + 1).ToString() + ".";

        // Tween cards to correct position
        if (reorderCards)
        {
            if (draggedCard != "Food")
            {
                foodCard.transform.DOLocalMove(new Vector2(posSlot[foodPriority], 0.0f), 0.4f).SetEase(Ease.OutQuint);
            }

            if (draggedCard != "Wood")
            {
                woodCard.transform.DOLocalMove(new Vector2(posSlot[woodPriority], 0.0f), 0.4f).SetEase(Ease.OutQuint);
            }

            if (draggedCard != "Metal")
            {
                metalCard.transform.DOLocalMove(new Vector2(posSlot[metalPriority], 0.0f), 0.4f).SetEase(Ease.OutQuint);
            }
        }
    }

    public void StartPotantialDrag()
    {
        dragPotentialStart = Input.mousePosition;
    }

    public void DragCard(string _type)
    {
        draggedCard = _type;
        dragging = true;

        if (useDragOffset)
        {
            switch (draggedCard)
            {
                case "Food":
                    dragOffset = foodCard.transform.position - dragPotentialStart;
                    break;

                case "Wood":
                    dragOffset = woodCard.transform.position - dragPotentialStart;
                    break;

                case "Metal":
                    dragOffset = metalCard.transform.position - dragPotentialStart;
                    break;
            }
        }
        else
        {
            dragOffset = Vector3.zero;
        }

        //Debug.Log("Dragging " + _type);
    }

    public void ReleaseCard(string _type)
    {
        dragging = false;
        draggedCard = "";

        ApplyCards();
        ApplyPriority();

    }

    public void LoadCardPriorites(int _food, int _wood, int _metal)
    {
        foodPriority = _food;
        woodPriority = _wood;
        metalPriority = _metal;

        ApplyCards();
    }

    private void ApplyPriority()
    {
        VillagerManager.GetInstance().SetPriorities(foodPriority, woodPriority, metalPriority);
    }

}