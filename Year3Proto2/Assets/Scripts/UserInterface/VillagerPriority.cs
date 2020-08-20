using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillagerPriority : MonoBehaviour
{
    public bool showPanel = false;
    private bool panelShown = false;
    public bool reorderCards = false;

    //private GameObject check;
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

    private float highPos;
    private float medPos;
    private float lowPos;

    public string draggedCard;
    private bool dragging;

    private float panelYinitial;
    private StructureManager structMan;

    private void Start()
    {
        panelYinitial = panel.localPosition.y;
        structMan = FindObjectOfType<StructureManager>();

        // Set Text
        foodText.text = (foodPriority + 1).ToString() + ".";
        woodText.text = (woodPriority + 1).ToString() + ".";
        metalText.text = (metalPriority + 1).ToString() + ".";

        highPos = foodCard.transform.localPosition.x;
        medPos = woodCard.transform.localPosition.x;
        lowPos = metalCard.transform.localPosition.x;
    }

    private void Update()
    {
        if (showPanel && !panelShown)
        {
            ShowPanel();
            panelShown = true;
        }

        if (!showPanel && panelShown)
        {
            HidePanel();
            panelShown = false;
        }
    }

    private void LateUpdate()
    {
        if (dragging && reorderCards)
        {
            switch (draggedCard)
            {
                case "Food":
                    foodCard.transform.position = Input.mousePosition;
                    foodCard.transform.SetAsLastSibling();
                    break;

                case "Wood":
                    woodCard.transform.position = Input.mousePosition;
                    woodCard.transform.SetAsLastSibling();
                    break;

                case "Metal":
                    metalCard.transform.position = Input.mousePosition;
                    metalCard.transform.SetAsLastSibling();
                    break;

                default:
                    break;
            }

            // Food
            if (foodCard.transform.localPosition.x < woodCard.transform.localPosition.x && foodPriority > woodPriority && foodPriority > 0)
            {
                foodPriority = woodPriority;
                woodPriority = foodPriority + 1;
                SetCards();
            }
            if (foodCard.transform.localPosition.x < metalCard.transform.localPosition.x && foodPriority > metalPriority && foodPriority > 0)
            {
                foodPriority = metalPriority;
                metalPriority = foodPriority + 1;
                SetCards();
            }

            // Wood
            if (woodCard.transform.localPosition.x < foodCard.transform.localPosition.x && woodPriority > foodPriority && woodPriority > 0)
            {
                woodPriority = foodPriority;
                foodPriority = woodPriority + 1;
                SetCards();
            }
            if (woodCard.transform.localPosition.x < metalCard.transform.localPosition.x && woodPriority > metalPriority && woodPriority > 0)
            {
                woodPriority = metalPriority;
                metalPriority = woodPriority + 1;
                SetCards();
            }

            // Metal
            if (metalCard.transform.localPosition.x < foodCard.transform.localPosition.x && metalPriority > foodPriority && metalPriority > 0)
            {
                metalPriority = foodPriority;
                foodPriority = metalPriority + 1;
                SetCards();
            }
            if (metalCard.transform.localPosition.x < woodCard.transform.localPosition.x && metalPriority > woodPriority && metalPriority > 0)
            {
                metalPriority = woodPriority;
                woodPriority = metalPriority + 1;
                SetCards();
            }
        }
    }

    public void TogglePanel()
    {
        showPanel = !showPanel;
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
                    if (foodPriority > 0)
                    {
                        foodPriority--;
                        if (woodPriority == foodPriority) { woodPriority++; }
                        if (metalPriority == foodPriority) { metalPriority++; }
                    }

                    structMan.SetPriority(Priority.Food);
                    break;

                case "Wood":
                    if (woodPriority > 0)
                    {
                        woodPriority--;
                        if (foodPriority == woodPriority) { foodPriority++; }
                        if (metalPriority == woodPriority) { metalPriority++; }
                    }

                    structMan.SetPriority(Priority.Wood);
                    break;

                case "Metal":
                    if (metalPriority > 0)
                    {
                        metalPriority--;
                        if (foodPriority == metalPriority) { foodPriority++; }
                        if (woodPriority == metalPriority) { woodPriority++; }
                    }

                    structMan.SetPriority(Priority.Metal);
                    break;

                default:
                    break;
            }
        }

        SetCards();
    }

    private void SetCards()
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
                switch (foodPriority)
                {
                    case 0:
                        foodCard.transform.DOLocalMove(new Vector2(highPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 1:
                        foodCard.transform.DOLocalMove(new Vector2(medPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 2:
                        foodCard.transform.DOLocalMove(new Vector2(lowPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;
                }
            }

            if (draggedCard != "Wood")
            {
                switch (woodPriority)
                {
                    case 0:
                        woodCard.transform.DOLocalMove(new Vector2(highPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 1:
                        woodCard.transform.DOLocalMove(new Vector2(medPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 2:
                        woodCard.transform.DOLocalMove(new Vector2(lowPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;
                }
            }

            if (draggedCard != "Metal")
            {
                switch (metalPriority)
                {
                    case 0:
                        metalCard.transform.DOLocalMove(new Vector2(highPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 1:
                        metalCard.transform.DOLocalMove(new Vector2(medPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;

                    case 2:
                        metalCard.transform.DOLocalMove(new Vector2(lowPos, 0.0f), 0.4f).SetEase(Ease.OutQuint);
                        break;
                }
            }
        }
    }

    public void DragCard(string _type)
    {
        draggedCard = _type;
        dragging = true;

        Debug.Log("Dragging " + _type);
    }

    public void ReleaseCard(string _type)
    {
        dragging = false;
        draggedCard = "";

        SetCards();
    }

    public void HideCheck()
    {
        //check.GetComponent<CanvasGroup>().alpha = 0.0f;
    }
}