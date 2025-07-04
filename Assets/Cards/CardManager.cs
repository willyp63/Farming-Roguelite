using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardManager : Singleton<CardManager>
{
    [Header("Hand Settings")]
    [SerializeField]
    private int maxHandSize = 8;

    [Header("Hand Settings")]
    [SerializeField]
    private List<Card> startingDeck;

    private List<Card> hand = new List<Card>();
    private List<Card> deck = new List<Card>();
    private List<Card> discard = new List<Card>();

    public IReadOnlyList<Card> Hand => hand.AsReadOnly();
    public IReadOnlyList<Card> Deck => deck.AsReadOnly();
    public IReadOnlyList<Card> Discard => discard.AsReadOnly();

    public UnityEvent<Card> OnCardAddedToHand;
    public UnityEvent<Card> OnCardRemovedFromHand;

    public int HandSize => hand.Count;
    public int MaxHandSize => maxHandSize;

    public bool IsHandFull => hand.Count >= maxHandSize;
    public bool IsHandEmpty => hand.Count == 0;

    private void Start()
    {
        Reset();
    }

    public void Reset()
    {
        deck.Clear();
        hand.Clear();
        discard.Clear();

        foreach (Card card in startingDeck)
        {
            deck.Add(card);
        }
    }

    // TODO: add methods for drawing cards, discarding cards, shuffling, etc...
}
