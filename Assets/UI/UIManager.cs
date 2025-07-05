using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]
    private TextMeshProUGUI coinText;

    [SerializeField]
    private CardUI cardUIPrefab;

    [SerializeField]
    private Transform cardContainer;

    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private GameObject messagePanel;

    [SerializeField]
    private Button endTurnButton;

    private List<CardUI> cardUIElements = new List<CardUI>();

    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragStarted = new();

    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragEnded = new();

    private void Start()
    {
        // Subscribe to coin events
        CoinManager.Instance.OnCoinsChanged.AddListener(UpdateCoinDisplay);

        // Subscribe to hand manager events
        CardManager.Instance.OnCardAddedToHand.AddListener(OnCardAdded);
        CardManager.Instance.OnCardRemovedFromHand.AddListener(OnCardRemoved);

        // Initialize coin display
        UpdateCoinDisplay(CoinManager.Instance.CurrentCoins);

        // Initialize hand display
        UpdateHandDisplay();

        // Hide message panel initially
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unsubscribe from events to prevent memory leaks
        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.OnCoinsChanged.RemoveListener(UpdateCoinDisplay);
        }

        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardAddedToHand.RemoveListener(OnCardAdded);
            CardManager.Instance.OnCardRemovedFromHand.RemoveListener(OnCardRemoved);
        }
    }

    private void UpdateCoinDisplay(int newAmount)
    {
        if (coinText != null)
        {
            coinText.text = newAmount.ToString();
        }
    }

    private void OnCardAdded(Card card)
    {
        Debug.Log($"Card added to hand: {card.name}");
        CreateCardUI(card);
    }

    private void OnCardRemoved(Card card)
    {
        RemoveCardUI(card);
    }

    private void UpdateHandDisplay()
    {
        // Clear existing card UI elements
        ClearCardUIElements();

        // Create UI elements for all cards in hand
        foreach (Card card in CardManager.Instance.Hand)
        {
            CreateCardUI(card);
        }
    }

    private void CreateCardUI(Card card)
    {
        if (cardUIPrefab == null || cardContainer == null)
        {
            Debug.LogError("CardUI prefab or card container is not assigned!");
            return;
        }

        // Instantiate the card UI
        CardUI cardUI = Instantiate(cardUIPrefab, cardContainer);
        cardUI.SetCard(card);

        // Subscribe to card UI events
        cardUI.OnCardDragStarted.AddListener(OnCardDragStart);
        cardUI.OnCardDragEnded.AddListener(OnCardDragEnd);

        // Add to our list
        cardUIElements.Add(cardUI);
    }

    private void RemoveCardUI(Card card)
    {
        // Find the CardUI element for this card
        CardUI cardUIToRemove = null;
        foreach (CardUI cardUI in cardUIElements)
        {
            if (cardUI.GetCard() == card)
            {
                cardUIToRemove = cardUI;
                break;
            }
        }

        if (cardUIToRemove != null)
        {
            // Unsubscribe from events
            cardUIToRemove.OnCardDragStarted.RemoveListener(OnCardDragStart);
            cardUIToRemove.OnCardDragEnded.RemoveListener(OnCardDragEnd);

            // Remove from list
            cardUIElements.Remove(cardUIToRemove);

            // Destroy the GameObject
            Destroy(cardUIToRemove.gameObject);
        }
    }

    private void ClearCardUIElements()
    {
        // Unsubscribe from all events and destroy all card UI elements
        foreach (CardUI cardUI in cardUIElements)
        {
            if (cardUI != null)
            {
                cardUI.OnCardDragStarted.RemoveListener(OnCardDragStart);
                cardUI.OnCardDragEnded.RemoveListener(OnCardDragEnd);
                Destroy(cardUI.gameObject);
            }
        }

        cardUIElements.Clear();
    }

    private void OnCardDragStart(CardUI cardUI)
    {
        OnCardDragStarted?.Invoke(cardUI);
    }

    private void OnCardDragEnd(CardUI cardUI)
    {
        OnCardDragEnded?.Invoke(cardUI);
    }

    public void ShowMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (messagePanel != null)
        {
            messagePanel.SetActive(true);

            // Auto-hide the message after 3 seconds
            StartCoroutine(HideMessageAfterDelay(3f));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }

    private void OnEndTurnButtonClicked()
    {
        GameManager.Instance.CompleteTurn();
    }
}
