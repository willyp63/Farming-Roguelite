using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileChangeEffect : PlaceableEffect
{
    [SerializeField]
    private List<TileType> fromTileTypes;

    [SerializeField]
    private Tile toTile;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        foreach (GridTile affectedTile in affectedTiles)
        {
            if (!fromTileTypes.Contains(affectedTile.Tile.TileType))
                continue;

            FloatingTextManager.Instance.SpawnText(
                $"{affectedTile.Tile.TileName} -> {toTile.TileName}",
                affectedTile.transform.position,
                Color.white
            );
        }
    }
}
