using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Place Card Effect",
    menuName = "Farming Roguelike/Card Effects/Place Card Effect"
)]
public class PlaceCardEffect : CardEffect
{
    [SerializeField]
    private Placeable placeablePrefab;

    public override bool IsValidPlacement(Vector2Int position, GridTile tile)
    {
        return tile != null
            && tile.PlacedObject == null
            && placeablePrefab.ValidTileTypes.Contains(tile.Tile.TileType);
    }

    public override void ApplyEffect(Vector2Int position, GridTile tile)
    {
        GridManager.Instance.PlaceObject(position, placeablePrefab);
    }
}
