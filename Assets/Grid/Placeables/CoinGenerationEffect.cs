using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerationEffect : PlaceableEffect
{
    [SerializeField]
    private int amount;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        foreach (GridTile affectedTile in affectedTiles)
        {
            GenerateCoins(amount, affectedTile);
        }
    }

    private void GenerateCoins(int amount, GridTile tile)
    {
        CoinManager.Instance.GainCoins(amount);

        FloatingTextManager.Instance.SpawnText(
            $"+{amount} coins",
            tile.transform.position,
            Color.yellow
        );
    }
}
