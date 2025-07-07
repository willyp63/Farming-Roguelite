using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private TextMeshProUGUI energyCostText;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI cardText;

    [SerializeField]
    private Image cardImage;

    [SerializeField]
    private List<Image> backgroundImages;

    [SerializeField]
    private float dragScale = 0.33f;

    [SerializeField]
    private List<AllowedTileTypeUI> allowedTileTypes;

    private Card card;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private CanvasGroup canvasGroup;

    // Events
    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragStarted = new();

    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragEnded = new();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetCard(Card card)
    {
        this.card = card;
        energyCostText.text = card.EnergyCost.ToString();
        nameText.text = card.CardName;
        cardText.text = card.Text;
        cardImage.sprite = card.Image;

        foreach (Image backgroundImage in backgroundImages)
        {
            backgroundImage.color = card.GetCardColor();
        }

        foreach (AllowedTileTypeUI allowedTileType in allowedTileTypes)
        {
            allowedTileType.gameObject.SetActive(
                card.GetAllowedTileTypes().Contains(allowedTileType.TileType)
            );
        }
    }

    public Card GetCard()
    {
        return card;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalPosition = transform.position;

        // Disable raycast blocking to allow pointer events to pass through
        canvasGroup.blocksRaycasts = false;

        OnCardDragStarted?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            transform.position = eventData.position;
            transform.localScale = Vector3.one * dragScale;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        transform.position = originalPosition;
        transform.localScale = Vector3.one;

        // Re-enable raycast blocking
        canvasGroup.blocksRaycasts = true;

        OnCardDragEnded?.Invoke(this);
    }
}
