using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect : ScriptableObject
{
    public virtual bool IsValidPlacement(Vector2Int position, GridTile tile, Card card)
    {
        return tile != null && card.AllowedTileTypes.Contains(tile.Tile.TileType);
    }

    public virtual void ApplyEffect(Vector2Int position, GridTile tile, Card card) { }
}
