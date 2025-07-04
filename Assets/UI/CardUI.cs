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
    private TextMeshProUGUI baseScoreText;

    [SerializeField]
    private Image cardImage;

    [SerializeField]
    private TextMeshProUGUI cardText;

    [SerializeField]
    private float dragScale = 0.33f;

    [SerializeField]
    private List<AllowedTileTypeUI> allowedTileTypes;

    private Card card;
    private Vector3 originalPosition;
    private bool isDragging = false;
    private CanvasGroup canvasGroup;

    // Events
    public UnityEvent<CardUI> OnCardDragStarted;
    public UnityEvent<CardUI> OnCardDragEnded;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetCard(Card card)
    {
        this.card = card;
        baseScoreText.text = card.BaseScore.ToString();
        cardImage.sprite = card.Image;
        cardText.text = card.Text;

        foreach (AllowedTileTypeUI allowedTileType in allowedTileTypes)
        {
            allowedTileType.gameObject.SetActive(
                card.AllowedTileTypes.Contains(allowedTileType.TileType)
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
