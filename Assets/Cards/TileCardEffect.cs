using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Tile Card Effect",
    menuName = "Farming Roguelike/Card Effects/Tile Card Effect"
)]
public class TileCardEffect : CardEffect
{
    [SerializeField]
    private List<Tile> fromTiles;

    [SerializeField]
    private Tile toTile;

    public override bool IsValidPlacement(Vector2Int position, GridTile tile)
    {
        return tile != null && fromTiles.Contains(tile.Tile);
    }

    public override void ApplyEffect(Vector2Int position, GridTile tile)
    {
        GridManager.Instance.GetTile(position).SetTile(toTile);
    }
}
