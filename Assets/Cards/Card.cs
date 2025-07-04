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
    [TextArea(4, 8)]
    private string text;

    [SerializeField]
    private int baseScore;

    [SerializeField]
    private List<TileType> allowedTileTypes;

    [SerializeField]
    private CardEffect effect;

    public string CardName => cardName;
    public Sprite Image => image;
    public string Text => text;
    public int BaseScore => baseScore;
    public List<TileType> AllowedTileTypes => allowedTileTypes;
    public CardEffect CardEffect => effect;
}
