using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private int pointAddition = 0;

    [SerializeField]
    private int multiAddition = 0;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        int totalPointAddition = pointAddition * count;
        int totalMultiAddition = multiAddition * count;

        foreach (GridTile applyToTile in applyToTiles)
        {
            ApplyEffectToTile(applyToTile, totalPointAddition, totalMultiAddition);
        }
    }

    private void ApplyEffectToTile(GridTile tile, int totalPointAddition, int totalMultiAddition)
    {
        Placeable placeable = tile.PlacedObject;
        if (placeable == null)
            return;

        if (totalPointAddition != 0)
        {
            RoundManager.Instance.AddPoints(totalPointAddition);
            FloatingTextManager.Instance.SpawnPointsText(
                totalPointAddition,
                tile.transform.position
            );
        }

        if (totalMultiAddition != 0)
        {
            RoundManager.Instance.AddMulti(totalMultiAddition);
            FloatingTextManager.Instance.SpawnMultiText(
                totalMultiAddition,
                tile.transform.position
            );
        }
    }
}
