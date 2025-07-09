using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private TileType toTileType;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        Debug.Log($"Applying effect to {applyToTiles.Count} tiles");
        foreach (GridTile applyToTile in applyToTiles)
        {
            TileInfo toTile = TileManager.GetTileInfo(toTileType);
            applyToTile.SetTile(toTile);
        }
    }
}
