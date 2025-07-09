using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private Image cardImage;

    [SerializeField]
    private List<Image> backgroundImages;

    [SerializeField]
    private List<Image> seasonImages;

    [SerializeField]
    private float dragScale = 0.33f;

    private Card card;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private CanvasGroup canvasGroup;

    private TooltipTrigger tooltipTrigger;

    // Events
    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragStarted = new();

    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragEnded = new();

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        tooltipTrigger = GetComponent<TooltipTrigger>();
    }

    public void SetCard(Card card)
    {
        this.card = card;
        cardImage.sprite = card.Image;

        tooltipTrigger.SetTooltipText(card.GetTooltipText());

        SeasonType season = card.GetSeason();
        Color seasonColor = SeasonManager.GetSeasonInfo(season).color;

        foreach (Image backgroundImage in backgroundImages)
        {
            backgroundImage.color = seasonColor;
        }

        Sprite seasonSymbol = SeasonManager.GetSeasonInfo(season).symbol;
        if (seasonSymbol == null)
        {
            Debug.LogWarning($"No season symbol found for season {season}");
            return;
        }

        foreach (Image seasonImage in seasonImages)
        {
            seasonImage.sprite = seasonSymbol;
        }
    }

    public Card GetCard()
    {
        return card;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!RoundManager.Instance.CanPlayCards)
        {
            return;
        }

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
        if (isDragging)
        {
            isDragging = false;
            transform.position = originalPosition;
            transform.localScale = Vector3.one;

            // Re-enable raycast blocking
            canvasGroup.blocksRaycasts = true;

            OnCardDragEnded?.Invoke(this);
        }
    }
}
