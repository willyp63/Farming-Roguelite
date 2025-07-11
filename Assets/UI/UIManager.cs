using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Text Elements")]
    [SerializeField]
    private TextMeshProUGUI dayText;

    [SerializeField]
    private TextMeshProUGUI requiredScoreText;

    [SerializeField]
    private TextMeshProUGUI lineLabelText;

    [SerializeField]
    private TextMeshProUGUI lineMultipliersText;

    [SerializeField]
    private TextMeshProUGUI lineScoreText;

    [SerializeField]
    private TextMeshProUGUI moneyText;

    [SerializeField]
    private TextMeshProUGUI numCardsPlayedText;

    [SerializeField]
    private TextMeshProUGUI deckCountText;

    [SerializeField]
    private TextMeshProUGUI discardCountText;

    [Header("Buttons")]
    [SerializeField]
    private Button endTurnButton;

    [SerializeField]
    private Button resetButton;

    [Header("Card UI")]
    [SerializeField]
    private Transform cardContainer;

    [SerializeField]
    private CardUI cardUIPrefab;

    // Events
    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragStarted = new();

    [NonSerialized]
    public UnityEvent<CardUI> OnCardDragEnded = new();

    private List<CardUI> cardUIElements = new List<CardUI>();

    public void Start()
    {
        // Subscribe to player manager events
        PlayerManager.Instance.OnMoneyChanged.AddListener(OnMoneyChanged);

        // Subscribe to hand manager events
        CardManager.Instance.OnCardAddedToHand.AddListener(OnCardAdded);
        CardManager.Instance.OnCardRemovedFromHand.AddListener(OnCardRemoved);
        CardManager.Instance.OnHandChanged.AddListener(OnHandChanged);

        // Subscribe to deck/discard events
        CardManager.Instance.OnDeckCountChanged.AddListener(OnDeckCountChanged);
        CardManager.Instance.OnDiscardCountChanged.AddListener(OnDiscardCountChanged);

        // Subscribe to score and day events
        RoundManager.Instance.OnScoreChange.AddListener(OnScoreChanged);
        RoundManager.Instance.OnDayChange.AddListener(OnDayChanged);

        // Subscribe to round events
        RoundManager.Instance.OnCardsPlayedChange.AddListener(OnCardsPlayedChange);

        // Initialize hand display
        UpdateHandDisplay();

        // Initialize required score
        requiredScoreText.text = RoundManager.Instance.RequiredScore.ToString();

        // Initialize Text elements
        OnMoneyChanged(PlayerManager.Instance.CurrentMoney);
        OnDeckCountChanged(CardManager.Instance.Deck.Count);
        OnDiscardCountChanged(CardManager.Instance.Discard.Count);
        OnScoreChanged(RoundManager.Instance.PointScore, RoundManager.Instance.MultiScore);
        OnDayChanged(RoundManager.Instance.CurrentDay);
        OnCardsPlayedChange(RoundManager.Instance.NumCardsPlayed);

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
        }

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unsubscribe from events to prevent memory leaks
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnMoneyChanged.RemoveListener(OnMoneyChanged);
        }

        if (CardManager.Instance != null)
        {
            CardManager.Instance.OnCardAddedToHand.RemoveListener(OnCardAdded);
            CardManager.Instance.OnCardRemovedFromHand.RemoveListener(OnCardRemoved);
        }
    }

    private void OnMoneyChanged(int newAmount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${newAmount}";
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

    private void OnHandChanged(List<Card> newHand)
    {
        UpdateHandDisplay();
    }

    private void OnDeckCountChanged(int newCount)
    {
        if (deckCountText != null)
        {
            deckCountText.text = newCount.ToString();
        }
    }

    private void OnDiscardCountChanged(int newCount)
    {
        if (discardCountText != null)
        {
            discardCountText.text = newCount.ToString();
        }
    }

    private void OnScoreChanged(int newPoints, int newMultiplier)
    {
        lineLabelText.text =
            GridManager.Instance.NumScoringTiles > 0
                ? $"Current score ({GridManager.Instance.NumScoringTiles} tiles):"
                : "Current score:";

        UpdateScoreTextElement(lineScoreText, RoundManager.Instance.TotalScore.ToString(), "0");
        UpdateScoreTextElement(
            lineMultipliersText,
            $"<color=#048BF0>{newPoints}</color> x <color=#F54840>{newMultiplier}</color>",
            $"<color=#048BF0>0</color> x <color=#F54840>0</color>"
        );
    }

    private void UpdateScoreTextElement(
        TextMeshProUGUI textElement,
        string newText,
        string doNotShakeOn = ""
    )
    {
        if (textElement == null)
            return;

        if (textElement.text != newText && newText != doNotShakeOn)
        {
            ShakeBehavior shake = textElement.GetComponent<ShakeBehavior>();
            if (shake != null)
                shake.Shake();
        }

        textElement.text = newText;
    }

    private void OnDayChanged(int newDay)
    {
        if (dayText != null)
        {
            dayText.text = $"Week {newDay} of {RoundManager.Instance.TotalDays}";
        }
    }

    private void OnCardsPlayedChange(int numCardsPlayed)
    {
        int maxCardsPlayedPerDay = RoundManager.Instance.MaxCardsPlayedPerDay;
        numCardsPlayedText.text = $"{maxCardsPlayedPerDay - numCardsPlayed}/{maxCardsPlayedPerDay}";
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

    private void OnEndTurnButtonClicked()
    {
        RoundManager.Instance.GoToNextDay();
    }

    private void OnResetButtonClicked()
    {
        RoundManager.Instance.ResetPlayedCards();
    }
}
