using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private Tile toTile;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        if (isSelfModifier)
        {
            if (affectedTiles.Count > 0)
                tile.SetTile(toTile);
        }
        else
        {
            foreach (GridTile affectedTile in affectedTiles)
            {
                affectedTile.SetTile(toTile);
            }
        }
    }
}
