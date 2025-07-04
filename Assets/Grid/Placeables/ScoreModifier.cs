using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreModifier : PlaceableEffect
{
    [SerializeField]
    private int scoreAddition = 0;

    [SerializeField]
    private int scoreMultiplier = 1;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        foreach (GridTile affectedTile in affectedTiles)
        {
            ApplyEffectToTile(affectedTile);
        }
    }

    private void ApplyEffectToTile(GridTile tile)
    {
        Placeable placeable = tile.PlacedObject;
        if (placeable == null)
        {
            return;
        }

        if (scoreAddition != 0)
        {
            placeable.AddScore(scoreAddition);
            FloatingTextManager.Instance.SpawnText(
                $"+{scoreAddition}",
                placeable.GridTile.transform.position,
                new Color(200f / 255f, 0f / 255f, 255f / 255f, 1f)
            );
        }

        if (scoreMultiplier != 1)
        {
            placeable.MultiplyScore(scoreMultiplier);
            FloatingTextManager.Instance.SpawnText(
                $"x{scoreMultiplier}",
                placeable.GridTile.transform.position,
                new Color(200f / 255f, 0f / 255f, 255f / 255f, 1f)
            );
        }
    }
}
