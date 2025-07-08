using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CardQuantity
{
    public Card card;
    public int quantity;
}

public class CardManager : Singleton<CardManager>
{
    [Header("Hand Settings")]
    [SerializeField]
    private int maxHandSize = 8;

    [Header("Hand Settings")]
    [SerializeField]
    private List<CardQuantity> startingDeck;

    private List<Card> hand = new List<Card>();
    private List<Card> deck = new List<Card>();
    private List<Card> discard = new List<Card>();

    public IReadOnlyList<Card> Hand => hand.AsReadOnly();
    public IReadOnlyList<Card> Deck => deck.AsReadOnly();
    public IReadOnlyList<Card> Discard => discard.AsReadOnly();

    [NonSerialized]
    public UnityEvent<Card> OnCardAddedToHand = new();

    [NonSerialized]
    public UnityEvent<Card> OnCardRemovedFromHand = new();

    [NonSerialized]
    public UnityEvent<int> OnDeckCountChanged = new();

    [NonSerialized]
    public UnityEvent<int> OnDiscardCountChanged = new();

    public int HandSize => hand.Count;
    public int MaxHandSize => maxHandSize;

    public bool IsHandFull => hand.Count >= maxHandSize;
    public bool IsHandEmpty => hand.Count == 0;

    public int CardsInDeck => deck.Count;
    public bool IsDeckEmpty => deck.Count == 0;

    public int CardsInDiscard => discard.Count;
    public bool IsDiscardEmpty => discard.Count == 0;

    public void Reset()
    {
        deck.Clear();
        hand.Clear();
        discard.Clear();

        foreach (CardQuantity cardQuantity in startingDeck)
        {
            for (int i = 0; i < cardQuantity.quantity; i++)
            {
                deck.Add(cardQuantity.card);
            }
        }

        ShuffleDeck();

        OnDeckCountChanged?.Invoke(deck.Count);
        OnDiscardCountChanged?.Invoke(discard.Count);
    }

    public void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public Card DrawCard()
    {
        if (deck.Count == 0 || IsHandFull)
        {
            return null;
        }

        Card drawnCard = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawnCard);

        OnCardAddedToHand?.Invoke(drawnCard);
        OnDeckCountChanged?.Invoke(deck.Count);

        return drawnCard;
    }

    public List<Card> DrawCards(int count)
    {
        List<Card> drawnCards = new List<Card>();

        for (int i = 0; i < count; i++)
        {
            Card card = DrawCard();
            if (card != null)
            {
                drawnCards.Add(card);
            }
            else
            {
                break; // Stop if we can't draw more cards
            }
        }

        return drawnCards;
    }

    public bool DiscardCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            discard.Add(card);

            OnCardRemovedFromHand?.Invoke(card);
            OnDiscardCountChanged?.Invoke(discard.Count);

            return true;
        }
        return false;
    }

    public Card DiscardCardAt(int index)
    {
        if (index >= 0 && index < hand.Count)
        {
            Card card = hand[index];
            hand.RemoveAt(index);
            discard.Add(card);

            OnCardRemovedFromHand?.Invoke(card);
            OnDiscardCountChanged?.Invoke(discard.Count);

            return card;
        }
        return null;
    }

    public int DiscardCards(List<Card> cards)
    {
        int discardedCount = 0;
        foreach (Card card in cards)
        {
            if (DiscardCard(card))
            {
                discardedCount++;
            }
        }
        return discardedCount;
    }

    public void ShuffleDiscardIntoDeck()
    {
        deck.AddRange(discard);
        discard.Clear();
        ShuffleDeck();

        OnDeckCountChanged?.Invoke(deck.Count);
        OnDiscardCountChanged?.Invoke(discard.Count);
    }
}
