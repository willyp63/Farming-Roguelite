using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileChangeEffect : PlaceableEffect
{
    [SerializeField]
    private List<Tile> fromTiles;

    [SerializeField]
    private Tile toTile;

    public override void OnPlace(Placeable placeable) { }

    public override void OnRemove(Placeable placeable) { }

    public override void OnEndOfTurn(Placeable placeable)
    {
        if (!fromTiles.Contains(placeable.GridTile.Tile))
            return;

        FloatingTextManager.Instance.SpawnText(
            $"{placeable.GridTile.Tile.TileName} -> {toTile.TileName}",
            placeable.GridTile.transform.position,
            Color.white
        );

        placeable.GridTile.SetTile(toTile);
    }

    public override void OnStartOfTurn(Placeable placeable) { }

    public override void OnEndOfRound(Placeable placeable) { }

    public override void OnStartOfRound(Placeable placeable) { }
}
