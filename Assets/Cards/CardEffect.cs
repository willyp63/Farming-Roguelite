using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardEffect : ScriptableObject
{
    public virtual bool IsValidPlacement(Vector2Int position, GridTile tile)
    {
        // Default implementation - can be overridden by derived classes
        return tile != null;
    }

    public virtual void ApplyEffect(Vector2Int position, GridTile tile)
    {
        // Default implementation - can be overridden by derived classes
        Debug.Log($"Applying card effect at position {position}");
    }
}
