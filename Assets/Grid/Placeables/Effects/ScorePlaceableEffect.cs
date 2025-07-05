using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private int scoreAddition = 0;

    [SerializeField]
    private int scoreMultiplier = 1;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        int totalScoreAddition = scoreAddition * count;
        int totalScoreMultiplier = (int)Mathf.Pow(scoreMultiplier, count);

        foreach (GridTile applyToTile in applyToTiles)
        {
            ApplyEffectToTile(applyToTile, totalScoreAddition, totalScoreMultiplier);
        }
    }

    private void ApplyEffectToTile(GridTile tile, int totalScoreAddition, int totalScoreMultiplier)
    {
        Placeable placeable = tile.PlacedObject;
        if (placeable == null)
            return;

        if (totalScoreAddition != 0)
        {
            placeable.AddScore(totalScoreAddition);

            FloatingTextManager.Instance.SpawnText(
                $"+{totalScoreAddition}",
                placeable.GridTile.transform.position,
                new Color(200f / 255f, 0f / 255f, 255f / 255f, 1f)
            );
        }

        if (totalScoreMultiplier != 1)
        {
            placeable.MultiplyScore(totalScoreMultiplier);

            FloatingTextManager.Instance.SpawnText(
                $"x{totalScoreMultiplier}",
                placeable.GridTile.transform.position,
                new Color(200f / 255f, 0f / 255f, 255f / 255f, 1f)
            );
        }
    }
}
