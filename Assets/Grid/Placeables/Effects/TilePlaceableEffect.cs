using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private Tile toTile;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        foreach (GridTile applyToTile in applyToTiles)
        {
            applyToTile.SetTile(toTile);
        }
    }
}
