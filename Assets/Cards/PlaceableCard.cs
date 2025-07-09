using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Placeable Card", menuName = "Farming Roguelike/Placeable Card")]
public class PlaceableCard : Card
{
    [SerializeField]
    private Placeable placeablePrefab;
    public Placeable PlaceablePrefab => placeablePrefab;

    public override void PlayCard(GridTile tile)
    {
        GridManager.Instance.PlaceObject(tile.Position, placeablePrefab);
        Placeable placeable = GridManager
            .Instance
            .Grid[tile.Position.x, tile.Position.y]
            .PlacedObject;
        placeable.SetCard(this);
    }

    public override bool IsValidPlacement(GridTile tile)
    {
        bool isAllowedTile = GetAllowedTileTypes().Contains(tile.Tile.TileType);
        bool isTileEmpty = tile.PlacedObject == null;

        return isAllowedTile && isTileEmpty;
    }

    public override List<TileType> GetAllowedTileTypes()
    {
        return placeablePrefab.AllowedTileTypes;
    }

    public override SeasonType GetSeason()
    {
        return placeablePrefab.Season;
    }

    public override string GetTooltipText()
    {
        return placeablePrefab.GetTooltipText();
    }

    public override PlaceableFamily GetPlaceableFamily()
    {
        return placeablePrefab.PlaceableFamily;
    }
}
