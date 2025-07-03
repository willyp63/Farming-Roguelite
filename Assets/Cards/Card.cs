using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Farming Roguelike/Card")]
public class Card : ScriptableObject
{
    [SerializeField]
    private string cardName;

    [SerializeField]
    private Sprite image;

    [SerializeField]
    private string text;

    [SerializeField]
    private int cost;

    [SerializeField]
    private CardEffect effect;

    public string CardName => cardName;
    public Sprite Image => image;
    public string Text => text;
    public int Cost => cost;
    public CardEffect CardEffect => effect;
}
