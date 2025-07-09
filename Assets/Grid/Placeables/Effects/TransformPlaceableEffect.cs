using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformPlaceableEffect : PlaceableEffect
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
            if (applyToTile.PlacedObject == null)
                continue;

            GridManager.Instance.RemoveObject(applyToTile.Position);
            GridManager.Instance.PlaceObject(applyToTile.Position, placeable, true);
        }
    }
}
