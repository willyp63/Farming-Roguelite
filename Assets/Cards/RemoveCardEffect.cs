using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Remove Card Effect",
    menuName = "Farming Roguelike/Card Effects/Remove Card Effect"
)]
public class RemoveCardEffect : CardEffect
{
    public override bool IsValidPlacement(Vector2Int position, GridTile tile, Card card)
    {
        return base.IsValidPlacement(position, tile, card) && tile.PlacedObject != null;
    }

    public override void ApplyEffect(Vector2Int position, GridTile tile, Card card)
    {
        GridManager.Instance.RemoveObject(position);
    }
}
