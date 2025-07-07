using System.Collections;
using System.Collections.Generic;
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

    public override Color GetCardColor()
    {
        return placeablePrefab.PlaceableType switch
        {
            PlaceableType.Crop => new Color(0f, 0f, 0.3f),
            PlaceableType.Animal => new Color(0.3f, 0f, 0f),
            PlaceableType.Building => new Color(0.2f, 0.2f, 0.2f),
            _ => Color.white,
        };
    }
}
