using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOnMovePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private Placeable placeable;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        foreach (GridTile applyToTile in applyToTiles)
        {
            applyToTile.PlacedObject.SetSpawnPlaceableOnMove(placeable);
        }
    }
}
