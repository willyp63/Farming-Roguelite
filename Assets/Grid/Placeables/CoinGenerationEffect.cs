using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinGenerationEffect : PlaceableEffect
{
    [SerializeField]
    private int amount;

    public override void OnPlace(Placeable placeable) { }

    public override void OnRemove(Placeable placeable) { }

    public override void OnEndOfTurn(Placeable placeable)
    {
        CoinManager.Instance.GainCoins(amount);

        FloatingTextManager.Instance.SpawnText(
            $"+{amount} coins",
            placeable.GridTile.transform.position,
            Color.yellow
        );
    }

    public override void OnStartOfTurn(Placeable placeable) { }

    public override void OnEndOfRound(Placeable placeable) { }

    public override void OnStartOfRound(Placeable placeable) { }
}
