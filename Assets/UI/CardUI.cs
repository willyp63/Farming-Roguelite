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
    private TextMeshProUGUI costText;

    [SerializeField]
    private Image cardImage;

    [SerializeField]
    private TextMeshProUGUI cardText;

    [SerializeField]
    private float dragScale = 0.33f;

    private Card card;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private List<Image> raycastTargetImages;
    private List<bool> originalRaycastTargetStates;

    // Events
    public UnityEvent<CardUI> OnCardDragStarted;
    public UnityEvent<CardUI> OnCardDragEnded;

    private void Awake()
    {
        // Cache all Image components that have raycast targets
        raycastTargetImages = new List<Image>();
        originalRaycastTargetStates = new List<bool>();

        Image[] images = GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            if (image.raycastTarget)
            {
                raycastTargetImages.Add(image);
                originalRaycastTargetStates.Add(true);
            }
        }
    }

    public void SetCard(Card card)
    {
        this.card = card;
        costText.text = card.Cost.ToString();
        cardImage.sprite = card.Image;
        cardText.text = card.Text;
    }

    public Card GetCard()
    {
        return card;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalPosition = transform.position;

        // Disable raycast targets on all Image components to allow pointer events to pass through
        for (int i = 0; i < raycastTargetImages.Count; i++)
        {
            raycastTargetImages[i].raycastTarget = false;
        }

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

        // Re-enable raycast targets on all Image components
        for (int i = 0; i < raycastTargetImages.Count; i++)
        {
            raycastTargetImages[i].raycastTarget = originalRaycastTargetStates[i];
        }

        OnCardDragEnded?.Invoke(this);
    }
}
