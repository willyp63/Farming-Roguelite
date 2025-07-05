using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private int amount;

    protected override void ApplyEffect(GridTile tile, List<GridTile> affectedTiles)
    {
        if (isSelfModifier)
        {
            GenerateCoins(tile, amount * affectedTiles.Count);
        }
        else
        {
            foreach (GridTile affectedTile in affectedTiles)
            {
                GenerateCoins(affectedTile, amount);
            }
        }
    }

    private void GenerateCoins(GridTile tile, int totalAmount)
    {
        if (totalAmount == 0)
            return;

        CoinManager.Instance.GainCoins(totalAmount);

        FloatingTextManager.Instance.SpawnText(
            $"+${totalAmount}",
            tile.transform.position,
            Color.yellow
        );
    }
}
