using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreModifierPerTile : PlaceableEffect
{
    [SerializeField]
    private List<PlaceableType> matchingPlaceableTypes;

    [SerializeField]
    private List<TileType> matchingTileTypes;

    [SerializeField]
    private int scoreAddition = 0;

    [SerializeField]
    private int scoreMultiplier = 1;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        int totalScoreAddition = 0;
        int totalScoreMultiplier = 1;

        foreach (GridTile affectedTile in affectedTiles)
        {
            if (IsMatchingTile(affectedTile))
            {
                totalScoreAddition += scoreAddition;
                totalScoreMultiplier *= scoreMultiplier;
            }
        }

        ApplyEffectToTile(tile, totalScoreAddition, totalScoreMultiplier);
    }

    private bool IsMatchingTile(GridTile tile)
    {
        if (
            matchingPlaceableTypes.Count > 0
            && !matchingPlaceableTypes.Contains(
                tile.PlacedObject?.PlaceableType ?? PlaceableType.None
            )
        )
        {
            return false;
        }

        if (matchingTileTypes.Count > 0 && !matchingTileTypes.Contains(tile.Tile.TileType))
        {
            return false;
        }

        return true;
    }

    private void ApplyEffectToTile(GridTile tile, int totalScoreAddition, int totalScoreMultiplier)
    {
        Placeable placeable = tile.PlacedObject;
        if (placeable == null)
        {
            return;
        }

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
