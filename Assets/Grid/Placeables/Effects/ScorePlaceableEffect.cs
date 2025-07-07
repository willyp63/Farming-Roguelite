using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private int pointAddition = 0;

    [SerializeField]
    private int pointMultiplier = 1;

    [SerializeField]
    private int multiAddition = 0;

    [SerializeField]
    private int multiMultiplier = 1;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        int totalPointAddition = pointAddition * count;
        int totalPointMultiplier = (int)Mathf.Pow(pointMultiplier, count);
        int totalMultiAddition = multiAddition * count;
        int totalMultiMultiplier = (int)Mathf.Pow(multiMultiplier, count);

        foreach (GridTile applyToTile in applyToTiles)
        {
            ApplyEffectToTile(
                applyToTile,
                totalPointAddition,
                totalPointMultiplier,
                totalMultiAddition,
                totalMultiMultiplier
            );
        }
    }

    private void ApplyEffectToTile(
        GridTile tile,
        int totalPointAddition,
        int totalPointMultiplier,
        int totalMultiAddition,
        int totalMultiMultiplier
    )
    {
        Placeable placeable = tile.PlacedObject;
        if (placeable == null)
            return;

        if (totalPointAddition != 0)
        {
            RoundManager.Instance.AddPoints(totalPointAddition);
            SpawnFloatingText($"+{totalPointAddition}", tile);
        }

        if (totalPointMultiplier != 1)
        {
            RoundManager.Instance.AddPoints(0, totalPointMultiplier);
            SpawnFloatingText($"x{totalPointMultiplier}", tile);
        }

        if (totalMultiAddition != 0)
        {
            RoundManager.Instance.AddMulti(totalMultiAddition);
            SpawnFloatingText($"+{totalMultiAddition}", tile, true);
        }

        if (totalMultiMultiplier != 1)
        {
            RoundManager.Instance.AddMulti(0, totalMultiMultiplier);
            SpawnFloatingText($"x{totalMultiMultiplier}", tile, true);
        }
    }

    private void SpawnFloatingText(string text, GridTile tile, bool isMulti = false)
    {
        FloatingTextManager.Instance.SpawnText(
            text,
            tile.transform.position,
            isMulti ? new Color(1f, 0f, 0f, 1f) : new Color(0f, 0f, 1f, 1f)
        );
    }
}
