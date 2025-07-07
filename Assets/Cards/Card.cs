using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    [SerializeField]
    private string cardName;
    public string CardName => cardName;

    [SerializeField]
    private Sprite image;
    public Sprite Image => image;

    [SerializeField]
    [TextArea(4, 8)]
    private string text;
    public string Text => text;

    [SerializeField]
    private int energyCost = 0;
    public int EnergyCost => energyCost;

    public abstract void PlayCard(GridTile tile);

    public abstract bool IsValidPlacement(GridTile tile);

    public abstract List<TileType> GetAllowedTileTypes();

    public abstract Color GetCardColor();
}
