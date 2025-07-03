using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HandManager : Singleton<HandManager>
{
    [Header("Hand Settings")]
    [SerializeField]
    private int maxHandSize = 8;

    [Header("Hand Settings")]
    [SerializeField]
    private List<Card> startingHand;

    public UnityEvent<Card> OnCardAdded;
    public UnityEvent<Card> OnCardRemoved;

    private List<Card> hand = new List<Card>();

    public int HandSize => hand.Count;
    public int MaxHandSize => maxHandSize;
    public IReadOnlyList<Card> Cards => hand.AsReadOnly();
    public bool IsHandFull => hand.Count >= maxHandSize;
    public bool IsHandEmpty => hand.Count == 0;

    private void Start()
    {
        foreach (Card card in startingHand)
        {
            AddCard(card);
        }
    }

    public bool AddCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("Attempted to add null card to hand");
            return false;
        }

        if (IsHandFull)
        {
            Debug.LogWarning(
                $"Cannot add card '{card.CardName}' - hand is full (max size: {maxHandSize})"
            );
            return false;
        }

        hand.Add(card);
        OnCardAdded?.Invoke(card);

        return true;
    }

    public Card RemoveCardAt(int index)
    {
        if (index < 0 || index >= hand.Count)
        {
            Debug.LogWarning($"Invalid card index: {index}. Hand size: {hand.Count}");
            return null;
        }

        Card removedCard = hand[index];
        hand.RemoveAt(index);
        OnCardRemoved?.Invoke(removedCard);

        return removedCard;
    }

    public bool RemoveCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("Attempted to remove null card from hand");
            return false;
        }

        int index = hand.IndexOf(card);
        if (index == -1)
        {
            Debug.LogWarning($"Card '{card.CardName}' not found in hand");
            return false;
        }

        RemoveCardAt(index);
        return true;
    }

    public Card GetCardAt(int index)
    {
        if (index < 0 || index >= hand.Count)
        {
            Debug.LogWarning($"Invalid card index: {index}. Hand size: {hand.Count}");
            return null;
        }

        return hand[index];
    }

    public void ClearHand()
    {
        hand.Clear();
    }
}
