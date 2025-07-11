using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPlaceableEffect : PlaceableEffect
{
    [SerializeField]
    private int amount;

    [SerializeField]
    private float chance = 1f;

    protected override void ApplyEffect(
        GridTile tile,
        GridTile newTile,
        List<GridTile> applyToTiles,
        int count
    )
    {
        int totalAmount = amount * count;
        if (totalAmount == 0)
            return;

        foreach (GridTile applyToTile in applyToTiles)
        {
            if (Random.value > chance)
                continue;

            GenerateCoins(applyToTile, totalAmount);
        }
    }

    private void GenerateCoins(GridTile tile, int totalAmount)
    {
        PlayerManager.Instance.GainMoney(totalAmount);

        FloatingTextManager.Instance.SpawnText(
            $"+${totalAmount}",
            tile.transform.position,
            Color.yellow
        );
    }
}
