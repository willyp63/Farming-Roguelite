using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Effect Card", menuName = "Farming Roguelike/Effect Card")]
public class EffectCard : Card
{
    [SerializeField]
    private string cardName;
    public string CardName => cardName;

    [SerializeField]
    private PlaceableEffect effectPrefab;
    public PlaceableEffect EffectPrefab => effectPrefab;

    [SerializeField]
    private List<TileType> allowedTileTypes;
    public List<TileType> AllowedTileTypes => allowedTileTypes;

    [SerializeField]
    private SeasonType season = SeasonType.Spring;
    public SeasonType Season => season;

    public override void PlayCard(GridTile tile)
    {
        // TODO: implement
    }

    public override bool IsValidPlacement(GridTile tile)
    {
        bool isAllowedTile = GetAllowedTileTypes().Contains(tile.Tile.TileType);
        bool isTileEmpty = tile.PlacedObject == null;

        return isAllowedTile && isTileEmpty;
    }

    public override List<TileType> GetAllowedTileTypes()
    {
        return allowedTileTypes;
    }

    public override SeasonType GetSeason()
    {
        return season;
    }

    public override string GetTooltipText()
    {
        return CardName;
    }

    public override PlaceableFamily GetPlaceableFamily()
    {
        return PlaceableFamily.None;
    }
}
