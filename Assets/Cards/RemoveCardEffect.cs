using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Remove Card Effect",
    menuName = "Farming Roguelike/Card Effects/Remove Card Effect"
)]
public class RemoveCardEffect : CardEffect
{
    public override bool IsValidPlacement(Vector2Int position, GridTile tile)
    {
        return tile != null && tile.PlacedObject != null;
    }

    public override void ApplyEffect(Vector2Int position, GridTile tile)
    {
        GridManager.Instance.RemoveObject(position);
    }
}
