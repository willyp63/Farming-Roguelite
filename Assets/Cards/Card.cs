using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : ScriptableObject
{
    [SerializeField]
    private Sprite image;
    public Sprite Image => image;

    [SerializeField]
    private int energyCost = 0;
    public int EnergyCost => energyCost;

    public abstract void PlayCard(GridTile tile);

    public abstract bool IsValidPlacement(GridTile tile);

    public abstract List<TileType> GetAllowedTileTypes();

    public abstract SeasonType GetSeason();

    public abstract string GetTooltipText();

    public abstract PlaceableFamily GetPlaceableFamily();
}
